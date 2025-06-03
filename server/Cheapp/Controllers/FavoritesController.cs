using Cheapp.Models;
using Cheapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cheapp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // This ensures all endpoints require authentication
public class FavoritesController : ControllerBase
{
    private readonly IFavoritesService _favorites;
    private readonly IOfferAggregator _offerAggregator;

    public FavoritesController(IFavoritesService favorites, IOfferAggregator offerAggregator)
    {
        _favorites = favorites;
        _offerAggregator = offerAggregator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FavoriteResponse>>> GetFavorites(CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Since [Authorize] is on the controller, this should never be null
        // But let's be safe and handle it anyway
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        var favorites = await _favorites.GetUserFavoritesAsync(userId, ct);
        return Ok(favorites);
    }

    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteDto dto, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        // For now, we'll create an offer from the OfferId
        // In a real implementation, you'd fetch the full offer details
        var offer = new Offer { Id = dto.OfferId };
        
        var favoriteId = await _favorites.AddFavoriteAsync(userId, offer, dto.Notes, ct);
        return Ok(new { favoriteId, message = "Added to favorites successfully" });
    }

    [HttpPost("from-offer")]
    public async Task<IActionResult> AddFavoriteFromOffer([FromBody] AddFavoriteFromOfferDto dto, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        var favoriteId = await _favorites.AddFavoriteAsync(userId, dto.Offer, dto.Notes, ct);
        return Ok(new { favoriteId, message = "Added to favorites successfully" });
    }

    [HttpDelete("{favoriteId}")]
    public async Task<IActionResult> RemoveFavorite(string favoriteId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        var removed = await _favorites.RemoveFavoriteAsync(userId, favoriteId, ct);
        
        if (!removed)
        {
            return NotFound("Favorite not found");
        }

        return Ok(new { message = "Removed from favorites successfully" });
    }

    [HttpPut("{favoriteId}/notes")]
    public async Task<IActionResult> UpdateNotes(string favoriteId, [FromBody] UpdateFavoriteNotesDto dto, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        var updated = await _favorites.UpdateFavoriteNotesAsync(userId, favoriteId, dto.Notes, ct);
        
        if (!updated)
        {
            return NotFound("Favorite not found");
        }

        return Ok(new { message = "Notes updated successfully" });
    }

    [HttpGet("check/{offerId}")]
    public async Task<ActionResult<bool>> IsFavorite(string offerId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        var isFavorite = await _favorites.IsFavoriteAsync(userId, offerId, ct);
        return Ok(new { isFavorite });
    }

    private async Task<Offer?> GetOfferById(string offerId, CancellationToken ct)
    {
        // This is a placeholder - implement based on how you store/retrieve offers
        // You might need to add this method to your IOfferAggregator interface
        throw new NotImplementedException("Implement offer retrieval by ID");
    }
}

// Move these to a separate file or Models folder
public record AddFavoriteFromOfferDto(Offer Offer, string? Notes);
public record AddFavoriteDto(string OfferId, string? Notes);
public record UpdateFavoriteNotesDto(string Notes);

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