using Library.Application.DTOs.BookCopies;

namespace Library.Application.Interfaces;

public interface IBookCopyService
{
    Task<List<BookCopyResponse>> GetByBookIdAsync(int bookId);
    Task<BookCopyResponse> GetByIdAsync(int id);
    Task<BookCopyResponse> CreateAsync(CreateBookCopyRequest request);
    Task<BookCopyResponse> UpdateConditionAsync(int id, UpdateBookCopyConditionRequest request);
    Task DeleteAsync(int id);
}
