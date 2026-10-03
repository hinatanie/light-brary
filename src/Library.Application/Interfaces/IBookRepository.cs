using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IBookRepository
{
    Task<List<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(Guid id);
    Task<bool> CategoryExistsAsync(Guid categoryId);
    Task<Book?> GetByIsbnAsync(string isbn);

    /// <summary>True nếu có bản sao nào của đầu sách đang được mượn hoặc từng có lịch sử mượn.</summary>
    Task<bool> HasBorrowHistoryAsync(Guid bookId);
    Task<Book> AddAsync(Book book);
    Task UpdateAsync(Book book);
    Task DeleteAsync(Book book);
}
