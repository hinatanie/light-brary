using Microsoft.EntityFrameworkCore;
using light_brary.Entities;
using light_brary.Data;

namespace light_brary.Repositories
{
    public class UserRepository : GenericRepository<User>
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.PhoneNumber == phoneNumber);
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.AnyAsync(user => user.PhoneNumber == phoneNumber);
        }

        public async Task<IEnumerable<User>> GetAllFilteredAsync(string? keyword = null, string? role = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(user => user.FullName.ToLower().Contains(kw) ||
                    (user.PhoneNumber != null && user.PhoneNumber.Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var normalizedRole = role.Trim().ToUpperInvariant();
                var userIds = from userRole in _context.UserRoles
                              join identityRole in _context.Roles on userRole.RoleId equals identityRole.Id
                              where identityRole.NormalizedName == normalizedRole
                              select userRole.UserId;
                query = query.Where(user => userIds.Contains(user.Id));
            }

            return await query.OrderByDescending(user => user.CreatedAt)
                .ThenByDescending(user => user.Id).ToListAsync();
        }

        public async Task<Dictionary<Guid, List<string>>> GetRoleNamesByUserIdsAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.Distinct().ToArray();
            if (ids.Length == 0) return new Dictionary<Guid, List<string>>();

            var roles = await (from userRole in _context.UserRoles
                               join role in _context.Roles on userRole.RoleId equals role.Id
                               where ids.Contains(userRole.UserId) && role.Name != null
                               select new { userRole.UserId, Name = role.Name! }).ToListAsync();
            return roles.GroupBy(role => role.UserId)
                .ToDictionary(group => group.Key, group => group.Select(role => role.Name).ToList());
        }

        public async Task<bool> HasBorrowHistoryAsync(Guid userId)
        {
            return await _context.BorrowRecords.AnyAsync(record => record.UserId == userId);
        }
    }
}
