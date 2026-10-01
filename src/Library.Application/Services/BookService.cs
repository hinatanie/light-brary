using Library.Application.Exceptions;
using Library.Application.DTOs.Books;
using Library.Application.Interfaces;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<List<BookResponse>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(MapToResponse).ToList();
    }

    public async Task<BookResponse> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            throw new NotFoundException($"Không tìm thấy sách với mã ID: {id}");
        }

        return MapToResponse(book);
    }

    public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
    {
        var categoryExists = await _bookRepository.CategoryExistsAsync(request.CategoryId);
        if (!categoryExists)
        {
            throw new NotFoundException($"Thể loại sách (CategoryId: {request.CategoryId}) không tồn tại");
        }

        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Isbn = request.Isbn?.Trim(),
            PublishYear = request.PublishYear,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        var createdBook = await _bookRepository.AddAsync(book);
        var bookWithCategory = await _bookRepository.GetByIdAsync(createdBook.Id) ?? createdBook;
        return MapToResponse(bookWithCategory);
    }

    public async Task<BookResponse> UpdateBookAsync(int id, UpdateBookRequest request)
    {
        var existingBook = await _bookRepository.GetByIdAsync(id);
        if (existingBook == null)
        {
            throw new NotFoundException($"Không tìm thấy sách với mã ID: {id}");
        }

        if (existingBook.CategoryId != request.CategoryId)
        {
            var categoryExists = await _bookRepository.CategoryExistsAsync(request.CategoryId);
            if (!categoryExists)
            {
                throw new NotFoundException($"Thể loại sách (CategoryId: {request.CategoryId}) không tồn tại");
            }
        }

        existingBook.Title = request.Title.Trim();
        existingBook.Author = request.Author.Trim();
        existingBook.Isbn = request.Isbn?.Trim();
        existingBook.PublishYear = request.PublishYear;
        existingBook.CategoryId = request.CategoryId;

        await _bookRepository.UpdateAsync(existingBook);

        var updatedBook = await _bookRepository.GetByIdAsync(id) ?? existingBook;
        return MapToResponse(updatedBook);
    }

    public async Task DeleteBookAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            throw new NotFoundException($"Không tìm thấy sách với mã ID: {id}");
        }

        await _bookRepository.DeleteAsync(book);
    }

    private static BookResponse MapToResponse(Book book)
    {
        return new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn,
            book.PublishYear,
            book.CategoryId,
            book.Category?.Name,
            book.CreatedAt
        );
    }
}
