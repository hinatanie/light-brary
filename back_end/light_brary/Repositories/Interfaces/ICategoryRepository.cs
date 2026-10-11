using System.Threading.Tasks;
using light_brary.Entities;

namespace light_brary.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<Category?> GetWithBooksAsync(Guid id);
    }
}