using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [ForeignKey(nameof(UserId))]
        public User User {get; set;} = null!; 

        [ForeignKey(nameof(RoleId))]
        public Role Role {get; set;} = null!;
    }
}
