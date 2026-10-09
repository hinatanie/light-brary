using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.BorrowRecords;

namespace light_brary.Application.Interfaces
{
    public interface IBorrowRecordService
    {
        Task<BorrowRecordResponse> CreateAsync(CreateBorrowRecordRequest request);
        Task<BorrowRecordResponse> BorrowForMemberAsync(Guid userId, MemberBorrowRequest request);
        Task<IEnumerable<BorrowRecordSummaryResponse>> GetAllAsync(Guid? userId = null, string? status = null, bool? overdue = null);
        Task<BorrowRecordResponse> GetByIdAsync(Guid id);
        Task<BorrowDetailActionResponse> ReturnSingleBookAsync(Guid borrowDetailId);
        Task<ReturnBooksResponse> ReturnMultipleBooksAsync(ReturnBooksRequest request);
        Task<BorrowDetailActionResponse> MarkLostBookAsync(Guid borrowDetailId);
    }
}