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
        public string FullName {get; set;} = null!;
        public string PhoneNumber {get; set;} = null!;
        public string? Address {get; set;}
        public string PasswordHash {get; set;} = null!;
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        // Navigation property
        public ICollection<UserRole> UserRoles {get; set;} = new List<UserRole>();
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}