using System.ComponentModel.DataAnnotations;

namespace light_brary.DTOs.Requests
{
    public record UpdateProfileRequest
    {
        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(255)]
        public string Address { get; set; } = null!;
    }
}
