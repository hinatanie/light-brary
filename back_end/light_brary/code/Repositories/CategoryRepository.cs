using Microsoft.EntityFrameworkCore;
using light_brary.Entities;
using light_brary.Data;
using light_brary.DTOs.Responses;

namespace light_brary.Repositories
{
    public class CategoryRepository(AppDbContext context) : GenericRepository<Category>(context)
    {
        public async Task<List<CategoryResponse>> GetAllWithBookCountsAsync(string? name)
        {
            return await _context.Categories
                .Where(c => name == null || c.Name.Contains(name))
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    BookCount = c.Books.Count
                })
                .ToListAsync();
        }

        public Task<Category?> GetByNameAsync(string name)
        {
            return _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<Category?> GetWithBooksAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
