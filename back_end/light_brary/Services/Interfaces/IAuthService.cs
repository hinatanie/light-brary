using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;

namespace light_brary.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
    }
}