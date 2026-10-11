using System.Threading.Tasks;
using light_brary.Entities;

namespace light_brary.Repositories.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);
    }
}