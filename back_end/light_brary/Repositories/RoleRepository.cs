using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.Repositories.Interfaces;
using light_brary.Entities;
using light_brary.Data;

namespace light_brary.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == name);
        }
    }
}