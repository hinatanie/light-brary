using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllWithBookCountAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> HasBooksAsync(int id);
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
}
