using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Application.DTOs.Categories
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}