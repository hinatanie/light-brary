using System.Threading.Tasks;
using light_brary.Application.DTOs.BorrowRecords;

namespace light_brary.Application.Interfaces
{
    public interface IBorrowDetailService
    {
        Task<BorrowDetailActionResponse> ReturnSingleBookAsync(int borrowDetailId);
        Task<BorrowDetailActionResponse> MarkLostBookAsync(int borrowDetailId);
    }
}