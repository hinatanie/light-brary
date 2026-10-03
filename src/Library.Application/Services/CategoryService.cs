using Library.Application.DTOs.Categories;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllWithBookCountAsync();
        return categories.Select(c => MapToResponse(c, c.Books.Count)).ToList();
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException($"Không tìm thấy thể loại với mã ID: {id}");

        return MapToResponse(category, category.Books.Count);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var nameExists = await _categoryRepository.ExistsByNameAsync(request.Name.Trim());
        if (nameExists)
            throw new ConflictException($"Thể loại '{request.Name}' đã tồn tại trong hệ thống");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        var created = await _categoryRepository.AddAsync(category);
        return MapToResponse(created, 0);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException($"Không tìm thấy thể loại với mã ID: {id}");

        var nameExists = await _categoryRepository.ExistsByNameAsync(request.Name.Trim(), excludeId: id);
        if (nameExists)
            throw new ConflictException($"Thể loại '{request.Name}' đã tồn tại trong hệ thống");

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();

        await _categoryRepository.UpdateAsync(category);

        var updated = await _categoryRepository.GetByIdAsync(id) ?? category;
        return MapToResponse(updated, updated.Books.Count);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException($"Không tìm thấy thể loại với mã ID: {id}");

        var hasBooks = await _categoryRepository.HasBooksAsync(id);
        if (hasBooks)
            throw new BusinessException($"Thể loại '{category.Name}' đang chứa sách. Vui lòng xóa hoặc chuyển sách sang thể loại khác trước.");

        await _categoryRepository.DeleteAsync(category);
    }

    private static CategoryResponse MapToResponse(Category c, int bookCount)
        => new(c.Id, c.Name, c.Description, bookCount);
}
