using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Library.Domain.Entities;

[Table("Users")]
[Index(nameof(Email), IsUnique = true, Name = "UX_Users_Email")]
[Index(nameof(PhoneNumber), IsUnique = true, Name = "UX_Users_PhoneNumber")]
public class User
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Email lưu dạng chữ thường; kiểu citext để unique không phân biệt hoa/thường ở tầng DB.</summary>
    [Required]
    [MaxLength(256)]
    [Column("Email", TypeName = "citext")]
    public string Email { get; set; } = string.Empty;

    [Column("PasswordHash")]
    public string? PasswordHash { get; set; }

    /// <summary>Số điện thoại đã chuẩn hóa (0xxxxxxxxx). Bắt buộc và duy nhất.</summary>
    [Required]
    [MaxLength(20)]
    [Column("PhoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Column("FullName")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Address")]
    public string? Address { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Bắt buộc đổi mật khẩu ở lần đăng nhập kế tiếp (tài khoản admin khởi tạo từ cấu hình).</summary>
    [Column("MustChangePassword")]
    public bool MustChangePassword { get; set; }

    /// <summary>Số lần đăng nhập sai liên tiếp.</summary>
    [Column("FailedLoginCount")]
    public int FailedLoginCount { get; set; }

    /// <summary>Thời điểm (UTC) tài khoản được mở khóa; null nếu không bị khóa.</summary>
    [Column("LockoutEndUtc")]
    public DateTime? LockoutEndUtc { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
