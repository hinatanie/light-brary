using System.ComponentModel.DataAnnotations;
namespace Library.Application.DTOs.Users;
public record UpdateUserRequest(
    [Required][MaxLength(150)] string FullName,
    [MaxLength(50)] string? PhoneNumber,
    [MaxLength(255)] string? Address
);
