using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;

namespace light_brary.Services.Interfaces
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