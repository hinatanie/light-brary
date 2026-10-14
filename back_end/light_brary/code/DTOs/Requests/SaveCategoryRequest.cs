using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.DTOs.Requests
{
    public record SaveCategoryRequest
    {
        [Required(ErrorMessage = "Tên thể loại không được để trống.")]
        [StringLength(150, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }
    }
}