using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Users;

public record UpdateUserRequest(
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [MaxLength(150, ErrorMessage = "Họ tên không được vượt quá 150 ký tự")]
    string FullName,

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    string PhoneNumber,

    [MaxLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
    string? Address
);
