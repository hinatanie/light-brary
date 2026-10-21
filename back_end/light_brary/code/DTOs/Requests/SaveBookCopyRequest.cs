using System.ComponentModel.DataAnnotations;

namespace light_brary.DTOs.Requests
{
    public record SaveBookCopyRequest
    {
        [Required(ErrorMessage = "Mã vạch không được để trống.")]
        [StringLength(50, MinimumLength = 1)]
        public string Barcode { get; set; } = null!;

        [StringLength(255)]
        public string? ConditionNote { get; set; }
    }
}