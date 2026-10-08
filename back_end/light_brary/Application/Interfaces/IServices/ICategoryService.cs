using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.Categories;

namespace light_brary.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
        Task<CategoryResponse> GetByIdAsync(int id);
        Task<CategoryResponse> CreateAsync(SaveCategoryRequest request);
        Task<CategoryResponse> UpdateAsync(int id, SaveCategoryRequest request);
        Task DeleteAsync(int id);
    }
}