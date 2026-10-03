using Library.Application.Common;
using Library.Application.DTOs.Auth;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Application.Interfaces.Authentication;
using Library.Application.Validation;
using Library.Domain.Constants;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Số điện thoại hoặc mật khẩu không chính xác.";
    private const string InvalidSessionMessage = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = PhoneNumberNormalizer.Normalize(request.PhoneNumber);
        PasswordPolicy.EnsureValid(request.Password, phone, email);

        // Kiểm tra trước để trả lỗi thân thiện; ràng buộc unique ở DB mới là lớp bảo vệ thật sự khi có request đồng thời.
        if (await _userRepository.EmailExistsAsync(email))
            throw new ConflictException($"Email '{email}' đã được đăng ký trong hệ thống.");

        if (await _userRepository.PhoneNumberExistsAsync(phone))
            throw new ConflictException($"Số điện thoại '{phone}' đã được đăng ký trong hệ thống.");

        var memberRole = await _roleRepository.GetByNameAsync(RoleNames.Member)
            ?? throw new InvalidOperationException("Role 'Member' chưa được seed trong cơ sở dữ liệu.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = request.FullName.Trim(),
            PhoneNumber = phone,
            Address = request.Address?.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = memberRole.Id, Role = memberRole });

        var created = await _userRepository.AddAsync(user);
        return await IssueTokensAsync(created);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var identifier = request.PhoneNumber.Trim();

        User? user;
        if (identifier.Contains('@'))
        {
            user = await _userRepository.GetByEmailAsync(identifier.ToLowerInvariant());
        }
        else
        {
            user = PhoneNumberNormalizer.TryNormalize(identifier, out var phone)
                ? await _userRepository.GetByPhoneNumberAsync(phone)
                : null;
        }

        var now = DateTime.UtcNow;

        if (user?.LockoutEndUtc is { } lockoutEnd && lockoutEnd > now)
            throw CreateLockedException(lockoutEnd, now);

        // Luôn chạy Verify (kể cả khi user == null) để cân bằng thời gian phản hồi.
        var isPasswordValid = _passwordHasher.Verify(request.Password, user?.PasswordHash);
        if (user == null || !isPasswordValid)
        {
            if (user != null)
                await RegisterFailedAttemptAsync(user, now);

            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        if (user.FailedLoginCount != 0 || user.LockoutEndUtc != null)
        {
            user.FailedLoginCount = 0;
            user.LockoutEndUtc = null;
            await _userRepository.UpdateUserAsync(user);
        }

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResult> RefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedException(InvalidSessionMessage);

        var stored = await _refreshTokenRepository.GetByHashAsync(RefreshTokenGenerator.Hash(refreshToken));
        if (stored == null)
            throw new UnauthorizedException(InvalidSessionMessage);

        var now = DateTime.UtcNow;

        if (stored.RevokedAt is { } revokedAt)
        {
            // Token đã xoay mà vẫn được dùng lại: nếu ngoài khoảng ân hạn thì coi là bị đánh cắp, thu hồi cả chuỗi.
            if (now - revokedAt > AuthConstants.RefreshReuseGrace)
                await _refreshTokenRepository.RevokeFamilyAsync(stored.FamilyId, now);

            throw new UnauthorizedException(InvalidSessionMessage);
        }

        if (stored.ExpiresAt <= now)
            throw new UnauthorizedException(InvalidSessionMessage);

        var user = await _userRepository.GetByIdAsync(stored.UserId)
            ?? throw new UnauthorizedException(InvalidSessionMessage);

        if (user.LockoutEndUtc is { } lockoutEnd && lockoutEnd > now)
            throw CreateLockedException(lockoutEnd, now);

        var newTokenId = Guid.NewGuid();
        if (!await _refreshTokenRepository.TryRevokeAsync(stored.Id, now, newTokenId))
            throw new UnauthorizedException(InvalidSessionMessage); // request song song đã xoay token này

        return await IssueTokensAsync(user, stored.FamilyId, newTokenId);
    }

    public async Task LogoutAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;

        var stored = await _refreshTokenRepository.GetByHashAsync(RefreshTokenGenerator.Hash(refreshToken));
        if (stored != null)
            await _refreshTokenRepository.RevokeFamilyAsync(stored.FamilyId, DateTime.UtcNow);
    }

    public async Task<AuthResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("Không tìm thấy thông tin người dùng.");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BusinessException("Mật khẩu hiện tại không chính xác.");

        if (request.NewPassword == request.CurrentPassword)
            throw new BusinessException("Mật khẩu mới phải khác mật khẩu hiện tại.");

        PasswordPolicy.EnsureValid(request.NewPassword, user.PhoneNumber, user.Email);

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;
        await _userRepository.UpdateUserAsync(user);

        // Đổi mật khẩu thì mọi phiên cũ phải đăng nhập lại.
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, DateTime.UtcNow);

        return await IssueTokensAsync(user);
    }

    public async Task<MeResponse> GetMeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("Không tìm thấy thông tin người dùng.");

        return new MeResponse(
            user.Id.ToString(),
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Address,
            GetRoleNames(user),
            user.MustChangePassword);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task RegisterFailedAttemptAsync(User user, DateTime now)
    {
        // Khóa cũ đã hết hạn thì bắt đầu đếm lại từ đầu.
        if (user.LockoutEndUtc is { } previousEnd && previousEnd <= now)
        {
            user.LockoutEndUtc = null;
            user.FailedLoginCount = 0;
        }

        user.FailedLoginCount++;
        if (user.FailedLoginCount >= AuthConstants.MaxFailedLoginAttempts)
        {
            user.LockoutEndUtc = now.Add(AuthConstants.LockoutDuration);
            user.FailedLoginCount = 0;
        }

        await _userRepository.UpdateUserAsync(user);
    }

    private static LockedException CreateLockedException(DateTime lockoutEndUtc, DateTime now)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling((lockoutEndUtc - now).TotalMinutes));
        return new LockedException(
            $"Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau {minutes} phút.",
            lockoutEndUtc);
    }

    private static List<string> GetRoleNames(User user) =>
        user.UserRoles
            .Select(ur => ur.Role?.Name)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .ToList();

    private async Task<AuthResult> IssueTokensAsync(User user, Guid? familyId = null, Guid? tokenId = null)
    {
        var access = _jwtTokenService.GenerateToken(user);

        var now = DateTime.UtcNow;
        var rawRefreshToken = RefreshTokenGenerator.Generate();
        var refreshEntity = new RefreshToken
        {
            Id = tokenId ?? Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = RefreshTokenGenerator.Hash(rawRefreshToken),
            FamilyId = familyId ?? Guid.NewGuid(),
            CreatedAt = now,
            ExpiresAt = now.Add(AuthConstants.RefreshTokenLifetime)
        };
        await _refreshTokenRepository.AddAsync(refreshEntity);

        var response = new AuthResponse(
            access.Token,
            user.Id.ToString(),
            user.FullName,
            user.Email,
            user.PhoneNumber,
            access.ExpiresAt,
            GetRoleNames(user),
            user.MustChangePassword);

        return new AuthResult(response, rawRefreshToken, refreshEntity.ExpiresAt);
    }
}
