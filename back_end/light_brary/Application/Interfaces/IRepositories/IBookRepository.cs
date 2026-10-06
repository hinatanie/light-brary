using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.Interfaces
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<Book?> GetByIsbnAsync(string isbn);
        Task<IEnumerable<Book>> GetByCategoryIdAsync(int categoryId);
        Task<Book?> GetWithDetailsAsync(int id);
    }
}