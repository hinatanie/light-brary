using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.Application.Interfaces;
using light_brary.Domain.Entities;
using light_brary.Infrastructure.Persistence;

namespace light_brary.Infrastructure.Repositories
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