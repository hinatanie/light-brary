using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class BorrowRecordRepository : IBorrowRecordRepository
{
    private readonly LibraryDbContext _context;

    public BorrowRecordRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<BorrowRecord>> GetAllAsync(string? status, DateTime nowUtc)
    {
        var query = _context.BorrowRecords
            .Include(br => br.User)
            .Include(br => br.BorrowDetails)
                .ThenInclude(d => d.BookCopy)
                    .ThenInclude(bc => bc!.Book)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (string.Equals(status, nameof(BorrowRecordStatus.Overdue), StringComparison.OrdinalIgnoreCase))
            {
                // Lọc phiếu quá hạn: phiếu đang mượn, hạn trả đã qua và còn ít nhất một cuốn chưa trả
                query = query.Where(br => br.Status == BorrowRecordStatus.Borrowing
                                          && br.DueDate < nowUtc
                                          && br.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing));
            }
            else if (string.Equals(status, nameof(BorrowRecordStatus.Borrowing), StringComparison.OrdinalIgnoreCase))
            {
                // Lọc phiếu đang mượn còn trong hạn (chưa quá hạn)
                query = query.Where(br => br.Status == BorrowRecordStatus.Borrowing
                                          && (br.DueDate >= nowUtc || !br.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing)));
            }
            else if (string.Equals(status, nameof(BorrowRecordStatus.Returned), StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(br => br.Status == BorrowRecordStatus.Returned);
            }
        }

        return await query
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync();
    }

    public async Task<BorrowRecord?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.BorrowRecords
            .Include(br => br.User)
            .Include(br => br.BorrowDetails)
                .ThenInclude(d => d.BookCopy)
                    .ThenInclude(bc => bc!.Book)
            .FirstOrDefaultAsync(br => br.Id == id);
    }

    public async Task<List<BorrowRecord>> GetByUserIdAsync(Guid userId)
    {
        return await _context.BorrowRecords
            .Include(br => br.User)
            .Include(br => br.BorrowDetails)
                .ThenInclude(d => d.BookCopy)
                    .ThenInclude(bc => bc!.Book)
            .AsNoTracking()
            .Where(br => br.UserId == userId)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync();
    }

    public async Task<BorrowRecord> AddAsync(BorrowRecord borrowRecord)
    {
        await _context.BorrowRecords.AddAsync(borrowRecord);
        await _context.SaveChangesAsync();
        return borrowRecord;
    }

    public async Task UpdateAsync(BorrowRecord borrowRecord)
    {
        _context.BorrowRecords.Update(borrowRecord);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }

    public async Task<List<BookCopy>> LockBookCopiesAsync(IReadOnlyCollection<int> bookCopyIds)
    {
        var sortedIds = bookCopyIds.OrderBy(id => id).ToArray();

        // Khóa các hàng bằng SELECT ... FOR UPDATE theo thứ tự ID cố định để triệt tiêu deadlock
        return await _context.BookCopies
            .FromSqlInterpolated($"SELECT * FROM \"BookCopies\" WHERE \"Id\" = ANY({sortedIds}) ORDER BY \"Id\" FOR UPDATE")
            .ToListAsync();
    }

    public async Task LockBorrowRecordAsync(int borrowRecordId)
    {
        // Khóa hàng phiếu mượn bằng SELECT ... FOR UPDATE để bảo vệ thao tác trả sách và báo mất đồng thời
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM \"BorrowRecords\" WHERE \"Id\" = {borrowRecordId} FOR UPDATE");
    }
}
