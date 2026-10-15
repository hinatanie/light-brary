using light_brary.Entities;
using light_brary.Enums;
using System;

namespace light_brary.DTOs.Responses
{
    public record BorrowRecordSummaryResponse
    {
        public Guid Id { get; set; }
        public UserResponse User { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = null!;
        public int TotalBooks { get; set; }
        public int BorrowingCount { get; set; }
        public int ReturnedCount { get; set; }
        public int LostCount { get; set; }
        public bool IsOverdue { get; set; }
        public List<BorrowDetailResponse> Details { get; set; } = new List<BorrowDetailResponse>();

        internal static BorrowRecordSummaryResponse FromEntity(BorrowRecord record, UserResponse user)
        {
            var isOverdue = record.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing) && DateTime.UtcNow > record.DueDate;

            return new BorrowRecordSummaryResponse
            {
                Id = record.Id,
                User = user,
                BorrowDate = record.BorrowDate,
                DueDate = record.DueDate,
                Status = record.Status.ToString(),
                TotalBooks = record.BorrowDetails.Count,
                BorrowingCount = record.BorrowDetails.Count(d => d.Status == BorrowDetailStatus.Borrowing),
                ReturnedCount = record.BorrowDetails.Count(d => d.Status == BorrowDetailStatus.Returned),
                LostCount = record.BorrowDetails.Count(d => d.Status == BorrowDetailStatus.Lost),
                IsOverdue = isOverdue,
                Details = record.BorrowDetails.Select(BorrowDetailResponse.FromEntity).ToList()
            };
        }
    }
}

