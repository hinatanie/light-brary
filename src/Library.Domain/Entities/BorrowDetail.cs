using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Library.Domain.Enums;

namespace Library.Domain.Entities;

[Table("BorrowDetails")]
public class BorrowDetail
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("BorrowRecordId")]
    public Guid BorrowRecordId { get; set; }

    [ForeignKey(nameof(BorrowRecordId))]
    public BorrowRecord? BorrowRecord { get; set; }

    [Column("BookCopyId")]
    public Guid BookCopyId { get; set; }

    [ForeignKey(nameof(BookCopyId))]
    public BookCopy? BookCopy { get; set; }

    [Column("ReturnDate")]
    public DateTime? ReturnDate { get; set; }

    [Column("Status")]
    public BorrowDetailStatus Status { get; set; } = BorrowDetailStatus.Borrowing;

    [Column("FineAmount", TypeName = "decimal(18,2)")]
    public decimal FineAmount { get; set; } = 0;
}
