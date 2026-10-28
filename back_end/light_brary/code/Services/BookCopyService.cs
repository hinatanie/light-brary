using light_brary.Repositories;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Enums;
using Microsoft.Extensions.Caching.Distributed;

namespace light_brary.Services
{
    public class BookCopyService(
        BookCopyRepository bookCopyRepository,
        BookRepository bookRepository,
        IDistributedCache cache)
    {
        public async Task<IEnumerable<BookCopyResponse>> GetAllAsync()
        {
            var copies = await bookCopyRepository.GetAllWithBookAsync();
            return copies.Select(BookCopyResponse.FromEntity).ToList();
        }

        public async Task<IEnumerable<BookCopyResponse>> GetByBookIdAsync(Guid bookId)
        {
            var book = await bookRepository.GetByIdAsync(bookId)
                ?? throw AppException.NotFound($"Không tìm thấy đầu sách với ID: {bookId}");

            var copies = await bookCopyRepository.GetByBookIdAsync(bookId);
            return copies.Select(BookCopyResponse.FromEntity).ToList();
        }

        public async Task<BookCopyResponse> GetByIdAsync(Guid id)
        {
            var copy = await bookCopyRepository.GetWithBookAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy bản sao sách với ID: {id}");

            return BookCopyResponse.FromEntity(copy);
        }

        public async Task<BookCopyResponse> GetByBarcodeAsync(string barcode)
        {
            var code = barcode.Trim();
            var copy = await bookCopyRepository.GetByBarcodeAsync(code)
                ?? throw AppException.NotFound($"Không tìm thấy bản sao sách với mã vạch: {code}");

            return BookCopyResponse.FromEntity(copy);
        }

        public async Task<BookCopyResponse> CreateAsync(Guid bookId, SaveBookCopyRequest request)
        {
            var book = await bookRepository.GetByIdAsync(bookId)
                ?? throw AppException.NotFound($"Không tìm thấy đầu sách với ID: {bookId}");

            var barcode = request.Barcode.Trim();
            var existing = await bookCopyRepository.GetByBarcodeAsync(barcode);
            if (existing is not null)
                throw AppException.Conflict($"Mã vạch '{barcode}' đã tồn tại.");

            var copy = new BookCopy
            {
                BookId = bookId,
                Barcode = barcode,
                ConditionNote = request.ConditionNote?.Trim(),
                Status = BookCopyStatus.Available
            };

            await bookCopyRepository.AddAsync(copy);
            await cache.RemoveAsync($"books:{bookId}");

            copy.Book = book;
            return BookCopyResponse.FromEntity(copy);
        }

        public async Task<BookCopyResponse> UpdateAsync(Guid id, SaveBookCopyRequest request)
        {
            var copy = await bookCopyRepository.GetWithBookAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy bản sao sách với ID: {id}");

            var barcode = request.Barcode.Trim();
            var existing = await bookCopyRepository.GetByBarcodeAsync(barcode);
            if (existing is not null && existing.Id != id)
                throw AppException.Conflict($"Mã vạch '{barcode}' đã tồn tại.");

            copy.Barcode = barcode;
            copy.ConditionNote = request.ConditionNote?.Trim();

            await bookCopyRepository.UpdateAsync(copy);

            return BookCopyResponse.FromEntity(copy);
        }

        public async Task DeleteAsync(Guid id)
        {
            var copy = await bookCopyRepository.GetByIdAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy bản sao sách với ID: {id}");

            var hasBorrowHistory = await bookCopyRepository.HasBorrowHistoryAsync(id);
            if (hasBorrowHistory)
                throw AppException.Conflict("Không thể xóa bản sao sách đã có lịch sử mượn.");

            await bookCopyRepository.DeleteAsync(copy);
            await cache.RemoveAsync($"books:{copy.BookId}");
        }


    }
}
