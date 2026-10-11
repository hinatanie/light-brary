using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;

namespace light_brary.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
        Task<CategoryResponse> GetByIdAsync(Guid id);
        Task<CategoryResponse> CreateAsync(SaveCategoryRequest request);
        Task<CategoryResponse> UpdateAsync(Guid id, SaveCategoryRequest request);
        Task DeleteAsync(Guid id);
    }
}