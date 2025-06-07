using Cheapp.Models;
using Cheapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cheapp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchHistoryController : ControllerBase
{
    private readonly ISearchHistoryService _searchHistory;

    public SearchHistoryController(ISearchHistoryService searchHistory)
    {
        _searchHistory = searchHistory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SearchHistoryResponse>>> GetHistory(
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        var history = await _searchHistory.GetUserHistoryAsync(userId, limit, ct);
        return Ok(history);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearHistory(CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        await _searchHistory.ClearUserHistoryAsync(userId, ct);
        return Ok(new { message = "Search history cleared successfully" });
    }

    [HttpDelete("{searchId}")]
    public async Task<IActionResult> DeleteSearch(string searchId, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }
        
        await _searchHistory.DeleteSearchAsync(userId, searchId, ct);
        return Ok(new { message = "Search deleted successfully" });
    }
}
public class SearchHistoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; }
    public int ResultCount { get; set; }
}
