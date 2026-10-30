using light_brary.Services;
using System.Security.Claims;
using light_brary.DTOs.Requests;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            await authService.LoginAsync(request);
            return new EmptyResult();
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await authService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpPut("phone-number")]
        [Authorize]
        public async Task<IActionResult> ChangePhoneNumber(ChangePhoneNumberRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await authService.ChangePhoneNumberAsync(userId, request);
            return Ok(new { message = "Đổi số điện thoại thành công." });
        }

        [HttpPut("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await authService.LogoutAsync(userId);
            return Ok(new { message = "Đăng xuất thành công." });
        }
    }
}
