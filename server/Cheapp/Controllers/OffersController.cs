using Cheapp.Models;
using Cheapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cheapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly IOfferAggregator _agg;
    public OffersController(IOfferAggregator agg) => _agg = agg;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Offer>>> Get([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query cannot be empty");
        var res = await _agg.GetBestAsync(q, ct);
        return Ok(res);
    }
}