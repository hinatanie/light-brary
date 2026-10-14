using light_brary.Services;
using light_brary.DTOs.Requests;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            await authService.LoginAsync(request);
            return new EmptyResult();
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            await authService.RegisterAsync(request);
            return new EmptyResult();
        }
    }
}
