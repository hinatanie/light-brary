using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.BorrowRecords;

namespace light_brary.Application.Interfaces
{
    public interface IBorrowRecordService
    {
        Task<BorrowRecordResponse> CreateAsync(CreateBorrowRecordRequest request);
        Task<BorrowRecordResponse> BorrowForMemberAsync(int userId, MemberBorrowRequest request);
        Task<IEnumerable<BorrowRecordSummaryResponse>> GetAllAsync(int? userId = null, string? status = null, bool? overdue = null);
        Task<BorrowRecordResponse> GetByIdAsync(int id);
        Task<BorrowDetailActionResponse> ReturnSingleBookAsync(int borrowDetailId);
        Task<ReturnBooksResponse> ReturnMultipleBooksAsync(ReturnBooksRequest request);
        Task<BorrowDetailActionResponse> MarkLostBookAsync(int borrowDetailId);
    }
}