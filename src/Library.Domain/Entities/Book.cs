using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

[Table("Books")]
public class Book
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(250)]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Column("Author")]
    public string Author { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("ISBN")]
    public string? Isbn { get; set; }

    [Column("PublishYear")]
    public int? PublishYear { get; set; }

    [MaxLength(500)]
    [Column("ImageUrl")]
    public string? ImageUrl { get; set; }

    [Column("CategoryId")]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
}
