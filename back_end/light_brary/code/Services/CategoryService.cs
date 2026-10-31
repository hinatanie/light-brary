using light_brary.Repositories;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace light_brary.Services
{
    public class CategoryService(
        CategoryRepository categoryRepository,
        BookRepository bookRepository,
        IDistributedCache cache)
    {
        public async Task<IEnumerable<CategoryResponse>> GetAllAsync(string? name = null)
        {
            return await categoryRepository.GetAllWithBookCountsAsync(name);
        }

        public async Task<CategoryResponse> GetByIdAsync(Guid id)
        {
            var category = await categoryRepository.GetWithBooksAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy thể loại với ID: {id}");

            return CategoryResponse.FromEntity(category);
        }

        public async Task<CategoryResponse> CreateAsync(SaveCategoryRequest request)
        {
            var name = await CheckNameAsync(request.Name);

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
            var category = await categoryRepository.GetByIdAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy thể loại với ID: {id}");

            var name = await CheckNameAsync(request.Name, id);

            category.Name = name;
            category.Description = request.Description?.Trim();

            await categoryRepository.UpdateAsync(category);

            var bookIds = await bookRepository.GetIdsByCategoryAsync(id);
            await Parallel.ForEachAsync(
                bookIds,
                new ParallelOptions { MaxDegreeOfParallelism = 8 },
                async (bookId, cancellationToken) =>
                    await cache.RemoveAsync($"books:{bookId}", cancellationToken));

            return CategoryResponse.FromEntity(category, bookIds.Count);
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await categoryRepository.GetByIdAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy thể loại với ID: {id}");

            if (await bookRepository.HasBooksInCategoryAsync(id))
                throw AppException.Conflict("Không thể xóa thể loại đang chứa các đầu sách.");

            await categoryRepository.DeleteAsync(category);
        }

        private async Task<string> CheckNameAsync(string name, Guid? currentId = null)
        {
            name = name.Trim();
            var existing = await categoryRepository.GetByNameAsync(name);
            if (existing is not null && existing.Id != currentId)
                throw AppException.Conflict($"Thể loại '{name}' đã tồn tại.");

            return name;
        }
    }
}
