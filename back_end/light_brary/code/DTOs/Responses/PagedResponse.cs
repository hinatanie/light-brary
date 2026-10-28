namespace light_brary.DTOs.Responses
{
    public record PagedResponse<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int PageNumber { get; init; }
        public int PageSize { get; init; } = 10;
        public int TotalCount { get; init; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
