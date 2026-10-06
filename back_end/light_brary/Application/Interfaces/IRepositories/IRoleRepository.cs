using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);
    }
}