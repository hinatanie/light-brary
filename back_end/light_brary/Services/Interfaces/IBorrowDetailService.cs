using System;
using System.Threading.Tasks;
using light_brary.DTOs.Responses;

namespace light_brary.Services.Interfaces
{
    public interface IBorrowDetailService
    {
        Task<BorrowDetailActionResponse> ReturnSingleBookAsync(Guid borrowDetailId);
        Task<BorrowDetailActionResponse> MarkLostBookAsync(Guid borrowDetailId);
    }
}