using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

using light_brary.Enums;

namespace light_brary.Entities
{
    public class BorrowRecord
    {
        [Key]
        public Guid Id {get; set;} = Guid.NewGuid();

        public DateTime BorrowDate {get; set;} = DateTime.UtcNow;

        public DateTime DueDate {get; set;} = DateTime.UtcNow.AddDays(14);

        public BorrowRecordStatus Status {get; set;} = BorrowRecordStatus.Borrowing;

        [StringLength(255)]
        public string? Note {get; set;}

        public Guid UserId {get; set;}
 
        public User User {get; set;} = null!;

        [InverseProperty(nameof(BorrowDetail.BorrowRecord))]
        public ICollection<BorrowDetail> BorrowDetails {get; set;} = new List<BorrowDetail>();

        public Fine? Fine { get; set; }
    }
}
