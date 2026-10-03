using Library.Application.Common;
using Library.Application.DTOs.BorrowRecords;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Services;

public class BorrowRecordService : IBorrowRecordService
{
    private static readonly string[] ValidStatusFilters =
        { nameof(BorrowRecordStatus.Borrowing), nameof(BorrowRecordStatus.Returned), nameof(BorrowRecordStatus.Overdue) };

    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BorrowRecordService(IBorrowRecordRepository borrowRecordRepository, IUnitOfWork unitOfWork)
    {
        _borrowRecordRepository = borrowRecordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BorrowRecordResponse>> GetAllAsync(string? status = null)
    {
        string? normalizedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            normalizedStatus = ValidStatusFilters.FirstOrDefault(s => string.Equals(s, status.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? throw new BusinessException($"Trạng thái lọc không hợp lệ. Giá trị hợp lệ: {string.Join(", ", ValidStatusFilters)}.");
        }

        var now = DateTime.UtcNow;
        var records = await _borrowRecordRepository.GetAllAsync(normalizedStatus, now);
        return records.Select(r => MapToResponse(r, now)).ToList();
    }

    public async Task<BorrowRecordResponse> GetByIdAsync(Guid id)
    {
        var record = await _borrowRecordRepository.GetByIdWithDetailsAsync(id);
        if (record == null)
            throw new NotFoundException($"Không tìm thấy phiếu mượn với mã ID: {id}");
        return MapToResponse(record, DateTime.UtcNow);
    }

    public async Task<List<BorrowRecordResponse>> GetByUserIdAsync(Guid userId)
    {
        var userExists = await _borrowRecordRepository.UserExistsAsync(userId);
        if (!userExists)
            throw new NotFoundException($"Không tìm thấy người dùng với mã ID: {userId}");

        var now = DateTime.UtcNow;
        var records = await _borrowRecordRepository.GetByUserIdAsync(userId);
        return records.Select(r => MapToResponse(r, now)).ToList();
    }

    public async Task<BorrowRecordResponse> CreateAsync(CreateBorrowRequest request)
    {
        // 1. Validate đầu vào không cần khóa
        if (request.DueDate.ToUniversalTime() <= DateTime.UtcNow)
            throw new BusinessException("Hạn trả sách phải là ngày trong tương lai.");

        var copyIds = request.BookCopyIds.Distinct().ToList();
        if (copyIds.Count == 0)
            throw new BusinessException("Phải chọn ít nhất một bản sao sách để mượn.");
        if (copyIds.Count > AuthConstants.MaxCopiesPerBorrow)
            throw new BusinessException($"Mỗi phiếu mượn tối đa {AuthConstants.MaxCopiesPerBorrow} bản sao sách.");

        var userExists = await _borrowRecordRepository.UserExistsAsync(request.UserId);
        if (!userExists)
            throw new NotFoundException($"Không tìm thấy người dùng với mã ID: {request.UserId}");

        // 2. Khóa hàng các bản sao (SELECT ... FOR UPDATE) → kiểm tra → ghi, tất cả trong MỘT transaction.
        //    Request đồng thời sẽ chờ ở bước khóa, sau đó thấy trạng thái Borrowed và bị từ chối.
        //    Chỉ mục unique UX_BorrowDetails_ActiveCopy ở DB là lớp bảo vệ thứ hai.
        var borrowRecordId = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var copies = await _borrowRecordRepository.LockBookCopiesAsync(copyIds);

            var missingIds = copyIds.Except(copies.Select(c => c.Id)).ToList();
            if (missingIds.Count > 0)
                throw new BusinessException($"Các bản sao sách sau không tồn tại: {string.Join(", ", missingIds)}");

            var unavailable = copies.Where(c => c.Status != BookCopyStatus.Available).Select(c => c.Id).ToList();
            if (unavailable.Count > 0)
                throw new BusinessException(
                    $"Các bản sao sách sau đang được mượn hoặc không sẵn sàng: {string.Join(", ", unavailable)}");

            var record = new BorrowRecord
            {
                UserId = request.UserId,
                BorrowDate = DateTime.UtcNow,
                DueDate = request.DueDate.ToUniversalTime(),
                Status = BorrowRecordStatus.Borrowing,
                Note = request.Note?.Trim(),
                BorrowDetails = copies.Select(copy => new BorrowDetail
                {
                    BookCopyId = copy.Id,
                    Status = BorrowDetailStatus.Borrowing
                }).ToList()
            };

            foreach (var copy in copies)
                copy.Status = BookCopyStatus.Borrowed;

            var created = await _borrowRecordRepository.AddAsync(record);
            return created.Id;
        });

        return await GetByIdAsync(borrowRecordId);
    }

