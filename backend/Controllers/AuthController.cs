using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenService _tokenService;

        public AuthController(UserManager<User> userManager, IJwtTokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            Console.WriteLine($"Received username: {dto.UserName}");
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return BadRequest(ModelState);
            }
            //check same name
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null)
            {
                return BadRequest(new { Message = "Username already exists" });
            }
            var user = new User { UserName = dto.UserName };
            //var user = new User { Username = dto.Username };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            //return BadRequest(result.Errors);
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error Code: {error.Code}, Description: {error.Description}");
                }
                return BadRequest(result.Errors);

            }

            return Ok(new {Message = "User registered successfully", UserId = user.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { Message = "Invalid username or password"});

            var token = _tokenService.GenerateToken(user);
            if (string.IsNullOrEmpty(token))
            {
                return StatusCode(500, new { Message = "Token generation failed" });
            }
            return Ok(new { Token = token });
        }
    }
}
