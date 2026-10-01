using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

[Table("AspNetUserRoles")]
public class UserRole
{
    [MaxLength(450)]
    [Column("UserId")]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [MaxLength(450)]
    [Column("RoleId")]
    public string RoleId { get; set; } = string.Empty;

    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }
}
