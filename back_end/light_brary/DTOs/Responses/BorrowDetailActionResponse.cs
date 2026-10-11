using System;

namespace light_brary.DTOs.Responses
{
    public class BorrowDetailActionResponse
    {
        public Guid Id { get; set; }
        public Guid BorrowRecordId { get; set; }
        public Guid BookCopyId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ReturnDate { get; set; }
        public string BookCopyStatus { get; set; } = null!;
        public string BorrowRecordStatus { get; set; } = null!;
    }
}

