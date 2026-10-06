using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.Application.Interfaces;
using light_brary.Domain.Entities;
using light_brary.Infrastructure.Persistence;

namespace light_brary.Infrastructure.Repositories
{
    public class BorrowRecordRepository : GenericRepository<BorrowRecord>, IBorrowRecordRepository
    {
        public BorrowRecordRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BorrowRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.BorrowRecords
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .Where(br => br.UserId == userId)
                .ToListAsync();
        }

        public async Task<BorrowRecord?> GetWithDetailsAsync(int id)
        {
            return await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .FirstOrDefaultAsync(br => br.Id == id);
        }
    }
}