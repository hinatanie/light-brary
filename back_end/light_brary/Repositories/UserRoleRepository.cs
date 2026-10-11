using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.Entities;
using light_brary.Data;

namespace light_brary.Repositories
{
    public class UserRoleRepository
    {
        private readonly AppDbContext _context;

        public UserRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<UserRole?> GetByUserAndRoleIdAsync(Guid userId, Guid roleId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid roleId)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task AddAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(UserRole userRole)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }
}