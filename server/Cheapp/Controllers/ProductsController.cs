using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] //[Authorize(Roles = "Admin")] - for admin access only
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        // Only logged user can access this page
        return Ok("Secured list of products...");
    }

 
}

