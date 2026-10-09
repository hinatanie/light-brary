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
    [Table("BookCopies")]
    [Index(nameof(Barcode), IsUnique = true)]
    public class BookCopy
    {
        [Key]
        public Guid Id {get; set;} = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Barcode {get; set;} = null!;

        public BookCopyStatus Status {get; set;} = BookCopyStatus.Available;

        [StringLength(255)]
        public string? ConditionNote {get; set;}

        public Guid BookId {get; set;}
 
        [ForeignKey(nameof(BookId))]
        [InverseProperty(nameof(Book.BookCopies))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Book Book {get; set;} = null!;

        [InverseProperty(nameof(BorrowDetail.BookCopy))]
        public ICollection<BorrowDetail> BorrowDetails {get; set;} = new List<BorrowDetail>();
    }
}
