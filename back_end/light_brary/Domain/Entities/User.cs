using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Domain.Entities
{
    [Table("AspNetUsers")]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        [Required]
        [StringLength(150)]
        public string FullName {get; set;} = null!;

        [Required]
        [StringLength(10)]
        public string PhoneNumber {get; set;} = null!;

        [StringLength(255)]
        public string? Address {get; set;}

        [Required]
        public string PasswordHash {get; set;} = null!;

        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        [InverseProperty(nameof(UserRole.User))]
        public ICollection<UserRole> UserRoles {get; set;} = new List<UserRole>();

        [InverseProperty(nameof(BorrowRecord.User))]
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}
