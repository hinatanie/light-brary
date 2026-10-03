using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Library.Domain.Entities;

/// <summary>
/// Refresh token đã băm (SHA-256). Mỗi lần refresh sẽ xoay vòng token; các token cùng <see cref="FamilyId"/>
/// bị thu hồi hàng loạt khi phát hiện tái sử dụng token đã xoay.
/// </summary>
[Table("RefreshTokens")]
[Index(nameof(TokenHash), IsUnique = true, Name = "UX_RefreshTokens_TokenHash")]
[Index(nameof(UserId), Name = "IX_RefreshTokens_UserId")]
[Index(nameof(FamilyId), Name = "IX_RefreshTokens_FamilyId")]
public class RefreshToken
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("UserId")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("TokenHash")]
    public string TokenHash { get; set; } = string.Empty;

    [Column("FamilyId")]
    public Guid FamilyId { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }

    [Column("RevokedAt")]
    public DateTime? RevokedAt { get; set; }

    [Column("ReplacedById")]
    public Guid? ReplacedById { get; set; }
}
