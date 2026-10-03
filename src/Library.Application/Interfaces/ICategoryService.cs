using Library.Application.DTOs.Categories;

namespace Library.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> GetByIdAsync(Guid id);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task DeleteAsync(Guid id);
}
