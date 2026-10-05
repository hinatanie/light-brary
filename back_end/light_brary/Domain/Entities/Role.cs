using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Domain.Entities
{
    [Table("AspNetRoles")]
    [Index(nameof(Name), IsUnique = true)]
    public class Role
    {
        public const string ADMIN = "Admin";
        public const string LIBRARIAN = "Librarian";
        public const string USER = "User";

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        [Required]
        [StringLength(50)]
        public string Name {get; set;} = string.Empty;

        [InverseProperty(nameof(UserRole.Role))]
        public ICollection<UserRole> UserRoles {get; set;} = new List<UserRole>();
    }
}
