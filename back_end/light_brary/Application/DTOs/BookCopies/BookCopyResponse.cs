using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.DTOs.BookCopies
{
    public class BookCopyResponse
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public string Barcode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? ConditionNote { get; set; }
    }
}