using Cheapp.Models;

namespace Cheapp.Services;

public interface IFavoritesService
{
    Task<string> AddFavoriteAsync(string userId, string productId, CancellationToken ct = default);
    Task<IEnumerable<FavoriteResponse>> GetUserFavoritesAsync(string userId, CancellationToken ct = default);
    Task<IEnumerable<Offer>> GetUserFavoriteOffersAsync(string userId, CancellationToken ct = default);
    Task<bool> RemoveFavoriteByProductIdAsync(string userId, string productId, CancellationToken ct = default);
    Task<bool> IsFavoriteAsync(string userId, string productId, CancellationToken ct = default);
    Task<bool> UpdateFavoriteNotesAsync(string userId, string favoriteId, string notes, CancellationToken ct = default);
}