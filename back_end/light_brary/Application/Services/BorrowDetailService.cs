using System;
using System.Threading.Tasks;
using light_brary.Application.DTOs.BorrowRecords;
using light_brary.Application.Interfaces;

namespace light_brary.Application.Services
{
    public class BorrowDetailService : IBorrowDetailService
    {
        private readonly IBorrowRecordService _borrowRecordService;

        public BorrowDetailService(IBorrowRecordService borrowRecordService)
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