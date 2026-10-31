using Microsoft.EntityFrameworkCore;
using light_brary.Entities;
using light_brary.Data;

namespace light_brary.Repositories
{
    public class CategoryRepository(AppDbContext context) : GenericRepository<Category>(context)
    {
        public override Task<IEnumerable<Category>> GetAllAsync() => GetAllAsync(null);

        public async Task<IEnumerable<Category>> GetAllAsync(string? name)
        {
            return await _context.Categories
                .Include(c => c.Books)
                .Where(c => name == null || c.Name.Contains(name))
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
