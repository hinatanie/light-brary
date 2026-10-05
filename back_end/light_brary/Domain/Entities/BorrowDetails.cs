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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        public DateTime? ReturnDate {get; set;}
        public BorrowDetailStatus Status {get; set;} = BorrowDetailStatus.Borrowing;

        [ForeignKey(nameof(BorrowRecord))]
        public int BorrowRecordId {get; set;}

        [ForeignKey(nameof(BookCopy))]
        public int BookCopyId {get; set;}

        public BorrowRecord BorrowRecord {get; set;} = null!;
        public BookCopy BookCopy {get; set;} = null!;
    }
}
