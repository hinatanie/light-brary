using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Domain.Entities
{
    [Table("AspNetUserRoles")]
    public class UserRole
    {
        public int UserId {get; set;}
        public int RoleId {get; set;}
        public User? User {get; set;}
        public Role? Role {get; set;}
    }
}