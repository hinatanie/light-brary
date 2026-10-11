using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.DTOs.Responses
{
    public class RoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}