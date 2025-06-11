using Cheapp.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cheapp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        var adminIds = adminUsers.Select(a => a.Id).ToHashSet();

        var users = _userManager.Users
            .Where(u => !adminIds.Contains(u.Id))
            .Select(u => new UserDto
            {
                Id = u.Id.ToString(),
                Email = u.Email ?? string.Empty,
                UserName = u.UserName
            })
            .ToList();
        return Ok(users);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserName { get; set; }
    }
}