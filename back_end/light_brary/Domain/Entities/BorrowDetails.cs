using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

using light_brary.Domain.Enums;

namespace light_brary.Domain.Entities
{
    [Table("BorrowDetails")]
    public class BorrowDetail
    {
        [Key] public int Id {get; set;}
        public DateTime? ReturnDate {get; set;}
        public BorrowDetailStatus Status {get; set;} = BorrowDetailStatus.Borrowing;
        public int BorrowRecordId {get; set;}
        public int BookCopyId {get; set;}

        // Navigation
        public BorrowRecord? BorrowRecord {get; set;}
        public BookCopy? BookCopy {get; set;}
    }
}