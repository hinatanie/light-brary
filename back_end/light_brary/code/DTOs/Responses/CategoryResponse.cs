using light_brary.Entities;

namespace light_brary.DTOs.Responses
{
    public record CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? BookCount { get; set; }

        internal static CategoryResponse FromEntity(Category category, int? bookCount = null) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            BookCount = bookCount ?? category.Books.Count
        };
    }
}
