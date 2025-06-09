using Cheapp.Models;
using Cheapp.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheapp.Services;

public class MongoFavoritesService : IFavoritesService
{
    private readonly IMongoCollection<Favorite> _favoritesCollection;
    private readonly ILogger<MongoFavoritesService> _logger;

    public MongoFavoritesService(IOptions<MongoOptions> mongoOptions, ILogger<MongoFavoritesService> logger)
    {
        _logger = logger;

        try
        {
            var mongoClient = new MongoClient(mongoOptions.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoOptions.Value.Database);
            _favoritesCollection = mongoDatabase.GetCollection<Favorite>("Favorites");

            _logger.LogInformation("Connected to MongoDB database: {Database}", mongoOptions.Value.Database);

            // Create indexes for better performance
            CreateIndexes();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MongoDB");
            throw;
        }
    }

    private void CreateIndexes()
    {
        try
        {
            // Index for fast user lookup
            var userIdIndex = Builders<Favorite>.IndexKeys.Ascending(f => f.UserId);
            _favoritesCollection.Indexes.CreateOne(new CreateIndexModel<Favorite>(userIdIndex));

            // Unique index for user-product pairs to prevent duplicates
            var userProductIndex = Builders<Favorite>.IndexKeys
                .Ascending(f => f.UserId)
                .Ascending(f => f.ProductId);
            _favoritesCollection.Indexes.CreateOne(
                new CreateIndexModel<Favorite>(userProductIndex, new CreateIndexOptions { Unique = true }));

            _logger.LogInformation("MongoDB indexes created successfully");
        }
        catch (MongoCommandException ex)
        {
            _logger.LogWarning(ex, "Index creation failed (might already exist)");
        }
    }

    public async Task<string> AddFavoriteAsync(string userId, string productId, CancellationToken ct = default)
    {
        _logger.LogInformation("Adding favorite for user {UserId}, product {ProductId}", userId, productId);

        var favorite = new Favorite
        {
            UserId = userId,
            ProductId = productId
        };

        try
        {
            await _favoritesCollection.InsertOneAsync(favorite, cancellationToken: ct);
            _logger.LogInformation("Favorite added successfully with ID: {FavoriteId}", favorite.Id);
            return favorite.Id;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning("Duplicate favorite detected for user {UserId}, product {ProductId}", userId, productId);

            // Handle duplicate (already exists in favorites)
            var existing = await _favoritesCollection
                .Find(f => f.UserId == userId && f.ProductId == productId)
                .FirstOrDefaultAsync(ct);

            return existing?.Id ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add favorite for user {UserId}, product {ProductId}", userId, productId);
            throw;
        }
    }

    public async Task<IEnumerable<FavoriteResponse>> GetUserFavoritesAsync(string userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting favorites for user {UserId}", userId);

        try
        {
            var favorites = await _favoritesCollection
                .Find(f => f.UserId == userId)
                .SortByDescending(f => f.AddedAt)
                .ToListAsync(ct);

            _logger.LogInformation("Found {Count} favorites for user {UserId}", favorites.Count, userId);

            return favorites.Select(f => new FavoriteResponse
            {
                Id = f.Id,
                ProductId = f.ProductId,
                AddedAt = f.AddedAt,
                Notes = f.Notes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get favorites for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RemoveFavoriteByProductIdAsync(string userId, string productId, CancellationToken ct = default)
    {
        var filter = Builders<Favorite>.Filter.And(
            Builders<Favorite>.Filter.Eq(f => f.UserId, userId),
            Builders<Favorite>.Filter.Eq(f => f.ProductId, productId) // or f.OfferId depending on your model
        );
        
        var result = await _favoritesCollection.DeleteOneAsync(filter, ct);
        return result.DeletedCount > 0;
    }

    public async Task<bool> IsFavoriteAsync(string userId, string productId, CancellationToken ct = default)
    {
        try
        {
            var count = await _favoritesCollection
                .CountDocumentsAsync(f => f.UserId == userId && f.ProductId == productId, cancellationToken: ct);

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if favorite exists for user {UserId}, product {ProductId}", userId, productId);
            throw;
        }
    }

    public async Task<bool> UpdateFavoriteNotesAsync(string userId, string favoriteId, string notes, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating notes for favorite {FavoriteId} for user {UserId}", favoriteId, userId);

        try
        {
            var update = Builders<Favorite>.Update.Set(f => f.Notes, notes);
            var result = await _favoritesCollection.UpdateOneAsync(
                f => f.Id == favoriteId && f.UserId == userId,
                update,
                cancellationToken: ct);

            bool success = result.ModifiedCount > 0;
            _logger.LogInformation("Update favorite notes result: {Success}", success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update notes for favorite {FavoriteId} for user {UserId}", favoriteId, userId);
            throw;
        }
    }
}

public class FavoriteResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}