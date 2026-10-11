using light_brary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Enums;

namespace light_brary.Services
{
    public class BookCopyService(
        BookCopyRepository bookCopyRepository,
        BookRepository bookRepository)
    {
        public async Task<IEnumerable<BookCopyResponse>> GetByBookIdAsync(Guid bookId)
        {
            var book = await bookRepository.GetByIdAsync(bookId);
            if (book is null)
                throw new NotFoundException($"Không tìm thấy đầu sách với ID: {bookId}");

            var copies = await bookCopyRepository.GetByBookIdAsync(bookId);
            return copies.Select(MapToResponse).ToList();
        }

        public async Task<BookCopyResponse> GetByIdAsync(Guid id)
        {
            var copy = await bookCopyRepository.GetWithBookAsync(id);
            if (copy is null)
                throw new NotFoundException($"Không tìm thấy bản sao sách với ID: {id}");

            return MapToResponse(copy);
        }

        public async Task<BookCopyResponse> GetByBarcodeAsync(string barcode)
        {
            var code = barcode.Trim();
            var copy = await bookCopyRepository.GetByBarcodeAsync(code);
            if (copy is null)
                throw new NotFoundException($"Không tìm thấy bản sao sách với mã vạch: {code}");

            return MapToResponse(copy);
        }

        public async Task<BookCopyResponse> CreateAsync(Guid bookId, SaveBookCopyRequest request)
        {
            var book = await bookRepository.GetByIdAsync(bookId);
            if (book is null)
                throw new NotFoundException($"Không tìm thấy đầu sách với ID: {bookId}");

            var barcode = request.Barcode.Trim();
            var existing = await bookCopyRepository.GetByBarcodeAsync(barcode);
            if (existing is not null)
                throw new ConflictException($"Mã vạch '{barcode}' đã tồn tại.");

            var copy = new BookCopy
            {
                BookId = bookId,
                Barcode = barcode,
                ConditionNote = request.ConditionNote?.Trim(),
                Status = BookCopyStatus.Available
            };

            await bookCopyRepository.AddAsync(copy);

            copy.Book = book;
            return MapToResponse(copy);
        }

        public async Task<BookCopyResponse> UpdateAsync(Guid id, SaveBookCopyRequest request)
        {
            var copy = await bookCopyRepository.GetWithBookAsync(id);
            if (copy is null)
                throw new NotFoundException($"Không tìm thấy bản sao sách với ID: {id}");

            var barcode = request.Barcode.Trim();
            var existing = await bookCopyRepository.GetByBarcodeAsync(barcode);
            if (existing is not null && existing.Id != id)
                throw new ConflictException($"Mã vạch '{barcode}' đã tồn tại.");

            copy.Barcode = barcode;
            copy.ConditionNote = request.ConditionNote?.Trim();

            await bookCopyRepository.UpdateAsync(copy);

            return MapToResponse(copy);
        }

        public async Task DeleteAsync(Guid id)
        {
            var copy = await bookCopyRepository.GetByIdAsync(id);
            if (copy is null)
                throw new NotFoundException($"Không tìm thấy bản sao sách với ID: {id}");

            var hasBorrowHistory = await bookCopyRepository.HasBorrowHistoryAsync(id);
            if (hasBorrowHistory)
                throw new ConflictException("Không thể xóa bản sao sách đã có lịch sử mượn.");

            await bookCopyRepository.DeleteAsync(copy);
        }

        public async Task<BookCopyResponse> MarkAvailableAsync(Guid id)
        {
            var copy = await bookCopyRepository.GetWithBookAsync(id);
            if (copy is null)
                throw new NotFoundException($"Không tìm thấy bản sao sách với ID: {id}");

            if (copy.Status == BookCopyStatus.Available)
                throw new ConflictException("Bản sao sách hiện đã ở trạng thái Available.");

            if (copy.Status == BookCopyStatus.Borrowed)
                throw new ConflictException("Bản sao đang được mượn, vui lòng thực hiện thông qua nghiệp vụ trả sách.");

            copy.Status = BookCopyStatus.Available;
            await bookCopyRepository.UpdateAsync(copy);

            return MapToResponse(copy);
        }

        public async Task<BookCopyResponse> MarkLostAsync(Guid id)
        {
            var copy = await bookCopyRepository.GetWithBookAsync(id);
            if (copy is null)
                throw new NotFoundException($"Không tìm thấy bản sao sách với ID: {id}");

            if (copy.Status == BookCopyStatus.Lost)
                throw new ConflictException("Bản sao sách hiện đã ở trạng thái Lost.");

            if (copy.Status == BookCopyStatus.Borrowed)
                throw new ConflictException("Bản sao đang được mượn, vui lòng báo mất thông qua chi tiết phiếu mượn.");

            copy.Status = BookCopyStatus.Lost;
            await bookCopyRepository.UpdateAsync(copy);

            return MapToResponse(copy);
        }

        private static BookCopyResponse MapToResponse(BookCopy copy)
        {
            return new BookCopyResponse
            {
                Id = copy.Id,
                BookId = copy.BookId,
                BookTitle = copy.Book?.Title ?? string.Empty,
                Barcode = copy.Barcode,
                Status = copy.Status.ToString(),
                ConditionNote = copy.ConditionNote
            };
        }
    }
}