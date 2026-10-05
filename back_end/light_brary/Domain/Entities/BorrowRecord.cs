using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

using light_brary.Domain.Enums;

namespace light_brary.Domain.Entities
{
    [Table("BorrowRecords")]
    public class BorrowRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        public DateTime BorrowDate {get; set;} = DateTime.UtcNow;
        public DateTime DueDate {get; set;} = DateTime.UtcNow;
        public BorrowRecordStatus Status {get; set;} = BorrowRecordStatus.Borrowing;

        [StringLength(255)]
        public string? Note {get; set;}

        public int UserId {get; set;}
 
        [ForeignKey(nameof(UserId))]
        public User User {get; set;} = null!;
        public ICollection<BorrowDetail> BorrowDetails {get; set;} = new List<BorrowDetail>();
    }
}
