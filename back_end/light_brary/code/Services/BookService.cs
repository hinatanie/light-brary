using light_brary.Repositories;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Enums;

namespace light_brary.Services
{
    public class BookService(
        BookRepository bookRepository,
        CategoryRepository categoryRepository)
    {
        public async Task<BookResponse> GetByIdAsync(Guid id)
        {
            var book = await bookRepository.GetWithDetailsAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy đầu sách với ID: {id}");

            return BookResponse.FromEntity(book);
        }

        public async Task<BookResponse> CreateAsync(SaveBookRequest request)
        {
            var category = await categoryRepository.GetByIdAsync(request.CategoryId)
                ?? throw AppException.NotFound($"Không tìm thấy thể loại với ID: {request.CategoryId}");

            var isbn = request.ISBN.Trim();
            var existingIsbn = await bookRepository.GetByIsbnAsync(isbn);
            if (existingIsbn is not null)
                throw AppException.Conflict($"Mã ISBN '{isbn}' đã tồn tại.");

            var book = new Book
            {
                Title = request.Title.Trim(),
                Author = request.Author.Trim(),
                ISBN = isbn,
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
                PublishYear = request.PublishYear,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            await bookRepository.AddAsync(book);

            book.Category = category;
            return BookResponse.FromEntity(book);
        }

        public async Task<BookResponse> UpdateAsync(Guid id, SaveBookRequest request)
        {
            var book = await bookRepository.GetWithDetailsAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy đầu sách với ID: {id}");

            var category = await categoryRepository.GetByIdAsync(request.CategoryId)
                ?? throw AppException.NotFound($"Không tìm thấy thể loại với ID: {request.CategoryId}");

            var isbn = request.ISBN.Trim();
            var existingIsbn = await bookRepository.GetByIsbnAsync(isbn);
            if (existingIsbn is not null && existingIsbn.Id != id)
                throw AppException.Conflict($"Mã ISBN '{isbn}' đã tồn tại.");

            book.Title = request.Title.Trim();
            book.Author = request.Author.Trim();
            book.ISBN = isbn;
            book.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            book.PublishYear = request.PublishYear;
            book.CategoryId = request.CategoryId;
            book.Category = category;

            await bookRepository.UpdateAsync(book);

            return BookResponse.FromEntity(book);
        }

        public async Task DeleteAsync(Guid id)
        {
            var book = await bookRepository.GetWithDetailsAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy đầu sách với ID: {id}");

            if (book.BookCopies.Any(bc => bc.Status == BookCopyStatus.Borrowed))
                throw AppException.Conflict("Không thể xóa đầu sách đang có bản sao đang được mượn.");

            await bookRepository.DeleteAsync(book);
        }

        public async Task<PagedResponse<BookResponse>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? keyword = null,
            Guid? categoryId = null,
            string? author = null,
            bool? onlyAvailable = null,
            string sortBy = "newest")
        {
            var (books, totalCount) = await bookRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                keyword: keyword,
                categoryId: categoryId,
                author: author,
                onlyAvailable: onlyAvailable,
                sortBy: sortBy);

            var responses = books.Select(BookResponse.FromEntity).ToList();

            return new PagedResponse<BookResponse>
            {
                Items = responses,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
