using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Library.Domain.Entities;

[Table("Books")]
[Index(nameof(CategoryId), Name = "IX_Books_CategoryId")]
public class Book
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(250)]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Column("Author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>ISBN đã chuẩn hóa (bỏ dấu gạch/khoảng trắng). Duy nhất khi có giá trị (filtered unique index).</summary>
    [MaxLength(20)]
    [Column("ISBN")]
    public string? Isbn { get; set; }

    [Column("PublishYear")]
    public int? PublishYear { get; set; }

    [MaxLength(500)]
    [Column("ImageUrl")]
    public string? ImageUrl { get; set; }

    [Column("CategoryId")]
    public Guid CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
}
