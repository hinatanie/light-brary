using Library.Application.DTOs.BorrowRecords;

namespace Library.Application.Interfaces;

public interface IBorrowRecordService
{
    Task<List<BorrowRecordResponse>> GetAllAsync();
    Task<BorrowRecordResponse> GetByIdAsync(int id);
    Task<List<BorrowRecordResponse>> GetByUserIdAsync(string userId);
    Task<BorrowRecordResponse> CreateAsync(CreateBorrowRequest request);
    Task<BorrowRecordResponse> ReturnBooksAsync(int borrowRecordId, ReturnBooksRequest request);
}
