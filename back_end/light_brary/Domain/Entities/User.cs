using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace light_brary.Domain.Entities
{
    [Table("AspNetUsers")]
    public class User
    {
        [Key] public int Id {get; set;} 
        public string FullName {get; set;} = string.Empty;
        public string PhoneNumber {get; set;} = string.Empty;
        public string? Address {get; set;}
        public string PasswordHash {get; set;} = string.Empty;
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        // Navigation property
        public ICollection<UserRole> UserRoles {get; set;} = new List<UserRole>();
    }
}