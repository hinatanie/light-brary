using Library.Application.DTOs.BorrowRecords;

namespace Library.Application.Interfaces;

public interface IBorrowRecordService
{
    Task<List<BorrowRecordResponse>> GetAllAsync(string? status = null);
    Task<BorrowRecordResponse> GetByIdAsync(Guid id);
    Task<List<BorrowRecordResponse>> GetByUserIdAsync(Guid userId);
    Task<BorrowRecordResponse> CreateAsync(CreateBorrowRequest request);
    Task<BorrowRecordResponse> ReturnBooksAsync(Guid borrowRecordId, ReturnBooksRequest request);
    Task<BorrowRecordResponse> MarkLostAsync(Guid borrowRecordId, Guid borrowDetailId, MarkLostRequest request);
}
