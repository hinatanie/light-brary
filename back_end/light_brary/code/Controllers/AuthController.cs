using light_brary.Services;
using light_brary.DTOs.Requests;
using light_brary.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // api/auth
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // [ApiController] tự trả 400 nếu request vi phạm [Required]/[StringLength]
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
