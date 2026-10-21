using System.ComponentModel.DataAnnotations;

namespace light_brary.DTOs.Requests
{
    public record LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(256)]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string Password { get; set; } = null!;
    }
}
