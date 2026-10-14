using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.DTOs.Requests
{
    public record CreateBorrowRecordRequest
    {
        [Required(ErrorMessage = "Người mượn không được để trống.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Danh sách bản sao không được để trống.")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một bản sao.")]
        public List<Guid> BookCopyIds { get; set; } = new List<Guid>();

        [Required(ErrorMessage = "Hạn trả không được để trống.")]
        public DateTime DueDate { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }
    }
}