using light_brary.Application.DTOs.Auth;

namespace light_brary.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}