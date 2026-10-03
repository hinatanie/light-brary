using Library.Application.DTOs.BookCopies;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Services;

public class BookCopyService : IBookCopyService
{
    private readonly IBookCopyRepository _bookCopyRepository;

    public BookCopyService(IBookCopyRepository bookCopyRepository)
    {
        _bookCopyRepository = bookCopyRepository;
    }

    public async Task<List<BookCopyResponse>> GetByBookIdAsync(int bookId)
    {
        var copies = await _bookCopyRepository.GetByBookIdAsync(bookId);
        return copies.Select(MapToResponse).ToList();
    }

    public async Task<BookCopyResponse> GetByIdAsync(int id)
    {
        var copy = await _bookCopyRepository.GetByIdAsync(id);
        if (copy == null)
            throw new NotFoundException($"Không tìm thấy bản sao sách với mã ID: {id}");
        return MapToResponse(copy);
    }

    public async Task<BookCopyResponse> CreateAsync(CreateBookCopyRequest request)
    {
        var bookExists = await _bookCopyRepository.BookExistsAsync(request.BookId);
        if (!bookExists)
            throw new NotFoundException($"Không tìm thấy đầu sách với mã ID: {request.BookId}");

        var barcodeExists = await _bookCopyRepository.BarcodeExistsAsync(request.Barcode.Trim());
        if (barcodeExists)
            throw new ConflictException($"Mã vạch '{request.Barcode}' đã tồn tại trong hệ thống");

        var bookCopy = new BookCopy
        {
            BookId = request.BookId,
            Barcode = request.Barcode.Trim(),
            Status = BookCopyStatus.Available,
            ConditionNote = request.ConditionNote?.Trim()
        };

        var created = await _bookCopyRepository.AddAsync(bookCopy);
        var withBook = await _bookCopyRepository.GetByIdAsync(created.Id) ?? created;
        return MapToResponse(withBook);
    }

    public async Task<BookCopyResponse> UpdateConditionAsync(int id, UpdateBookCopyConditionRequest request)
    {
        var copy = await _bookCopyRepository.GetByIdAsync(id);
        if (copy == null)
            throw new NotFoundException($"Không tìm thấy bản sao sách với mã ID: {id}");

        copy.ConditionNote = request.ConditionNote?.Trim();
        await _bookCopyRepository.UpdateAsync(copy);

        var updated = await _bookCopyRepository.GetByIdAsync(id) ?? copy;
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var copy = await _bookCopyRepository.GetByIdAsync(id);
        if (copy == null)
            throw new NotFoundException($"Không tìm thấy bản sao sách với mã ID: {id}");

        if (copy.Status == BookCopyStatus.Borrowed)
            throw new BusinessException($"Bản sao sách (Barcode: {copy.Barcode}) đang được mượn, không thể xóa.");

        // Đã từng có lịch sử mượn (kể cả đã trả hoặc báo mất) thì không được xóa cứng để bảo toàn phiếu mượn.
        if (await _bookCopyRepository.HasBorrowHistoryAsync(id))
        {
            throw new ConflictException(
                $"Bản sao sách (Barcode: {copy.Barcode}) đã có lịch sử mượn nên không thể xóa.");
        }

        await _bookCopyRepository.DeleteAsync(copy);
    }

    private static BookCopyResponse MapToResponse(BookCopy bc) => new(
        bc.Id,
        bc.BookId,
        bc.Book?.Title ?? string.Empty,
        bc.Book?.Author ?? string.Empty,
        bc.Barcode,
        bc.Status.ToString(),
        bc.ConditionNote
    );
}
