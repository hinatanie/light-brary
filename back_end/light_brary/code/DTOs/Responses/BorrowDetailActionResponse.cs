using light_brary.Entities;

namespace light_brary.DTOs.Responses
{
    public record BorrowDetailActionResponse
    {
        public Guid Id { get; set; }
        public Guid BorrowRecordId { get; set; }
        public Guid BookCopyId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ReturnDate { get; set; }
        public string BookCopyStatus { get; set; } = null!;
        public string BorrowRecordStatus { get; set; } = null!;

        internal static BorrowDetailActionResponse FromEntity(BorrowDetail detail)
        {
            return new BorrowDetailActionResponse
            {
                Id = detail.Id,
                BorrowRecordId = detail.BorrowRecordId,
                BookCopyId = detail.BookCopyId,
                Status = detail.Status.ToString(),
                ReturnDate = detail.ReturnDate,
                BookCopyStatus = detail.BookCopy.Status.ToString(),
                BorrowRecordStatus = detail.BorrowRecord.Status.ToString()
            };
        }
    }
}

