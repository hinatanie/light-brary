using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.DTOs.Responses
{
    public record BorrowedBookResponse
    {
        public Guid BorrowDetailId { get; set; }
        public Guid BorrowRecordId { get; set; }
        public Guid BookCopyId { get; set; }
        public string Barcode { get; set; } = null!;
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }
}