using System;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class BorrowDetailActionResponse
    {
        public int Id { get; set; }
        public int BorrowRecordId { get; set; }
        public int BookCopyId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ReturnDate { get; set; }
        public string BookCopyStatus { get; set; } = null!;
        public string BorrowRecordStatus { get; set; } = null!;
    }
}

