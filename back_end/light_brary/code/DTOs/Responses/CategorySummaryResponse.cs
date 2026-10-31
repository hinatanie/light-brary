namespace light_brary.DTOs.Responses
{
    public record CategorySummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
