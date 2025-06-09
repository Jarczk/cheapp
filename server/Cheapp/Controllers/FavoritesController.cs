using Cheapp.Models;
using Cheapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Cheapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoritesService _favoritesService;

        public FavoritesController(IFavoritesService favoritesService)
        {
            _favoritesService = favoritesService;
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var favoriteId = await _favoritesService.AddFavoriteAsync(userId, request.ProductId);
                return Ok(new { Id = favoriteId, Message = "Favorite added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding favorite: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var favorites = await _favoritesService.GetUserFavoritesAsync(userId);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving favorites: {ex.Message}");
            }
        }

        [HttpGet("full")]
        public async Task<IActionResult> GetFavoriteOffers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
    
            var offers = await _favoritesService.GetUserFavoriteOffersAsync(userId);
            return Ok(offers);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFavoriteByProductId(string productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var removed = await _favoritesService.RemoveFavoriteByProductIdAsync(userId, productId);
                if (removed)
                    return Ok(new { Message = "Favorite removed successfully" });
                else
                    return NotFound("Favorite not found");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error removing favorite: {ex.Message}");
            }
        }

        [HttpGet("check/{productId}")]
        public async Task<IActionResult> CheckIfFavorite(string productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var isFavorite = await _favoritesService.IsFavoriteAsync(userId, productId);
                return Ok(new { IsFavorite = isFavorite });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error checking favorite: {ex.Message}");
            }
        }
    }

    // Request DTOs
    public class AddFavoriteRequest
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;
    }
}