using Library.Application.DTOs.Books;

namespace Library.Application.Interfaces;

public interface IBookService
{
    Task<List<BookResponse>> GetAllBooksAsync();
    Task<BookResponse> GetBookByIdAsync(int id);
    Task<BookResponse> CreateBookAsync(CreateBookRequest request);
    Task<BookResponse> UpdateBookAsync(int id, UpdateBookRequest request);
    Task DeleteBookAsync(int id);
}
