using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Domain.Entities
{
    [Table("AspNetRoles")]
    public class Role
    {
        public const string ADMIN = "Admin";
        public const string LIBRARIAN = "Librarian";
        public const string USER = "User";

        [Key] public int Id {get; set;}
        public string Name {get; set;} = string.Empty;

        // Navigation property
        public ICollection<UserRole> UserRoles {get; set;} = new List<UserRole>();
    }
}