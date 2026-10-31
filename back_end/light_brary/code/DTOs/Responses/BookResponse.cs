using light_brary.Enums;
using light_brary.Entities;

namespace light_brary.DTOs.Responses
{
    public record BookResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int? PublishYear { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public CategorySummaryResponse? Category { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime CreatedAt { get; set; }

        internal static BookResponse FromEntity(Book book)
        {
            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                ImageUrl = book.ImageUrl,
                PublishYear = book.PublishYear,
                CategoryId = book.CategoryId,
                CategoryName = book.Category?.Name ?? string.Empty,
                Category = book.Category != null ? new CategorySummaryResponse
                {
                    Id = book.Category.Id,
                    Name = book.Category.Name,
                    Description = book.Category.Description
                } : null,
                TotalCopies = book.BookCopies?.Count ?? 0,
                AvailableCopies = book.BookCopies?.Count(c => c.Status == BookCopyStatus.Available) ?? 0,
                CreatedAt = book.CreatedAt
            };
        }
    }
}
