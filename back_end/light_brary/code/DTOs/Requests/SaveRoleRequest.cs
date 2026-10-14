using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.DTOs.Requests
{
    public record SaveRoleRequest
    {
        [Required(ErrorMessage = "Tên vai trò không được để trống.")]
        [StringLength(256, MinimumLength = 1)]
        public string Name { get; set; } = null!;
    }
}