using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IBorrowRecordRepository
{
    Task<List<BorrowRecord>> GetAllAsync();
    Task<BorrowRecord?> GetByIdWithDetailsAsync(int id);
    Task<List<BorrowRecord>> GetByUserIdAsync(string userId);
    Task<BorrowRecord> AddAsync(BorrowRecord borrowRecord);
    Task UpdateAsync(BorrowRecord borrowRecord);
    Task<bool> UserExistsAsync(string userId);
    Task<List<BookCopy>> GetAvailableBookCopiesByIdsAsync(List<int> bookCopyIds);
}
