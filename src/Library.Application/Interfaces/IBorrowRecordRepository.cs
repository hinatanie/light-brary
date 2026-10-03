using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IBorrowRecordRepository
{
    /// <param name="status">null/"" = tất cả; "Overdue" = quá hạn (suy ra); "Borrowing" = đang mượn chưa quá hạn; "Returned".</param>
    Task<List<BorrowRecord>> GetAllAsync(string? status, DateTime nowUtc);
    Task<BorrowRecord?> GetByIdWithDetailsAsync(int id);
    Task<List<BorrowRecord>> GetByUserIdAsync(Guid userId);
    Task<BorrowRecord> AddAsync(BorrowRecord borrowRecord);
    Task UpdateAsync(BorrowRecord borrowRecord);
    Task<bool> UserExistsAsync(Guid userId);

    /// <summary>
    /// Khóa hàng (SELECT ... FOR UPDATE) các bản sao theo thứ tự Id. BẮT BUỘC gọi bên trong transaction.
    /// Trả về các bản sao tồn tại (đã bị khóa) với trạng thái hiện tại mới nhất.
    /// </summary>
    Task<List<BookCopy>> LockBookCopiesAsync(IReadOnlyCollection<int> bookCopyIds);

    /// <summary>Khóa hàng phiếu mượn (SELECT ... FOR UPDATE). BẮT BUỘC gọi bên trong transaction.</summary>
    Task LockBorrowRecordAsync(int borrowRecordId);
}
