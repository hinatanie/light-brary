using System;
using System.ComponentModel.DataAnnotations;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class MemberBorrowRequest
    {
        [Required(ErrorMessage = "Mã đầu sách không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã đầu sách không hợp lệ.")]
        public int BookId { get; set; }

        public int? BookCopyId { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự.")]
        public string? Note { get; set; }
    }
}

