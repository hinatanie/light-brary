namespace Library.Application.DTOs.Users;

public record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string? Address,
    DateTime CreatedAt,
    List<string> Roles
);
