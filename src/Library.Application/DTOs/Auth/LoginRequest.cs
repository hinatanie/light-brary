using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Auth;

public record LoginRequest(
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    string PhoneNumber,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    string Password
);
