using light_brary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.DTOs.Responses
{
    public record CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        internal static CategoryResponse FromEntity(Category category) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}