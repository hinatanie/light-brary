using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllWithBookCountAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    Task<bool> HasBooksAsync(Guid id);
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
}
