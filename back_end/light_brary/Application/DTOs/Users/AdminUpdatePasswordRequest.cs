using System.ComponentModel.DataAnnotations;

namespace light_brary.Application.DTOs.Users
{
    public class AdminUpdatePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        [StringLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự.")]
        public string NewPassword { get; set; } = null!;
    }
}

