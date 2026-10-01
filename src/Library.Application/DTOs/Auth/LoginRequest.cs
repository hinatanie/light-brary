using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Auth;

public record LoginRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    string Password
);
