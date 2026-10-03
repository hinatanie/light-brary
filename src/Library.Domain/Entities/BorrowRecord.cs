using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Library.Domain.Enums;

namespace Library.Domain.Entities;

[Table("BorrowRecords")]
public class BorrowRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("UserId")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Column("BorrowDate")]
    public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

    [Column("DueDate")]
    public DateTime DueDate { get; set; }

    [Column("Status")]
    public BorrowRecordStatus Status { get; set; } = BorrowRecordStatus.Borrowing;

    [MaxLength(255)]
    [Column("Note")]
    public string? Note { get; set; }

    // Navigation properties
    public ICollection<BorrowDetail> BorrowDetails { get; set; } = new List<BorrowDetail>();
}
