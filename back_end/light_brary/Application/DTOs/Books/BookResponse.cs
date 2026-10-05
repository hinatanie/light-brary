using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.DTOs.Books
{
    public class BookResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public int? PublishYear { get; set; }
        public Category Category { get; set; } = null!;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}