using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.DTOs.Requests
{
    public record UpdateUserRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(50, MinimumLength = 1)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [StringLength(10)]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(255)]
        public string Address { get; set; } = null!;
    }
}