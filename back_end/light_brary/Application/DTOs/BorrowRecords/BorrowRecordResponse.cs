using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using light_brary.Application.DTOs.Users;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class BorrowRecordResponse
    {
        public Guid Id { get; set; }
        public UserResponse User { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
        public bool IsOverdue { get; set; }
        public List<BorrowDetailResponse> Details { get; set; } = new List<BorrowDetailResponse>();
    }
}