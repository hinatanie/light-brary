using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly LibraryDbContext _context;

    public RefreshTokenRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    public async Task<bool> TryRevokeAsync(Guid tokenId, DateTime nowUtc, Guid? replacedById)
    {
        // Thực thi UPDATE nguyên tử ở DB: chỉ thu hồi nếu RevokedAt IS NULL
        var affected = await _context.RefreshTokens
            .Where(rt => rt.Id == tokenId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.RevokedAt, nowUtc)
                .SetProperty(rt => rt.ReplacedById, replacedById));

        return affected > 0;
    }

    public async Task RevokeFamilyAsync(Guid familyId, DateTime nowUtc)
    {
        await _context.RefreshTokens
            .Where(rt => rt.FamilyId == familyId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, nowUtc));
    }

    public async Task RevokeAllForUserAsync(Guid userId, DateTime nowUtc)
    {
        await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, nowUtc));
    }
}
