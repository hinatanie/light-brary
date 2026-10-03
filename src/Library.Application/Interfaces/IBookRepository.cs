using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IBookRepository
{
    Task<List<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task<Book?> GetByIsbnAsync(string isbn);

    /// <summary>True nếu có bản sao nào của đầu sách đang được mượn hoặc từng có lịch sử mượn.</summary>
    Task<bool> HasBorrowHistoryAsync(int bookId);
    Task<Book> AddAsync(Book book);
    Task UpdateAsync(Book book);
    Task DeleteAsync(Book book);
}
