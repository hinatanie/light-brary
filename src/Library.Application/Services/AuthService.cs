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

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _userRepository.EmailExistsAsync(request.Email.ToLower().Trim());
        if (emailExists)
            throw new ConflictException($"Email '{request.Email}' đã được đăng ký trong hệ thống.");

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

        var created = await _userRepository.AddAsync(user);

        var token = _jwtTokenService.GenerateToken(created);
        var expiresAt = _jwtTokenService.GetExpiration();

        return new AuthResponse(token, created.Id, created.FullName, created.Email, expiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower().Trim());
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedException("Email hoặc mật khẩu không đúng.");

        var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new UnauthorizedException("Email hoặc mật khẩu không đúng.");

        var token = _jwtTokenService.GenerateToken(user);
        var expiresAt = _jwtTokenService.GetExpiration();

        return new AuthResponse(token, user.Id, user.FullName, user.Email, expiresAt);
    }
}
