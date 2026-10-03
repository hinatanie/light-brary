using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Auth;

public record ChangePasswordRequest(
    [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
    [MaxLength(128)]
    string CurrentPassword,

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MaxLength(128, ErrorMessage = "Mật khẩu quá dài")]
    string NewPassword
);
