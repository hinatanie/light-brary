using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

[Table("Categories")]
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Description")]
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Book> Books { get; set; } = new List<Book>();
}