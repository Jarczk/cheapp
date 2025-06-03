using Cheapp.Models;

namespace Cheapp.Services;

// This implementation assumes you're using an in-memory collection or similar storage
// You can replace this with your actual data access pattern (MongoDB, Entity Framework, etc.)
public class FavoritesService : IFavoritesService
{
    private static readonly List<Favorite> _favorites = new();

    public Task<string> AddFavoriteAsync(string userId, Offer offer, string? notes = null, CancellationToken ct = default)
    {
        // Check if already favorited
        var existing = _favorites.FirstOrDefault(f => f.UserId == userId && f.OfferId == offer.Id);
        if (existing != null)
        {
            return Task.FromResult(existing.Id); // Already favorited
        }

        var favorite = new Favorite
        {
            UserId = userId,
            OfferId = offer.Id,
            ProductId = offer.ProductId,
            Marketplace = offer.Marketplace,
            Title = offer.Title,
            Price = offer.Price,
            Currency = offer.Currency,
            Url = offer.Url,
            Notes = notes
        };

        _favorites.Add(favorite);
        return Task.FromResult(favorite.Id);
    }

    public Task<IEnumerable<FavoriteResponse>> GetUserFavoritesAsync(string userId, CancellationToken ct = default)
    {
        var userFavorites = _favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.AddedAt)
            .Select(f => new FavoriteResponse
            {
                Id = f.Id,
                OfferId = f.OfferId,
                ProductId = f.ProductId,
                Marketplace = f.Marketplace,
                Title = f.Title,
                Price = f.Price,
                Currency = f.Currency,
                Url = f.Url,
                AddedAt = f.AddedAt,
                Notes = f.Notes
            });

        return Task.FromResult(userFavorites);
    }

    public Task<bool> RemoveFavoriteAsync(string userId, string favoriteId, CancellationToken ct = default)
    {
        var favorite = _favorites.FirstOrDefault(f => f.Id == favoriteId && f.UserId == userId);
        if (favorite != null)
        {
            _favorites.Remove(favorite);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> UpdateFavoriteNotesAsync(string userId, string favoriteId, string notes, CancellationToken ct = default)
    {
        var favorite = _favorites.FirstOrDefault(f => f.Id == favoriteId && f.UserId == userId);
        if (favorite != null)
        {
            favorite.Notes = notes;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> IsFavoriteAsync(string userId, string offerId, CancellationToken ct = default)
    {
        var exists = _favorites.Any(f => f.UserId == userId && f.OfferId == offerId);
        return Task.FromResult(exists);
    }
}
public class FavoriteResponse
{
    public string Id { get; set; } = string.Empty;
    public string OfferId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Marketplace { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Url { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public string? Notes { get; set; }
}