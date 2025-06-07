using Cheapp.Models;
using Cheapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Cheapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly IOfferAggregator _agg;
    private readonly ISearchHistoryService _searchHistory;
    private readonly ILogger<OffersController> _logger;

    public OffersController(
        IOfferAggregator agg, 
        ISearchHistoryService searchHistory,
        ILogger<OffersController> logger)
    {
        _agg = agg;
        _searchHistory = searchHistory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Offer>>> Get([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query cannot be empty");
        
        var res = await _agg.GetBestAsync(q, ct);
        var resultCount = res.Count();

        _logger.LogInformation($"Search performed: Query='{q}', Results={resultCount}");
        _logger.LogInformation($"User.Identity?.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        
        if (User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("User is authenticated. Claims:");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation($"  {claim.Type}: {claim.Value}");
            }
        }

        string? userId = null;
        
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.FindFirstValue("sub") 
                     ?? User.FindFirstValue("user_id")
                     ?? User.FindFirstValue("id");

            _logger.LogInformation($"Extracted userId: '{userId}'");

            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    _logger.LogInformation($"Saving search history: UserId='{userId}', Query='{q}', Count={resultCount}");
                    await _searchHistory.AddSearchAsync(userId, q, resultCount, ct);
                    _logger.LogInformation("Search history saved successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save search history");
                }
            }
            else
            {
                _logger.LogWarning("No valid userId found in claims");
            }
        }
        else
        {
            _logger.LogInformation("User is not authenticated, skipping search history");
        }

        return Ok(res);
    }
}