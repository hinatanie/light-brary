using System;
using System.Threading.Tasks;
using light_brary.DTOs.Responses;

namespace light_brary.Services
{
    public class BorrowDetailService
    {
        private readonly BorrowRecordService _borrowRecordService;

        public BorrowDetailService(BorrowRecordService borrowRecordService)
        {
            _borrowRecordService = borrowRecordService;
        }

        public async Task<BorrowDetailActionResponse> ReturnSingleBookAsync(Guid borrowDetailId)
        {
            return await _borrowRecordService.ReturnSingleBookAsync(borrowDetailId);
        }

        public async Task<BorrowDetailActionResponse> MarkLostBookAsync(Guid borrowDetailId)
        {
            return await _borrowRecordService.MarkLostBookAsync(borrowDetailId);
        }
    }
}