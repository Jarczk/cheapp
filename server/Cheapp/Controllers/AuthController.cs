using Cheapp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Cheapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtOptions _jwt;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtOptions> jwt)            // <-- tu!
        {
            _userManager = userManager;
            _jwt = jwt.Value ?? throw new InvalidOperationException("Jwt section missing");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // Creating user in MongoDb
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "user");

            return Ok("User registered successfully");
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok("Admin user registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (string.IsNullOrEmpty(model.Email)) 
                return BadRequest("Email is required");
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) 
                return Unauthorized("Email not found");
            
            if (string.IsNullOrEmpty(model.Password))
                return BadRequest("Password is required");
            
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid) 
                return Unauthorized("Incorrect password");
            
            var token = GenerateJwtToken(user);
            return Ok(new { access_token = token });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }
            return Ok(new { message = "Logout successful" });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(_jwt.Key))
                throw new InvalidOperationException("Jwt:Key not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!)
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

public class RegisterDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
public class LoginDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}