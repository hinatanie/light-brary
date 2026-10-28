using Microsoft.EntityFrameworkCore;
using light_brary.Data;
using light_brary.Entities;

namespace light_brary.Repositories
{
    public class BorrowDetailRepository(AppDbContext context) : GenericRepository<BorrowDetail>(context)
    {
        public async Task<List<BorrowDetail>> GetByIdsWithDetailsAsync(List<Guid> ids)
        {
            return await _context.BorrowDetails
                .Include(d => d.BookCopy)
                .Include(d => d.BorrowRecord)
                    .ThenInclude(r => r.BorrowDetails)
                .Where(d => ids.Contains(d.Id))
                .ToListAsync();
        }

        public async Task<BorrowDetail?> GetWithDetailsAsync(Guid id)
        {
            return await _context.BorrowDetails
                .Include(d => d.BookCopy)
                .Include(d => d.BorrowRecord)
                    .ThenInclude(r => r.BorrowDetails)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
