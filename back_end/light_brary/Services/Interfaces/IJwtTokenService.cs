using light_brary.Entities;

namespace light_brary.Services.Interfaces
{
    public interface IJwtTokenService
    {
        /// <summary>Tạo access token cho user kèm danh sách role.</summary>
        string GenerateToken(User user, IEnumerable<string> roles);

        /// <summary>Thời gian sống của token (giây).</summary>
        int ExpiresInSeconds { get; }
    }
}