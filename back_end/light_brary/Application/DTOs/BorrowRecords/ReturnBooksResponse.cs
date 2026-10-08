using System.Collections.Generic;

namespace light_brary.Application.DTOs.BorrowRecords
{
    public class ReturnBooksResponse
    {
        public List<BorrowDetailActionResponse> Items { get; set; } = new List<BorrowDetailActionResponse>();
    }
}

