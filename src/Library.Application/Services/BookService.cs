using Library.Application.Exceptions;
using Library.Application.DTOs.Books;
using Library.Application.Interfaces;
using Library.Application.Validation;
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

    public async Task<BookResponse> GetBookByIdAsync(Guid id)
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

        var isbn = IsbnNormalizer.Normalize(request.Isbn);
        await EnsureIsbnAvailableAsync(isbn, excludeBookId: null);

        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Isbn = isbn,
            PublishYear = request.PublishYear,
            ImageUrl = request.ImageUrl?.Trim(),
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        var createdBook = await _bookRepository.AddAsync(book);
        var bookWithCategory = await _bookRepository.GetByIdAsync(createdBook.Id) ?? createdBook;
        return MapToResponse(bookWithCategory);
    }

    public async Task<BookResponse> UpdateBookAsync(Guid id, UpdateBookRequest request)
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

        var isbn = IsbnNormalizer.Normalize(request.Isbn);
        await EnsureIsbnAvailableAsync(isbn, excludeBookId: id);

        existingBook.Title = request.Title.Trim();
        existingBook.Author = request.Author.Trim();
        existingBook.Isbn = isbn;
        existingBook.PublishYear = request.PublishYear;
        existingBook.ImageUrl = request.ImageUrl?.Trim();
        existingBook.CategoryId = request.CategoryId;

        await _bookRepository.UpdateAsync(existingBook);

        var updatedBook = await _bookRepository.GetByIdAsync(id) ?? existingBook;
        return MapToResponse(updatedBook);
    }

    public async Task DeleteBookAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            throw new NotFoundException($"Không tìm thấy sách với mã ID: {id}");
        }

        // Sách (hoặc bản sao của nó) đã từng được mượn thì phải giữ lại để bảo toàn lịch sử phiếu mượn.
        if (await _bookRepository.HasBorrowHistoryAsync(id))
        {
            throw new ConflictException(
                $"Đầu sách '{book.Title}' có bản sao đang được mượn hoặc đã có lịch sử mượn nên không thể xóa.");
        }

        await _bookRepository.DeleteAsync(book);
    }

    /// <summary>Kiểm tra trước để báo lỗi dễ hiểu; chỉ mục unique ở DB là lớp bảo vệ khi có request đồng thời.</summary>
    private async Task EnsureIsbnAvailableAsync(string? isbn, Guid? excludeBookId)
    {
        if (isbn == null) return;

        var existing = await _bookRepository.GetByIsbnAsync(isbn);
        if (existing != null && existing.Id != excludeBookId)
        {
            throw new ConflictException(
                $"ISBN '{isbn}' đã tồn tại ở đầu sách #{existing.Id} \"{existing.Title}\". " +
                "Hãy thêm bản sao (BookCopy) vào đầu sách đó thay vì tạo đầu sách mới.");
        }
    }

    private static BookResponse MapToResponse(Book book)
    {
        return new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn,
            book.PublishYear,
            book.ImageUrl,
            book.CategoryId,
            book.Category?.Name,
            book.CreatedAt
        );
    }
}
