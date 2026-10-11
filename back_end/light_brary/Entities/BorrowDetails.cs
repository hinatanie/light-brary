using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using light_brary.Enums;

namespace light_brary.Entities
{
    [Table("BorrowDetails")]
    public class BorrowDetail
    {
        [Key]
        public Guid Id {get; set;} = Guid.NewGuid();

        public DateTime? ReturnDate {get; set;}
        public BorrowDetailStatus Status {get; set;} = BorrowDetailStatus.Borrowing;

        public Guid BorrowRecordId {get; set;}

        public Guid BookCopyId {get; set;}

        [ForeignKey(nameof(BorrowRecordId))]
        [InverseProperty(nameof(BorrowRecord.BorrowDetails))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public BorrowRecord BorrowRecord {get; set;} = null!;

        [ForeignKey(nameof(BookCopyId))]
        [InverseProperty(nameof(BookCopy.BorrowDetails))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public BookCopy BookCopy {get; set;} = null!;
    }
}
