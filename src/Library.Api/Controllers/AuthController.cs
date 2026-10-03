using System.Security.Claims;
using Library.Application.DTOs.Auth;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    public const string RefreshTokenCookieName = "lightbrary_refresh_token";

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng ký tài khoản mới cho độc giả
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return StatusCode(StatusCodes.Status201Created, result.Response);
    }

    /// <summary>
    /// Đăng nhập bằng số điện thoại hoặc email, nhận JWT token và HttpOnly refresh cookie
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.Response);
    }

    /// <summary>
    /// Xoay vòng refresh token và nhận access token mới
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        var result = await _authService.RefreshAsync(refreshToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.Response);
    }

    /// <summary>
    /// Đăng xuất: thu hồi refresh token và xóa cookie
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        await _authService.LogoutAsync(refreshToken);
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/auth"
        });
        return NoContent();
    }

    /// <summary>
    /// Đổi mật khẩu tài khoản
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.ChangePasswordAsync(userId, request);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.Response);
    }

    /// <summary>
    /// Lấy thông tin cá nhân của người dùng hiện tại
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MeResponse>> GetMe()
    {
        var userId = GetCurrentUserId();
        var response = await _authService.GetMeAsync(userId);
        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var guid))
            throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        return guid;
    }

    private void SetRefreshTokenCookie(string token, DateTime expiresAt)
    {
        Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !HostIsLocalhost(Request.Host.Host), // Cho phép HTTP khi chạy dev local
            SameSite = SameSiteMode.Lax, // Lax để hoạt động trơn tru với proxy/cross-port dev
            Expires = expiresAt,
            Path = "/api/v1/auth"
        });
    }

    private static bool HostIsLocalhost(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
}
