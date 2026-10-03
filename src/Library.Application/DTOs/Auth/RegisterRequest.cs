using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Auth;

public record RegisterRequest(
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [MaxLength(150, ErrorMessage = "Họ tên không được vượt quá 150 ký tự")]
    string FullName,

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [MaxLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MaxLength(128, ErrorMessage = "Mật khẩu quá dài")]
    string Password,

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    string PhoneNumber,

    [MaxLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
    string? Address
);
