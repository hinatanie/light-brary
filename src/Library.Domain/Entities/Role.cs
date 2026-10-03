using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Library.Domain.Entities;

[Table("Roles")]
[Index(nameof(Name), IsUnique = true, Name = "UX_Roles_Name")]
public class Role
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(256)]
    [Column("Name", TypeName = "citext")]
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
