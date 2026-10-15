using System.ComponentModel.DataAnnotations;

namespace light_brary.DTOs.Requests
{
    public record ChangePhoneNumberRequest
    {
        [Required(ErrorMessage = "Số điện thoại mới không được để trống.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có đúng 10 chữ số.")]
        [RegularExpression(@"[0-9]{10}", ErrorMessage = "Số điện thoại phải có đúng 10 chữ số.")]
        public string NewPhoneNumber { get; set; } = null!;
    }
}
