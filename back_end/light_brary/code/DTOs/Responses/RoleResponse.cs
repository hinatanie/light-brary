using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace light_brary.DTOs.Responses
{
    public record RoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        internal static RoleResponse FromEntity(IdentityRole<Guid> role) => new()
        {
            Id = role.Id,
            Name = role.Name!
        };
    }
}
