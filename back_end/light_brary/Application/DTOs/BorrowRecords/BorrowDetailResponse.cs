using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class BorrowDetailResponse
    {
        public int Id { get; set; }
        public int BookCopyId { get; set; }
        public string Barcode { get; set; } = null!;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? ReturnDate { get; set; }
    }
}