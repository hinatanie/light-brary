using System;
using System.Threading.Tasks;
using light_brary.Application.DTOs.BorrowRecords;

namespace light_brary.Application.Interfaces
{
    public interface IBorrowDetailService
    {
        Task<BorrowDetailActionResponse> ReturnSingleBookAsync(Guid borrowDetailId);
        Task<BorrowDetailActionResponse> MarkLostBookAsync(Guid borrowDetailId);
    }
}