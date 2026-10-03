using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);

    /// <summary>
    /// Thu hồi nguyên tử một token còn hiệu lực (UPDATE ... WHERE RevokedAt IS NULL).
    /// Trả về false nếu token đã bị thu hồi trước đó (ví dụ do request song song).
    /// </summary>
    Task<bool> TryRevokeAsync(Guid tokenId, DateTime nowUtc, Guid? replacedById);

    Task RevokeFamilyAsync(Guid familyId, DateTime nowUtc);
    Task RevokeAllForUserAsync(Guid userId, DateTime nowUtc);
}
