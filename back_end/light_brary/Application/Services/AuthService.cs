using light_brary.Application.DTOs.Auth;
using light_brary.Application.DTOs.Users;
using light_brary.Application.Exceptions;
using light_brary.Application.Interfaces;

namespace light_brary.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // 1. Tìm user theo số điện thoại (kèm roles)
            var user = await _userRepository.GetByPhoneNumberWithRolesAsync(request.PhoneNumber.Trim());

            // 2. Kiểm tra mật khẩu
            // Dùng chung 1 thông báo cho cả 2 trường hợp để không lộ việc SĐT có tồn tại hay không
            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Số điện thoại hoặc mật khẩu không chính xác.");
            }

            // 3. Sinh JWT
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var token = _jwtTokenService.GenerateToken(user, roles);

            // 4. Trả response
            return new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = _jwtTokenService.ExpiresInSeconds,
                User = new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Roles = roles,
                    CreatedAt = user.CreatedAt,
                },
            };
        }
    }
}