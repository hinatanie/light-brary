namespace Library.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    string UserId,
    string FullName,
    string Email,
    string PhoneNumber,
    DateTime ExpiresAt,
    List<string> Roles,
    bool MustChangePassword
);

/// <summary>
/// Kết quả xác thực nội bộ: phần <see cref="Response"/> trả về body, còn <see cref="RefreshToken"/>
/// được controller đặt vào cookie HttpOnly (không bao giờ nằm trong body JSON).
/// </summary>
public record AuthResult(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt);
