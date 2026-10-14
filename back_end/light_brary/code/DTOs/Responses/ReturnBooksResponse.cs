using System.Collections.Generic;

namespace light_brary.DTOs.Responses
{
    public record ReturnBooksResponse
    {
        public List<BorrowDetailActionResponse> Items { get; set; } = new List<BorrowDetailActionResponse>();
    }
}

