using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.Books;

namespace light_brary.Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponse>> GetAllAsync(string? keyword = null, int? categoryId = null, string? author = null);
        Task<BookResponse> GetByIdAsync(int id);
        Task<BookResponse> CreateAsync(SaveBookRequest request);
        Task<BookResponse> UpdateAsync(int id, SaveBookRequest request);
        Task DeleteAsync(int id);
    }
}