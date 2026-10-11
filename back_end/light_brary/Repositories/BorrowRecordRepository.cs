using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.Repositories.Interfaces;
using light_brary.Entities;
using light_brary.Enums;
using light_brary.Data;

namespace light_brary.Repositories
{
    public class BorrowRecordRepository : GenericRepository<BorrowRecord>, IBorrowRecordRepository
    {
        public BorrowRecordRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BorrowRecord>> GetByUserIdAsync(Guid userId)
        {
            return await _context.BorrowRecords
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .Where(br => br.UserId == userId)
                .ToListAsync();
        }

        public async Task<BorrowRecord?> GetWithDetailsAsync(Guid id)
        {
            return await _context.BorrowRecords
                .Include(br => br.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .FirstOrDefaultAsync(br => br.Id == id);
        }

        public async Task<IEnumerable<BorrowRecord>> GetAllWithDetailsAsync(Guid? userId = null, string? status = null, bool? overdue = null)
        {
            var query = _context.BorrowRecords
                .Include(br => br.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Include(br => br.BorrowDetails)
                    .ThenInclude(bd => bd.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .AsQueryable();

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                query = query.Where(br => br.UserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BorrowRecordStatus>(status.Trim(), true, out var parsedStatus))
            {
                query = query.Where(br => br.Status == parsedStatus);
            }

            var list = await query
                .OrderByDescending(br => br.BorrowDate)
                .ThenByDescending(br => br.Id)
                .ToListAsync();

            if (overdue.HasValue)
            {
                var now = DateTime.UtcNow;
                if (overdue.Value)
                {
                    list = list.Where(br => br.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing) && now > br.DueDate).ToList();
                }
                else
                {
                    list = list.Where(br => !(br.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing) && now > br.DueDate)).ToList();
                }
            }

            return list;
        }
    }
}