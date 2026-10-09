using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using light_brary.Application.DTOs.Categories;
using light_brary.Application.Exceptions;
using light_brary.Application.Interfaces;
using light_brary.Domain.Entities;

namespace light_brary.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }

        public async Task<CategoryResponse> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw new NotFoundException($"Không tìm thấy thể loại với ID: {id}");

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryResponse> CreateAsync(SaveCategoryRequest request)
        {
            var name = request.Name.Trim();
            var existing = await _categoryRepository.GetByNameAsync(name);
            if (existing is not null)
                throw new ConflictException($"Thể loại '{name}' đã tồn tại.");

            var category = new Category
            {
                Name = name,
                Description = request.Description?.Trim()
            };

            await _categoryRepository.AddAsync(category);

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryResponse> UpdateAsync(Guid id, SaveCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw new NotFoundException($"Không tìm thấy thể loại với ID: {id}");

            var name = request.Name.Trim();
            var existing = await _categoryRepository.GetByNameAsync(name);
            if (existing is not null && existing.Id != id)
                throw new ConflictException($"Thể loại '{name}' đã tồn tại.");

            category.Name = name;
            category.Description = request.Description?.Trim();

            await _categoryRepository.UpdateAsync(category);

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetWithBooksAsync(id);
            if (category is null)
                throw new NotFoundException($"Không tìm thấy thể loại với ID: {id}");

            if (category.Books != null && category.Books.Any())
                throw new ConflictException("Không thể xóa thể loại đang chứa các đầu sách.");

            await _categoryRepository.DeleteAsync(category);
        }
    }
}