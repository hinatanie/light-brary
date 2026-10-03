using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IBookCopyRepository
{
    Task<List<BookCopy>> GetByBookIdAsync(Guid bookId);
    Task<BookCopy?> GetByIdAsync(Guid id);
    Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null);
    Task<bool> BookExistsAsync(Guid bookId);
    Task<bool> HasBorrowHistoryAsync(Guid bookCopyId);
    Task<BookCopy> AddAsync(BookCopy bookCopy);
    Task UpdateAsync(BookCopy bookCopy);
    Task DeleteAsync(BookCopy bookCopy);
}
