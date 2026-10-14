using light_brary.Entities;

namespace light_brary.DTOs.Responses
{
    public record BorrowDetailResponse
    {
        public Guid Id { get; set; }
        public Guid BookCopyId { get; set; }
        public string Barcode { get; set; } = null!;
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? ReturnDate { get; set; }

        internal static BorrowDetailResponse FromEntity(BorrowDetail detail) => new()
        {
            Id = detail.Id,
            BookCopyId = detail.BookCopyId,
            Barcode = detail.BookCopy?.Barcode ?? string.Empty,
            BookId = detail.BookCopy?.BookId ?? Guid.Empty,
            BookTitle = detail.BookCopy?.Book?.Title ?? string.Empty,
            Status = detail.Status.ToString(),
            ReturnDate = detail.ReturnDate
        };
    }
}
