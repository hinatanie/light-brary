using light_brary.Entities;

namespace light_brary.DTOs.Responses
{
    public record BookCopyResponse
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public string BookAuthor { get; set; } = null!;
        public string Barcode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? ConditionNote { get; set; }

        internal static BookCopyResponse FromEntity(BookCopy copy)
        {
            return new BookCopyResponse
            {
                Id = copy.Id,
                BookId = copy.BookId,
                BookTitle = copy.Book?.Title ?? string.Empty,
                BookAuthor = copy.Book?.Author ?? string.Empty,
                Barcode = copy.Barcode,
                Status = copy.Status.ToString(),
                ConditionNote = copy.ConditionNote
            };
        }
    }
}
