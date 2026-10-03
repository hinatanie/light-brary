using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IBookCopyRepository
{
    Task<List<BookCopy>> GetByBookIdAsync(int bookId);
    Task<BookCopy?> GetByIdAsync(int id);
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);
    Task<bool> BookExistsAsync(int bookId);
    Task<bool> HasBorrowHistoryAsync(int bookCopyId);
    Task<BookCopy> AddAsync(BookCopy bookCopy);
    Task UpdateAsync(BookCopy bookCopy);
    Task DeleteAsync(BookCopy bookCopy);
}
