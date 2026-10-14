using System;
using System.ComponentModel.DataAnnotations;

namespace light_brary.DTOs.Requests
{
    public record MemberBorrowRequest
    {
        [Required(ErrorMessage = "Mã đầu sách không được để trống.")]
        public Guid BookId { get; set; }

        public Guid? BookCopyId { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự.")]
        public string? Note { get; set; }
    }
}

