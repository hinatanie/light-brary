using Library.Application.DTOs.Books;

namespace Library.Application.Interfaces;

public interface IBookService
{
    Task<List<BookResponse>> GetAllBooksAsync();
    Task<BookResponse> GetBookByIdAsync(Guid id);
    Task<BookResponse> CreateBookAsync(CreateBookRequest request);
    Task<BookResponse> UpdateBookAsync(Guid id, UpdateBookRequest request);
    Task DeleteBookAsync(Guid id);
}
