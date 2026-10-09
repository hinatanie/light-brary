using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.Books;

namespace light_brary.Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponse>> GetAllAsync(string? keyword = null, Guid? categoryId = null, string? author = null, bool? onlyAvailable = null);
        Task<BookResponse> GetByIdAsync(Guid id);
        Task<BookResponse> CreateAsync(SaveBookRequest request);
        Task<BookResponse> UpdateAsync(Guid id, SaveBookRequest request);
        Task DeleteAsync(Guid id);
    }
}