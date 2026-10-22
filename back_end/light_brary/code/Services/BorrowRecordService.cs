using light_brary.Repositories;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Enums;
using light_brary.Data;

namespace light_brary.Services
{
    public class BorrowRecordService(
        AppDbContext context,
        BorrowRecordRepository borrowRecordRepository,
        BorrowDetailRepository borrowDetailRepository,
        UserService userService,
        BookCopyRepository bookCopyRepository)
    {
        public async Task<BorrowRecordResponse> CreateAsync(CreateBorrowRecordRequest request)
        {
            await userService.GetRequiredUserAsync(request.UserId);

            var bookCopyIds = request.BookCopyIds.Distinct().ToList();

            if (request.DueDate <= DateTime.UtcNow)
                throw AppException.BadRequest("Hạn trả sách phải sau thời điểm hiện tại.");

            var copies = await bookCopyRepository.GetBookCopysByListIdAsync(bookCopyIds);

            if (copies.Count() != bookCopyIds.Count)
            {
                throw AppException.NotFound("Không tìm thấy một hoặc nhiều bản sao sách được yêu cầu.");
            }

            foreach (var copy in copies)
            {
                if (copy.Status != BookCopyStatus.Available)
                {
                    throw AppException.Conflict($"Bản sao sách '{copy.Barcode}' không sẵn sàng để mượn (trạng thái hiện tại: {copy.Status}).");
                }

                copy.Status = BookCopyStatus.Borrowed;
            }

            var record = new BorrowRecord
            {
                UserId = request.UserId,
                BorrowDate = DateTime.UtcNow,
                DueDate = request.DueDate,
                Status = BorrowRecordStatus.Borrowing,
                Note = request.Note?.Trim(),
                BorrowDetails = copies.Select(c => new BorrowDetail
                {
                    BookCopyId = c.Id,
                    Status = BorrowDetailStatus.Borrowing,
                    ReturnDate = null
                }).ToList()
            };

            await borrowRecordRepository.AddAsync(record);

            return await GetByIdAsync(record.Id);
        }

        public async Task<IEnumerable<BorrowRecordSummaryResponse>> GetAllAsync(
            Guid? userId = null,
            string? status = null,
            bool? overdue = null)
        {
            var records = await borrowRecordRepository.GetAllWithDetailsAsync(userId, status, overdue);

            return records.Select(record => BorrowRecordSummaryResponse.FromEntity(
                record,
                UserResponse.FromEntity(record.User, Array.Empty<string>()))).ToList();
        }

        public async Task<BorrowRecordResponse> GetByIdAsync(Guid id)
        {
            var record = await borrowRecordRepository.GetWithDetailsAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy phiếu mượn với ID: {id}");

            return BorrowRecordResponse.FromEntity(
                record,
                UserResponse.FromEntity(record.User, Array.Empty<string>()));
        }

        public async Task<ReturnBooksResponse> ReturnMultipleBooksAsync(Guid borrowRecordId, ReturnBooksRequest request)
        {
            var record = await borrowRecordRepository.GetWithDetailsAsync(borrowRecordId)
                ?? throw AppException.NotFound($"Không tìm thấy phiếu mượn với ID: {borrowRecordId}");

            var detailIds = request.BorrowDetailIds.Distinct().ToList();

            var details = record.BorrowDetails.Where(d => detailIds.Contains(d.Id)).ToList();

            EnsureBorrowing(details);

            var now = DateTime.UtcNow;

            foreach (var detail in details)
            {
                detail.Status = BorrowDetailStatus.Returned;
                detail.ReturnDate = now;
                detail.BookCopy.Status = BookCopyStatus.Available;
            }

            RecalculateRecordStatus(record);

            await context.SaveChangesAsync();

            return new ReturnBooksResponse
            {
                Items = details.Select(BorrowDetailActionResponse.FromEntity).ToList()
            };
        }

        public async Task<BorrowDetailActionResponse> MarkLostBookAsync(Guid borrowDetailId)
        {
            var detail = await borrowDetailRepository.GetWithDetailsAsync(borrowDetailId)
                ?? throw AppException.NotFound($"Không tìm thấy chi tiết mượn với ID: {borrowDetailId}");

            EnsureBorrowing([detail]);

            detail.Status = BorrowDetailStatus.Lost;
            detail.ReturnDate = null;
            detail.BookCopy.Status = BookCopyStatus.Lost;

            RecalculateRecordStatus(detail.BorrowRecord);

            await context.SaveChangesAsync();

            return BorrowDetailActionResponse.FromEntity(detail);
        }

        private static void EnsureBorrowing(List<BorrowDetail> details)
        {
            if (details.Any(d => d.Status != BorrowDetailStatus.Borrowing))
                throw AppException.Conflict(
                    "Một hoặc nhiều chi tiết mượn không ở trạng thái đang mượn.");
        }

        private static void RecalculateRecordStatus(BorrowRecord record)
        {
            if (record.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing))
            {
                record.Status = BorrowRecordStatus.Borrowing;
            }
            else
            {
                record.Status = BorrowRecordStatus.Closed;
            }
        }
    }
}
