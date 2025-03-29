using Cheapp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cheapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            // Tu tworzysz nowego użytkownika w Mongo:
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

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            // Dla przykładu logowanie za pomocą Identity, ale w Web API raczej wydajesz tokeny JWT
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("Invalid credentials");

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid) return Unauthorized("Invalid credentials");

            // Jeżeli chcesz wystawić token (JWT), to tu generujesz i zwracasz
            // a jeśli wolisz autentykację cookiem (mniej popularne w WEB API), to signInManager
            return Ok("Logged in!");
        }
    }
}

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}