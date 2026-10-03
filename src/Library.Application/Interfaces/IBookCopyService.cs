using Library.Application.DTOs.BookCopies;

namespace Library.Application.Interfaces;

public interface IBookCopyService
{
    Task<List<BookCopyResponse>> GetByBookIdAsync(Guid bookId);
    Task<BookCopyResponse> GetByIdAsync(Guid id);
    Task<BookCopyResponse> CreateAsync(CreateBookCopyRequest request);
    Task<BookCopyResponse> UpdateConditionAsync(Guid id, UpdateBookCopyConditionRequest request);
    Task DeleteAsync(Guid id);
}
