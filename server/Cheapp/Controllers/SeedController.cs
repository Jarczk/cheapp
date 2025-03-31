using Cheapp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cheapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public SeedController(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpPost("roles")]
        public async Task<IActionResult> SeedRoles()
        {
            // Validate if roles exist, if not create them
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });

            return Ok("Roles seeded");
        }
    }
}
