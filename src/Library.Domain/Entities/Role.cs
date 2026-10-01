using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

[Table("AspNetRoles")]
public class Role
{
    [Key]
    [MaxLength(450)]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(256)]
    [Column("Name")]
    public string? Name { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
