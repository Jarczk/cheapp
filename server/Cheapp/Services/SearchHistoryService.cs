using Cheapp.Models;

namespace Cheapp.Services;

// This implementation uses in-memory storage for simplicity
// You can replace this with your actual data access pattern (MongoDB, Entity Framework, etc.)
public class SearchHistoryService : ISearchHistoryService
{
    private static readonly List<SearchHistory> _searches = new();

    public Task AddSearchAsync(string userId, string query, int resultCount, CancellationToken ct = default)
    {
        // Check if the same query was searched recently (within last 5 minutes)
        var recentSearch = _searches
            .FirstOrDefault(s => s.UserId == userId && s.Query == query && s.SearchedAt > DateTime.UtcNow.AddMinutes(-5));

        if (recentSearch != null)
        {
            // Update the existing search instead of creating a new one
            recentSearch.SearchedAt = DateTime.UtcNow;
            recentSearch.ResultCount = resultCount;
            return Task.CompletedTask;
        }

        var search = new SearchHistory
        {
            UserId = userId,
            Query = query,
            ResultCount = resultCount
        };

        _searches.Add(search);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<SearchHistoryResponse>> GetUserHistoryAsync(string userId, int limit = 50, CancellationToken ct = default)
    {
        var searches = _searches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SearchedAt)
            .Take(limit)
            .Select(s => new SearchHistoryResponse
            {
                Id = s.Id,
                Query = s.Query,
                SearchedAt = s.SearchedAt,
                ResultCount = s.ResultCount
            });

        return Task.FromResult(searches);
    }

    public Task ClearUserHistoryAsync(string userId, CancellationToken ct = default)
    {
        _searches.RemoveAll(s => s.UserId == userId);
        return Task.CompletedTask;
    }

    public Task DeleteSearchAsync(string userId, string searchId, CancellationToken ct = default)
    {
        _searches.RemoveAll(s => s.Id == searchId && s.UserId == userId);
        return Task.CompletedTask;
    }
}
