using light_brary.Entities;
using light_brary.Enums;

namespace light_brary.DTOs.Responses
{
    public record BorrowRecordResponse
    {
        public Guid Id { get; set; }
        public UserResponse User { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
        public bool IsOverdue { get; set; }
        public List<BorrowDetailResponse> Details { get; set; } = new List<BorrowDetailResponse>();

        internal static BorrowRecordResponse FromEntity(BorrowRecord record, UserResponse user) => new()
        {
            Id = record.Id,
            User = user,
            BorrowDate = record.BorrowDate,
            DueDate = record.DueDate,
            Status = record.Status.ToString(),
            Note = record.Note,
            IsOverdue = record.BorrowDetails.Any(detail => detail.Status == BorrowDetailStatus.Borrowing) &&
                DateTime.UtcNow > record.DueDate,
            Details = record.BorrowDetails.Select(BorrowDetailResponse.FromEntity).ToList()
        };
    }
}
