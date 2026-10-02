namespace Library.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    string UserId,
    string FullName,
    string Email,
    DateTime ExpiresAt,
    List<string> Roles
);
