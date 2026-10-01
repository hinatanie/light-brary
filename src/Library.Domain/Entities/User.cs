using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

[Table("AspNetUsers")]
public class User
{
    [Key]
    [MaxLength(450)]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("PasswordHash")]
    public string? PasswordHash { get; set; }

    [MaxLength(50)]
    [Column("PhoneNumber")]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("FullName")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Address")]
    public string? Address { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
}
