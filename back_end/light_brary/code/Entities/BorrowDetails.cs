using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using light_brary.Enums;

namespace light_brary.Entities
{
    public class BorrowDetail
    {
        [Key]
        public Guid Id {get; set;} = Guid.NewGuid();

        public DateTime? ReturnDate {get; set;}

        public BorrowDetailStatus Status {get; set;} = BorrowDetailStatus.Borrowing;

        public Guid BorrowRecordId {get; set;}

        public BorrowRecord BorrowRecord {get; set;} = null!;

        public Guid? BookCopyId {get; set;}

        [DeleteBehavior(DeleteBehavior.SetNull)]
        public BookCopy? BookCopy {get; set;} = null!;
    }
}
