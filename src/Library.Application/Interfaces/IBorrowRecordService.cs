using Library.Application.DTOs.BorrowRecords;

namespace Library.Application.Interfaces;

public interface IBorrowRecordService
{
    Task<List<BorrowRecordResponse>> GetAllAsync(string? status = null);
    Task<BorrowRecordResponse> GetByIdAsync(int id);
    Task<List<BorrowRecordResponse>> GetByUserIdAsync(Guid userId);
    Task<BorrowRecordResponse> CreateAsync(CreateBorrowRequest request);
    Task<BorrowRecordResponse> ReturnBooksAsync(int borrowRecordId, ReturnBooksRequest request);
    Task<BorrowRecordResponse> MarkLostAsync(int borrowRecordId, int borrowDetailId, MarkLostRequest request);
}
