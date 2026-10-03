using Library.Application.DTOs.Auth;

namespace Library.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshAsync(string? refreshToken);
    Task LogoutAsync(string? refreshToken);
    Task<AuthResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<MeResponse> GetMeAsync(Guid userId);
}
