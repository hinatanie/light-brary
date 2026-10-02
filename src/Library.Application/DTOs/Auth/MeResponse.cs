namespace Library.Application.DTOs.Auth;

public record MeResponse(
    string UserId, 
    string FullName, 
    string Email, 
    string? PhoneNumber, 
    List<string> Roles
);
