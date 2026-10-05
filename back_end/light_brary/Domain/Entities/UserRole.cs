using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Domain.Entities
{
    [Table("AspNetUserRoles")]
    [PrimaryKey(nameof(UserId), nameof(RoleId))]
    public class UserRole
    {
        public int UserId {get; set;}

        public int RoleId {get; set;}

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(User.UserRoles))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public User User {get; set;} = null!; 

        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(Role.UserRoles))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public Role Role {get; set;} = null!;
    }
}
