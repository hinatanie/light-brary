using light_brary.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Repositories
{
    public class RoleRepository(AppDbContext context)
    {
        public async Task<IEnumerable<IdentityRole<Guid>>> GetByNamesAsync(IEnumerable<string> roleNames)
        {
            var names = roleNames.ToArray();
            return await context.Roles
                .AsNoTracking()
                .Where(role => names.Contains(role.Name!))
                .ToListAsync();
        }

        public async Task<IdentityRole<Guid>?> GetByIdForUpdateAsync(Guid id)
        {
            // Serialize concurrent membership changes for this role.
            return await context.Roles
                .FromSqlInterpolated($"SELECT * FROM \"AspNetRoles\" WHERE \"Id\" = {id} FOR UPDATE")
                .SingleOrDefaultAsync();
        }
    }
}
