using Library.Domain.Entities;

namespace Library.Application.Interfaces.Authentication;

public record AccessToken(string Token, DateTime ExpiresAt);

public interface IJwtTokenService
{
    /// <summary>Tạo access token ngắn hạn. Role lấy từ user.UserRoles (đã Include Role).</summary>
    AccessToken GenerateToken(User user);
}
