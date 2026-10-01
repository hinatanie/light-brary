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

    public async Task<List<BorrowRecord>> GetAllAsync()
    {
        return await _context.BorrowRecords
            .Include(br => br.User)
            .Include(br => br.BorrowDetails)
                .ThenInclude(d => d.BookCopy)
                    .ThenInclude(bc => bc!.Book)
            .AsNoTracking()
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

    public async Task<List<BorrowRecord>> GetByUserIdAsync(string userId)
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

    public async Task<bool> UserExistsAsync(string userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }

    public async Task<List<BookCopy>> GetAvailableBookCopiesByIdsAsync(List<int> bookCopyIds)
    {
        return await _context.BookCopies
            .Where(bc => bookCopyIds.Contains(bc.Id) && bc.Status == BookCopyStatus.Available)
            .ToListAsync();
    }
}
