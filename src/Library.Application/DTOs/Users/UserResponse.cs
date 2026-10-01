namespace Library.Application.DTOs.Users;

public record UserResponse(
    string Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? Address,
    DateTime CreatedAt
);
