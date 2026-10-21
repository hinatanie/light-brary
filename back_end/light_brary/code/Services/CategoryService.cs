using light_brary.Repositories;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;

namespace light_brary.Services
{
    public class CategoryService(CategoryRepository categoryRepository)
    {
        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            var categories = await categoryRepository.GetAllAsync();
            return categories.Select(CategoryResponse.FromEntity).ToList();
        }

        public async Task<CategoryResponse> GetByIdAsync(Guid id)
        {
            var category = await categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw AppException.NotFound($"Không tìm thấy thể loại với ID: {id}");

            return CategoryResponse.FromEntity(category);
        }

        public async Task<CategoryResponse> CreateAsync(SaveCategoryRequest request)
        {
            var name = request.Name.Trim();
            var existing = await categoryRepository.GetByNameAsync(name);
            if (existing is not null)
                throw AppException.Conflict($"Thể loại '{name}' đã tồn tại.");

            var category = new Category
            {
                Name = name,
                Description = request.Description?.Trim()
            };

            await categoryRepository.AddAsync(category);

            return CategoryResponse.FromEntity(category);
        }

        public async Task<CategoryResponse> UpdateAsync(Guid id, SaveCategoryRequest request)
        {
            var category = await categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw AppException.NotFound($"Không tìm thấy thể loại với ID: {id}");

            var name = request.Name.Trim();
            var existing = await categoryRepository.GetByNameAsync(name);
            if (existing is not null && existing.Id != id)
                throw AppException.Conflict($"Thể loại '{name}' đã tồn tại.");

            category.Name = name;
            category.Description = request.Description?.Trim();

            await categoryRepository.UpdateAsync(category);

            return CategoryResponse.FromEntity(category);
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await categoryRepository.GetWithBooksAsync(id);
            if (category is null)
                throw AppException.NotFound($"Không tìm thấy thể loại với ID: {id}");

            if (category.Books != null && category.Books.Any())
                throw AppException.Conflict("Không thể xóa thể loại đang chứa các đầu sách.");

            await categoryRepository.DeleteAsync(category);
        }
    }
}
