using System;

namespace light_brary.DTOs.Responses
{
    public class BorrowRecordSummaryResponse
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
    }
}