    public async Task<BorrowRecordResponse> ReturnBooksAsync(Guid borrowRecordId, ReturnBooksRequest request)
    {
        var duplicated = request.Items.GroupBy(i => i.BorrowDetailId).FirstOrDefault(g => g.Count() > 1);
        if (duplicated != null)
            throw new BusinessException($"Chi tiết mượn (ID: {duplicated.Key}) bị lặp trong yêu cầu trả sách.");

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Khóa phiếu mượn trước khi đọc để hai request trả sách đồng thời không ghi đè lên nhau.
            await _borrowRecordRepository.LockBorrowRecordAsync(borrowRecordId);

            var borrowRecord = await _borrowRecordRepository.GetByIdWithDetailsAsync(borrowRecordId)
                ?? throw new NotFoundException($"Không tìm thấy phiếu mượn với mã ID: {borrowRecordId}");

            if (borrowRecord.Status == BorrowRecordStatus.Returned)
                throw new BusinessException("Phiếu mượn này đã được trả đầy đủ.");

            var now = DateTime.UtcNow;
            foreach (var item in request.Items)
            {
                var detail = FindDetail(borrowRecord, item.BorrowDetailId, borrowRecordId);

                if (detail.Status == BorrowDetailStatus.Returned)
                    throw new BusinessException($"Cuốn sách trong chi tiết mượn (ID: {item.BorrowDetailId}) đã được trả rồi.");
                if (detail.Status == BorrowDetailStatus.Lost)
                    throw new BusinessException($"Cuốn sách trong chi tiết mượn (ID: {item.BorrowDetailId}) đã được báo mất.");

                detail.ReturnDate = now;
                detail.Status = BorrowDetailStatus.Returned;
                detail.FineAmount = item.FineAmount;

                if (detail.BookCopy != null)
                    detail.BookCopy.Status = BookCopyStatus.Available;
            }

            CloseRecordIfFinished(borrowRecord);
            await _borrowRecordRepository.UpdateAsync(borrowRecord);
            return 0;
        });

        return await GetByIdAsync(borrowRecordId);
    }

    public async Task<BorrowRecordResponse> MarkLostAsync(Guid borrowRecordId, Guid borrowDetailId, MarkLostRequest request)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _borrowRecordRepository.LockBorrowRecordAsync(borrowRecordId);

            var borrowRecord = await _borrowRecordRepository.GetByIdWithDetailsAsync(borrowRecordId)
                ?? throw new NotFoundException($"Không tìm thấy phiếu mượn với mã ID: {borrowRecordId}");

            var detail = FindDetail(borrowRecord, borrowDetailId, borrowRecordId);
            if (detail.Status != BorrowDetailStatus.Borrowing)
                throw new BusinessException("Chỉ có thể báo mất cuốn sách đang được mượn.");

            detail.Status = BorrowDetailStatus.Lost;
            detail.FineAmount = request.FineAmount;
            if (detail.BookCopy != null)
                detail.BookCopy.Status = BookCopyStatus.Lost;

            CloseRecordIfFinished(borrowRecord);
            await _borrowRecordRepository.UpdateAsync(borrowRecord);
            return 0;
        });

        return await GetByIdAsync(borrowRecordId);
    }

    private static BorrowDetail FindDetail(BorrowRecord record, Guid borrowDetailId, Guid borrowRecordId) =>
        record.BorrowDetails.FirstOrDefault(d => d.Id == borrowDetailId)
        ?? throw new NotFoundException($"Chi tiết mượn (ID: {borrowDetailId}) không thuộc phiếu mượn #{borrowRecordId}.");

    /// <summary>Phiếu đóng khi không còn cuốn nào đang mượn (mọi chi tiết là Returned hoặc Lost).</summary>
    private static void CloseRecordIfFinished(BorrowRecord record)
    {
        if (record.BorrowDetails.All(d => d.Status != BorrowDetailStatus.Borrowing))
            record.Status = BorrowRecordStatus.Returned;
    }

    private static BorrowRecordResponse MapToResponse(BorrowRecord record, DateTime nowUtc) => new(
        record.Id,
        record.UserId,
        record.User?.FullName ?? string.Empty,
        record.BorrowDate,
        record.DueDate,
        BorrowStatusCalculator.EffectiveStatus(record, nowUtc),
        record.Note,
        record.BorrowDetails.Select(d => new BorrowDetailResponse(
            d.Id,
            d.BookCopyId,
            d.BookCopy?.Barcode ?? string.Empty,
            d.BookCopy?.Book?.Title ?? string.Empty,
            d.BookCopy?.Book?.Author ?? string.Empty,
            d.ReturnDate,
            d.Status.ToString(),
            d.FineAmount,
            BorrowStatusCalculator.IsDetailOverdue(record, d, nowUtc)
        )).ToList()
    );
}
