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
        [Key] public int Id {get; set;}
        public DateTime BorrowDate {get; set;} = DateTime.UtcNow;
        public DateTime DueDate {get; set;}
        public BorrowRecordStatus Status {get; set;} = BorrowRecordStatus.Borrowing;
        public string? Note {get; set;}
        public int UserId {get; set;}
 
        // Navigation property
        public User? User {get; set;}
        public ICollection<BorrowDetail> BorrowDetails {get; set;} = new List<BorrowDetail>();
    }
}