using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.Repositories.Interfaces;
using light_brary.Entities;
using light_brary.Data;

namespace light_brary.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<Category?> GetWithBooksAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}