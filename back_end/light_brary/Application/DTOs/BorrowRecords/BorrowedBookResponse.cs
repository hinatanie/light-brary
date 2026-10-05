using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class BorrowedBookResponse
    {
        public int BorrowDetailId { get; set; }
        public int BorrowRecordId { get; set; }
        public int BookCopyId { get; set; }
        public string Barcode { get; set; } = null!;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }
}