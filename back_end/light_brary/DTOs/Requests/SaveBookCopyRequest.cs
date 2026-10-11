using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.DTOs.Requests
{
    public class SaveBookCopyRequest
    {
        [Required(ErrorMessage = "Mã vạch không được để trống.")]
        [StringLength(50, MinimumLength = 1)]
        public string Barcode { get; set; } = null!;

        [StringLength(255)]
        public string? ConditionNote { get; set; }
    }
}