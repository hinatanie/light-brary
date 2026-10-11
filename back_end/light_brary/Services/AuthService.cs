using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Services.Interfaces;
using light_brary.Repositories.Interfaces;
using light_brary.Entities;

namespace light_brary.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
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

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var phone = request.PhoneNumber.Trim();
            if (await _userRepository.ExistsByPhoneNumberAsync(phone))
            {
                throw new ConflictException("Số điện thoại này đã được đăng ký.");
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                PhoneNumber = phone,
                Address = request.Address?.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            var defaultRole = await _roleRepository.GetByNameAsync(Role.USER);
            if (defaultRole is not null)
            {
                await _userRoleRepository.AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id
                });
            }

            var roles = defaultRole != null ? new List<string> { defaultRole.Name } : new List<string>();
            var token = _jwtTokenService.GenerateToken(user, roles);

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
                }
            };
        }
    }
}