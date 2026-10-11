using light_brary.Services.Authentication;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly UserService _userService;
        private readonly JwtTokenService _jwtTokenService;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager,
            UserService userService, JwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.PhoneNumber.Trim());
            if (user is null)
                throw new UnauthorizedException("Số điện thoại hoặc mật khẩu không chính xác.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                throw new UnauthorizedException("Số điện thoại hoặc mật khẩu không chính xác.");

            var response = await _userService.GetByIdAsync(user.Id);
            return CreateAuthResponse(user, response);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var response = await _userService.CreateAsync(new CreateUserRequest
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Password = request.Password
            });
            var user = await _userManager.FindByIdAsync(response.Id.ToString())
                ?? throw new NotFoundException("Không tìm thấy tài khoản vừa tạo.");
            return CreateAuthResponse(user, response);
        }

        private AuthResponse CreateAuthResponse(User user, UserResponse response)
        {
            return new AuthResponse
            {
                AccessToken = _jwtTokenService.GenerateToken(user, response.Roles),
                TokenType = "Bearer",
                ExpiresIn = _jwtTokenService.ExpiresInSeconds,
                User = response
            };
        }
    }
}
