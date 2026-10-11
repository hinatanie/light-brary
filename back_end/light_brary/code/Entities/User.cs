using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace light_brary.Entities
{
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class User : IdentityUser<Guid>
    {
        [Required]
        [StringLength(50)]
        public string FullName {get; set;} = null!;

        [StringLength(255)]
        [Required]
        public string Address {get; set;} = null!;

        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}
