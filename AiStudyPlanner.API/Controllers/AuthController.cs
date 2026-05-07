using AiStudyPlanner.API.Contracts.Auth;
using AiStudyPlanner.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiStudyPlanner.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await _authService.RegisterAsync(request.Username, request.Email, request.Password);

                return Created(string.Empty, new
                {
                    message = "User registered successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Email, request.Password);

                return Ok(new LoginResponse
                {
                    Token = result.Token,
                    User = new AuthUserResponse
                    {
                        Id = result.User.Id,
                        Username = result.User.UserName,
                        Email = result.User.Email,
                        Role = result.User.Role,
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out int userId))
                return Unauthorized();

            return Ok(new AuthUserResponse
            {
                Id = userId,
                Username = username ?? string.Empty,
                Email = email ?? string.Empty,
                Role = role ?? "User"
            });
        }

        public record RegisterRequest(string Username,string Email, string Password);
        public record LoginRequest(string Email, string Password);
    }
}
