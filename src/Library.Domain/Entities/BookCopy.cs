using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Library.Domain.Enums;

namespace Library.Domain.Entities;

[Table("BookCopies")]
public class BookCopy
{
    [Key]
    public int Id { get; set; }

    [Column("BookId")]
    public int BookId { get; set; }

    [ForeignKey(nameof(BookId))]
    public Book? Book { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("Barcode")]
    public string Barcode { get; set; } = string.Empty;

    [Column("Status")]
    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

    [MaxLength(255)]
    [Column("ConditionNote")]
    public string? ConditionNote { get; set; }

    // Navigation properties
    public ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();
}
