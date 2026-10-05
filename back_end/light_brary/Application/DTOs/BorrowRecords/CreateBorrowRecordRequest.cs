using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class CreateBorrowRecordRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Người mượn không hợp lệ.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Danh sách bản sao không được để trống.")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một bản sao.")]
        public List<int> BookCopyIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Hạn trả không được để trống.")]
        public DateTime DueDate { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }
    }
}