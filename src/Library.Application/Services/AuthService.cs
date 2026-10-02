using Library.Application.DTOs.Auth;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Application.Interfaces.Authentication;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRoleRepository _roleRepository;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _roleRepository = roleRepository;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _userRepository.EmailExistsAsync(request.Email.ToLower().Trim());
        if (emailExists)
            throw new ConflictException($"Email '{request.Email}' đã được đăng ký trong hệ thống.");

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneExists = await _userRepository.PhoneNumberExistsAsync(request.PhoneNumber.Trim());
            if (phoneExists)
                throw new ConflictException($"Số điện thoại '{request.PhoneNumber}' đã được đăng ký trong hệ thống.");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email.ToLower().Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        var memberRole = await _roleRepository.GetByNameAsync("Member");
        if (memberRole != null)
        {
            user.UserRoles = new List<UserRole>
            {
                new UserRole { UserId = user.Id, RoleId = memberRole.Id, Role = memberRole }
            };
        }

        var created = await _userRepository.AddAsync(user);

        var token = _jwtTokenService.GenerateToken(created);
        var expiresAt = _jwtTokenService.GetExpiration();

        var roles = created.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>();

        return new AuthResponse(token, created.Id, created.FullName, created.Email, expiresAt, roles);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var identifier = request.PhoneNumber.Trim();

        // Ưu tiên tìm theo số điện thoại, nếu không tìm theo email
        var user = await _userRepository.GetByPhoneNumberAsync(identifier)
                   ?? await _userRepository.GetByEmailOrPhoneAsync(identifier)
                   ?? await _userRepository.GetByEmailAsync(identifier.ToLower());

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedException("Số điện thoại hoặc mật khẩu không chính xác.");

        var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new UnauthorizedException("Số điện thoại hoặc mật khẩu không chính xác.");

        var token = _jwtTokenService.GenerateToken(user);
        var expiresAt = _jwtTokenService.GetExpiration();

        var roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>();

        return new AuthResponse(token, user.Id, user.FullName, user.Email, expiresAt, roles);
    }

    public async Task<MeResponse> GetMeAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("Không tìm thấy thông tin người dùng.");

        var roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>();
        return new MeResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, roles);
    }
}
