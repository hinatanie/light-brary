using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class BorrowDetailResponse
    {
        public Guid Id { get; set; }
        public Guid BookCopyId { get; set; }
        public string Barcode { get; set; } = null!;
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? ReturnDate { get; set; }
    }
}