using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using light_brary.Application.DTOs.Books;
using light_brary.Application.Exceptions;
using light_brary.Application.Interfaces;
using light_brary.Domain.Entities;
using light_brary.Domain.Enums;

namespace light_brary.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;

        public BookService(IBookRepository bookRepository, ICategoryRepository categoryRepository)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<BookResponse>> GetAllAsync(string? keyword = null, int? categoryId = null, string? author = null, bool? onlyAvailable = null)
        {
            var books = await _bookRepository.GetAllWithDetailsAsync(keyword, categoryId, author);
            var responses = books.Select(MapToResponse);
            if (onlyAvailable == true)
            {
                responses = responses.Where(b => b.AvailableCopies > 0);
            }
            return responses.ToList();
        }

        public async Task<BookResponse> GetByIdAsync(int id)
        {
            var book = await _bookRepository.GetWithDetailsAsync(id);
            if (book is null)
                throw new NotFoundException($"Không tìm thấy đầu sách với ID: {id}");

            return MapToResponse(book);
        }

        public async Task<BookResponse> CreateAsync(SaveBookRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category is null)
                throw new NotFoundException($"Không tìm thấy thể loại với ID: {request.CategoryId}");

            var isbn = request.ISBN.Trim();
            var existingIsbn = await _bookRepository.GetByIsbnAsync(isbn);
            if (existingIsbn is not null)
                throw new ConflictException($"Mã ISBN '{isbn}' đã tồn tại.");

            var book = new Book
            {
                Title = request.Title.Trim(),
                Author = request.Author.Trim(),
                ISBN = isbn,
                PublishYear = request.PublishYear,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            await _bookRepository.AddAsync(book);

            book.Category = category;
            return MapToResponse(book);
        }

        public async Task<BookResponse> UpdateAsync(int id, SaveBookRequest request)
        {
            var book = await _bookRepository.GetWithDetailsAsync(id);
            if (book is null)
                throw new NotFoundException($"Không tìm thấy đầu sách với ID: {id}");

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category is null)
                throw new NotFoundException($"Không tìm thấy thể loại với ID: {request.CategoryId}");

            var isbn = request.ISBN.Trim();
            var existingIsbn = await _bookRepository.GetByIsbnAsync(isbn);
            if (existingIsbn is not null && existingIsbn.Id != id)
                throw new ConflictException($"Mã ISBN '{isbn}' đã tồn tại.");

            book.Title = request.Title.Trim();
            book.Author = request.Author.Trim();
            book.ISBN = isbn;
            book.PublishYear = request.PublishYear;
            book.CategoryId = request.CategoryId;
            book.Category = category;

            await _bookRepository.UpdateAsync(book);

            return MapToResponse(book);
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _bookRepository.GetWithDetailsAsync(id);
            if (book is null)
                throw new NotFoundException($"Không tìm thấy đầu sách với ID: {id}");

            if (book.BookCopies != null && book.BookCopies.Any())
                throw new ConflictException("Không thể xóa đầu sách đang có bản sao vật lý.");

            await _bookRepository.DeleteAsync(book);
        }

        private static BookResponse MapToResponse(Book book)
        {
            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishYear = book.PublishYear,
                CategoryId = book.CategoryId,
                CategoryName = book.Category?.Name ?? string.Empty,
                Category = book.Category != null ? new Category
                {
                    Id = book.Category.Id,
                    Name = book.Category.Name,
                    Description = book.Category.Description
                } : null!,
                TotalCopies = book.BookCopies?.Count ?? 0,
                AvailableCopies = book.BookCopies?.Count(c => c.Status == BookCopyStatus.Available) ?? 0,
                CreatedAt = book.CreatedAt
            };
        }
    }
}