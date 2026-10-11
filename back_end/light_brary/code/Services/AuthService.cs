using light_brary.Services.Authentication;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Services
{
    public class AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        UserService userService,
        JwtTokenService jwtTokenService)
    {
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await userManager.FindByNameAsync(request.PhoneNumber.Trim());
            if (user is null)
                throw new UnauthorizedException("Số điện thoại hoặc mật khẩu không chính xác.");

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                throw new UnauthorizedException("Số điện thoại hoặc mật khẩu không chính xác.");

            var response = await userService.GetByIdAsync(user.Id);
            return CreateAuthResponse(user, response);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var response = await userService.CreateAsync(new CreateUserRequest
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Password = request.Password
            });
            var user = await userManager.FindByIdAsync(response.Id.ToString())
                ?? throw new NotFoundException("Không tìm thấy tài khoản vừa tạo.");
            return CreateAuthResponse(user, response);
        }

        private AuthResponse CreateAuthResponse(User user, UserResponse response)
        {
            return new AuthResponse
            {
                AccessToken = jwtTokenService.GenerateToken(user, response.Roles),
                TokenType = "Bearer",
                ExpiresIn = jwtTokenService.ExpiresInSeconds,
                User = response
            };
        }
    }
}
