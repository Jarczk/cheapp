// Updated OffersController.cs
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

    public OffersController(IOfferAggregator agg, ISearchHistoryService searchHistory)
    {
        _agg = agg;
        _searchHistory = searchHistory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Offer>>> Get([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query cannot be empty");
        var res = await _agg.GetBestAsync(q, ct);

        if (User.Identity?.IsAuthenticated == true)
        {
            // Changed from ClaimTypes.NameIdentifier to JwtRegisteredClaimNames.Sub
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!string.IsNullOrEmpty(userId))
            {
                await _searchHistory.AddSearchAsync(userId, q, res.Count(), ct);
            }
        }
        return Ok(res);
    }
}