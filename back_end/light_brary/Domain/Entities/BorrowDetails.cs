using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using light_brary.Domain.Enums;

namespace light_brary.Domain.Entities
{
    [Table("BorrowDetails")]
    public class BorrowDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        public DateTime? ReturnDate {get; set;}
        public BorrowDetailStatus Status {get; set;} = BorrowDetailStatus.Borrowing;

        public int BorrowRecordId {get; set;}

        public int BookCopyId {get; set;}

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
