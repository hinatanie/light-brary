using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

using light_brary.Domain.Enums;

namespace light_brary.Domain.Entities
{
    [Table("BookCopies")]
    public class BookCopy
    {
        [Key] public int Id {get; set;}
        public string Barcode {get; set;} = string.Empty;
        public BookCopyStatus Status {get; set;} = BookCopyStatus.Available;
        public string? ConditionNote {get; set;}
        public int BookId {get; set;}
 
        // Navigation property
        public Book? Book {get; set;}
        public ICollection<BorrowDetail> BorrowDetails {get; set;} = new List<BorrowDetail>();
    }
}