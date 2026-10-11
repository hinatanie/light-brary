using System.Collections.Generic;

namespace light_brary.DTOs.Responses
{
    public class ReturnBooksResponse
    {
        public List<BorrowDetailActionResponse> Items { get; set; } = new List<BorrowDetailActionResponse>();
    }
}

