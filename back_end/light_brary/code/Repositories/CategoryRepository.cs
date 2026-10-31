using Microsoft.EntityFrameworkCore;
using light_brary.Entities;
using light_brary.Data;
using light_brary.DTOs.Responses;

namespace light_brary.Repositories
{
    public class CategoryRepository(AppDbContext context) : GenericRepository<Category>(context)
    {
        private IQueryable<CategoryResponse> CategoriesWithBookCounts => _context.Categories
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                BookCount = c.Books.Count
            });

        public async Task<List<CategoryResponse>> GetAllWithBookCountsAsync(string? name)
        {
            return await CategoriesWithBookCounts
                .Where(c => name == null || c.Name.Contains(name))
                .ToListAsync();
        }

        public Task<Category?> GetByNameAsync(string name)
        {
            return _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        }

        public Task<CategoryResponse?> GetByIdWithBookCountAsync(Guid id)
        {
            return CategoriesWithBookCounts
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
