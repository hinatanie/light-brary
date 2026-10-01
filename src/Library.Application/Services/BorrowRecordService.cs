using Library.Application.DTOs.BorrowRecords;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Services;

public class BorrowRecordService : IBorrowRecordService
{
    private readonly IBorrowRecordRepository _borrowRecordRepository;

    public BorrowRecordService(IBorrowRecordRepository borrowRecordRepository)
    {
        _borrowRecordRepository = borrowRecordRepository;
    }

    public async Task<List<BorrowRecordResponse>> GetAllAsync()
    {
        var records = await _borrowRecordRepository.GetAllAsync();
        return records.Select(MapToResponse).ToList();
    }

    public async Task<BorrowRecordResponse> GetByIdAsync(int id)
    {
        var record = await _borrowRecordRepository.GetByIdWithDetailsAsync(id);
        if (record == null)
            throw new NotFoundException($"Không tìm thấy phiếu mượn với mã ID: {id}");
        return MapToResponse(record);
    }

    public async Task<List<BorrowRecordResponse>> GetByUserIdAsync(string userId)
    {
        var userExists = await _borrowRecordRepository.UserExistsAsync(userId);
        if (!userExists)
            throw new NotFoundException($"Không tìm thấy người dùng với mã ID: {userId}");

        var records = await _borrowRecordRepository.GetByUserIdAsync(userId);
        return records.Select(MapToResponse).ToList();
    }

    public async Task<BorrowRecordResponse> CreateAsync(CreateBorrowRequest request)
    {
        // 1. Validate user tồn tại
        var userExists = await _borrowRecordRepository.UserExistsAsync(request.UserId);
        if (!userExists)
            throw new NotFoundException($"Không tìm thấy người dùng với mã ID: {request.UserId}");

        // 2. Validate ngày trả phải sau ngày mượn
        if (request.DueDate.ToUniversalTime() <= DateTime.UtcNow)
            throw new BusinessException("Hạn trả sách phải là ngày trong tương lai.");

        // 3. Validate tất cả BookCopy phải tồn tại và có trạng thái Available
        var availableCopies = await _borrowRecordRepository.GetAvailableBookCopiesByIdsAsync(request.BookCopyIds);

        var unavailableIds = request.BookCopyIds.Except(availableCopies.Select(c => c.Id)).ToList();
        if (unavailableIds.Count > 0)
        {
            throw new BusinessException(
                $"Các bản sao sách sau không tồn tại hoặc đang được mượn: {string.Join(", ", unavailableIds)}");
        }

        // 4. Tạo BorrowRecord + BorrowDetails + cập nhật trạng thái BookCopy (trong một transaction)
        var now = DateTime.UtcNow;
        var borrowRecord = new BorrowRecord
        {
            UserId = request.UserId,
            BorrowDate = now,
            DueDate = request.DueDate.ToUniversalTime(),
            Status = BorrowRecordStatus.Borrowing,
            Note = request.Note?.Trim(),
            BorrowDetails = availableCopies.Select(copy => new BorrowDetail
            {
                BookCopyId = copy.Id,
                Status = BorrowDetailStatus.Borrowing
            }).ToList()
        };

        // Cập nhật trạng thái tất cả bản sao sách thành Borrowed
        foreach (var copy in availableCopies)
        {
            copy.Status = BookCopyStatus.Borrowed;
        }

        var created = await _borrowRecordRepository.AddAsync(borrowRecord);

        // Load lại để có đầy đủ navigation properties
        var withDetails = await _borrowRecordRepository.GetByIdWithDetailsAsync(created.Id) ?? created;
        return MapToResponse(withDetails);
    }

    public async Task<BorrowRecordResponse> ReturnBooksAsync(int borrowRecordId, ReturnBooksRequest request)
    {
        // 1. Validate phiếu mượn tồn tại
        var borrowRecord = await _borrowRecordRepository.GetByIdWithDetailsAsync(borrowRecordId);
        if (borrowRecord == null)
            throw new NotFoundException($"Không tìm thấy phiếu mượn với mã ID: {borrowRecordId}");

        if (borrowRecord.Status == BorrowRecordStatus.Returned)
            throw new BusinessException("Phiếu mượn này đã được trả đầy đủ.");

        // 2. Xử lý từng cuốn cần trả
        var now = DateTime.UtcNow;
        foreach (var item in request.Items)
        {
            var detail = borrowRecord.BorrowDetails.FirstOrDefault(d => d.Id == item.BorrowDetailId);
            if (detail == null)
                throw new NotFoundException(
                    $"Chi tiết mượn (ID: {item.BorrowDetailId}) không thuộc phiếu mượn #{borrowRecordId}.");

            if (detail.Status == BorrowDetailStatus.Returned)
                throw new BusinessException(
                    $"Cuốn sách trong chi tiết mượn (ID: {item.BorrowDetailId}) đã được trả rồi.");

            // Cập nhật BorrowDetail
            detail.ReturnDate = now;
            detail.Status = BorrowDetailStatus.Returned;
            detail.FineAmount = item.FineAmount;

            // Cập nhật BookCopy → Available
            if (detail.BookCopy != null)
                detail.BookCopy.Status = BookCopyStatus.Available;
        }

        // 3. Nếu tất cả chi tiết đã trả → đánh dấu phiếu mượn là Returned
        var allReturned = borrowRecord.BorrowDetails.All(d => d.Status == BorrowDetailStatus.Returned);
        if (allReturned)
            borrowRecord.Status = BorrowRecordStatus.Returned;

        await _borrowRecordRepository.UpdateAsync(borrowRecord);

        var updated = await _borrowRecordRepository.GetByIdWithDetailsAsync(borrowRecordId) ?? borrowRecord;
        return MapToResponse(updated);
    }

    private static BorrowRecordResponse MapToResponse(BorrowRecord record) => new(
        record.Id,
        record.UserId,
        record.User?.FullName ?? string.Empty,
        record.BorrowDate,
        record.DueDate,
        record.Status.ToString(),
        record.Note,
        record.BorrowDetails.Select(d => new BorrowDetailResponse(
            d.Id,
            d.BookCopyId,
            d.BookCopy?.Barcode ?? string.Empty,
            d.BookCopy?.Book?.Title ?? string.Empty,
            d.BookCopy?.Book?.Author ?? string.Empty,
            d.ReturnDate,
            d.Status.ToString(),
            d.FineAmount
        )).ToList()
    );
}
