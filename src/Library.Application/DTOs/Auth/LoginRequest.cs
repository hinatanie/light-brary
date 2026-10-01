using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Auth;

public record LoginRequest(
    [Required(ErrorMessage = "Email hoặc Số điện thoại không được để trống")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    string Password
);
