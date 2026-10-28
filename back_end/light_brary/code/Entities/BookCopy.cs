using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using light_brary.Enums;

namespace light_brary.Entities
{
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
 
        public Book Book {get; set;} = null!;

        public ICollection<BorrowDetail> BorrowDetails {get; set;} = new List<BorrowDetail>();
    }
}
