using light_brary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            return categories.Select(MapToResponse).ToList();
        }

        public async Task<CategoryResponse> GetByIdAsync(Guid id)
        {
            var category = await categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw AppException.NotFound($"Không tìm thấy thể loại với ID: {id}");

            return MapToResponse(category);
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

            return MapToResponse(category);
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

            return MapToResponse(category);
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

        private static CategoryResponse MapToResponse(Category category) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}