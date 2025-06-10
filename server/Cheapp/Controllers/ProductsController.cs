using Cheapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cheapp.Services;
using Cheapp.Models;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IEbayClient _ebayClient;

    public ProductsController(IEbayClient ebayClient)
    {
        _ebayClient = ebayClient;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        // Only logged user can access this page
        return Ok("Secured list of products...");
    }

    [HttpGet("{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string productId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(productId)) return BadRequest("Product ID cannot be empty");

        var product = await _ebayClient.GetByIdAsync(productId, ct);
        if (product == null) return NotFound();

        return Ok(product);
    }
}