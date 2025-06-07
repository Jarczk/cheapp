using Cheapp.Models;
using MongoDB.Driver;

namespace Cheapp.Services;

public class SearchHistoryService : ISearchHistoryService
{
    private readonly IMongoCollection<SearchHistory> _searchHistoryCollection;
    private const int MAX_SEARCHES_PER_USER = 50;

    public SearchHistoryService(IMongoDatabase database)
    {
        _searchHistoryCollection = database.GetCollection<SearchHistory>("searchhistory");
        

        var indexKeysDefinition = Builders<SearchHistory>.IndexKeys
            .Ascending(x => x.UserId)
            .Ascending(x => x.SearchedAt);
        _searchHistoryCollection.Indexes.CreateOneAsync(new CreateIndexModel<SearchHistory>(indexKeysDefinition));
    }

    public async Task AddSearchAsync(string userId, string query, int resultCount, CancellationToken ct = default)
    {
        Console.WriteLine($"[SearchHistoryService] AddSearchAsync called: UserId='{userId}', Query='{query}', ResultCount={resultCount}");
        
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
        var recentSearchFilter = Builders<SearchHistory>.Filter.And(
            Builders<SearchHistory>.Filter.Eq(s => s.UserId, userId),
            Builders<SearchHistory>.Filter.Eq(s => s.Query, query),
            Builders<SearchHistory>.Filter.Gt(s => s.SearchedAt, fiveMinutesAgo)
        );

        var recentSearch = await _searchHistoryCollection.Find(recentSearchFilter).FirstOrDefaultAsync(ct);

        if (recentSearch != null)
        {
            Console.WriteLine($"[SearchHistoryService] Found recent search, updating existing record");

            var updateDefinition = Builders<SearchHistory>.Update
                .Set(s => s.SearchedAt, DateTime.UtcNow)
                .Set(s => s.ResultCount, resultCount);

            await _searchHistoryCollection.UpdateOneAsync(
                Builders<SearchHistory>.Filter.Eq(s => s.Id, recentSearch.Id),
                updateDefinition,
                cancellationToken: ct
            );
            Console.WriteLine($"[SearchHistoryService] Updated existing search successfully");
            return;
        }

        var userFilter = Builders<SearchHistory>.Filter.Eq(s => s.UserId, userId);
        var currentCount = await _searchHistoryCollection.CountDocumentsAsync(userFilter, cancellationToken: ct);

        Console.WriteLine($"[SearchHistoryService] Current search count for user: {currentCount}");

        if (currentCount >= MAX_SEARCHES_PER_USER)
        {
            var oldestSearchFilter = Builders<SearchHistory>.Filter.Eq(s => s.UserId, userId);
            var oldestSearchSort = Builders<SearchHistory>.Sort.Ascending(s => s.SearchedAt);
            
            var oldestSearch = await _searchHistoryCollection
                .Find(oldestSearchFilter)
                .Sort(oldestSearchSort)
                .FirstOrDefaultAsync(ct);

            if (oldestSearch != null)
            {
                await _searchHistoryCollection.DeleteOneAsync(
                    Builders<SearchHistory>.Filter.Eq(s => s.Id, oldestSearch.Id),
                    ct
                );
            }
        }

        var newSearch = new SearchHistory
        {
            UserId = userId,
            Query = query,
            ResultCount = resultCount
        };

        await _searchHistoryCollection.InsertOneAsync(newSearch, cancellationToken: ct);
        Console.WriteLine($"[SearchHistoryService] New search inserted successfully with ID: {newSearch.Id}");
    }

    public async Task<IEnumerable<SearchHistoryResponse>> GetUserHistoryAsync(string userId, int limit = 50, CancellationToken ct = default)
    {
        var filter = Builders<SearchHistory>.Filter.Eq(s => s.UserId, userId);
        var sort = Builders<SearchHistory>.Sort.Descending(s => s.SearchedAt);

        var searches = await _searchHistoryCollection
            .Find(filter)
            .Sort(sort)
            .Limit(limit)
            .ToListAsync(ct);

        return searches.Select(s => new SearchHistoryResponse
        {
            Id = s.Id,
            Query = s.Query,
            SearchedAt = s.SearchedAt,
            ResultCount = s.ResultCount
        });
    }

    public async Task ClearUserHistoryAsync(string userId, CancellationToken ct = default)
    {
        var filter = Builders<SearchHistory>.Filter.Eq(s => s.UserId, userId);
        await _searchHistoryCollection.DeleteManyAsync(filter, ct);
    }

    public async Task DeleteSearchAsync(string userId, string searchId, CancellationToken ct = default)
    {
        var filter = Builders<SearchHistory>.Filter.And(
            Builders<SearchHistory>.Filter.Eq(s => s.Id, searchId),
            Builders<SearchHistory>.Filter.Eq(s => s.UserId, userId)
        );
        await _searchHistoryCollection.DeleteOneAsync(filter, ct);
    }

    // Optional: Method to get current search count for a user (for debugging/monitoring)
    public async Task<int> GetUserSearchCountAsync(string userId, CancellationToken ct = default)
    {
        var filter = Builders<SearchHistory>.Filter.Eq(s => s.UserId, userId);
        var count = await _searchHistoryCollection.CountDocumentsAsync(filter, cancellationToken: ct);
        return (int)count;
    }
}