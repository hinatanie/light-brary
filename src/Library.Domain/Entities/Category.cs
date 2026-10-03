using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Library.Domain.Entities;

[Table("Categories")]
[Index(nameof(Name), IsUnique = true, Name = "UX_Categories_Name")]
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Name", TypeName = "citext")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Description")]
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Book> Books { get; set; } = new List<Book>();
}