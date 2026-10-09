using System.ComponentModel.DataAnnotations;

namespace light_brary.Application.DTOs.Users
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(150, MinimumLength = 1)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [StringLength(10)]
        public string PhoneNumber { get; set; } = null!;

        [StringLength(255)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string Password { get; set; } = null!;
    }
}

