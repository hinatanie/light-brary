using light_brary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        UserRepository userRepository,
        UserService userService)
    {
        public async Task<BorrowRecordResponse> CreateAsync(CreateBorrowRecordRequest request)
        {
            var user = await userService.GetRequiredUserAsync(request.UserId);

            if (request.BookCopyIds == null || !request.BookCopyIds.Any())
                throw AppException.BadRequest("Danh sách bản sao sách không được để trống.");

            if (request.BookCopyIds.Distinct().Count() != request.BookCopyIds.Count)
                throw AppException.BadRequest("Danh sách bản sao sách chứa ID bị trùng lặp.");

            if (request.DueDate <= DateTime.UtcNow)
                throw AppException.BadRequest("Hạn trả sách phải sau thời điểm hiện tại.");

            await using var tx = await context.Database.BeginTransactionAsync();
            var copies = await context.BookCopies
                .Include(bc => bc.Book)
                .Where(bc => request.BookCopyIds.Contains(bc.Id))
                .ToListAsync();

            if (copies.Count != request.BookCopyIds.Count)
            {
                var foundIds = copies.Select(c => c.Id).ToHashSet();
                var missingId = request.BookCopyIds.First(id => !foundIds.Contains(id));
                throw AppException.NotFound($"Không tìm thấy bản sao sách với ID: {missingId}");
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

            await context.BorrowRecords.AddAsync(record);
            await context.SaveChangesAsync();
            await tx.CommitAsync();

            return await GetByIdAsync(record.Id);
        }

        public async Task<BorrowRecordResponse> BorrowForMemberAsync(Guid userId, MemberBorrowRequest request)
        {
            var user = await userService.GetRequiredUserAsync(userId);

            var dueDate = request.DueDate ?? DateTime.UtcNow.AddDays(14);
            if (dueDate <= DateTime.UtcNow)
                throw AppException.BadRequest("Hạn trả sách phải sau thời điểm hiện tại.");

            await using var tx = await context.Database.BeginTransactionAsync();
            BookCopy? copy = null;
            if (request.BookCopyId.HasValue && request.BookCopyId.Value != Guid.Empty)
            {
                copy = await context.BookCopies
                    .Include(bc => bc.Book)
                    .FirstOrDefaultAsync(bc => bc.Id == request.BookCopyId.Value && bc.BookId == request.BookId);

                if (copy is null)
                    throw AppException.NotFound($"Không tìm thấy bản sao sách với ID: {request.BookCopyId.Value}");

                if (copy.Status != BookCopyStatus.Available)
                    throw AppException.Conflict($"Bản sao sách '{copy.Barcode}' không sẵn sàng để mượn (trạng thái: {copy.Status}).");
            }
            else
            {
                copy = await context.BookCopies
                    .Include(bc => bc.Book)
                    .Where(bc => bc.BookId == request.BookId && bc.Status == BookCopyStatus.Available)
                    .OrderBy(bc => bc.Id)
                    .FirstOrDefaultAsync();

                if (copy is null)
                    throw AppException.Conflict("Đầu sách này hiện không còn bản sao nào có sẵn trên kệ để cho mượn.");
            }

            copy.Status = BookCopyStatus.Borrowed;

            var record = new BorrowRecord
            {
                UserId = userId,
                BorrowDate = DateTime.UtcNow,
                DueDate = dueDate,
                Status = BorrowRecordStatus.Borrowing,
                Note = string.IsNullOrWhiteSpace(request.Note) ? "Độc giả tự đăng ký mượn qua hệ thống" : request.Note.Trim(),
                BorrowDetails = new List<BorrowDetail>
                {
                    new BorrowDetail
                    {
                        BookCopyId = copy.Id,
                        Status = BorrowDetailStatus.Borrowing,
                        ReturnDate = null
                    }
                }
            };

            await context.BorrowRecords.AddAsync(record);
            await context.SaveChangesAsync();
            await tx.CommitAsync();

            return await GetByIdAsync(record.Id);
        }

        public async Task<IEnumerable<BorrowRecordSummaryResponse>> GetAllAsync(Guid? userId = null, string? status = null, bool? overdue = null)
        {
            var records = (await borrowRecordRepository.GetAllWithDetailsAsync(userId, status, overdue)).ToList();
            var roles = await userRepository.GetRoleNamesByUserIdsAsync(records.Select(record => record.UserId));
            return records.Select(record => BorrowRecordSummaryResponse.FromEntity(record,
                UserResponse.FromEntity(record.User, roles.GetValueOrDefault(record.UserId, new List<string>())))).ToList();
        }

        public async Task<BorrowRecordResponse> GetByIdAsync(Guid id)
        {
            var record = await borrowRecordRepository.GetWithDetailsAsync(id);
            if (record is null)
                throw AppException.NotFound($"Không tìm thấy phiếu mượn với ID: {id}");

            var roles = await userRepository.GetRoleNamesByUserIdsAsync(new[] { record.UserId });
            return BorrowRecordResponse.FromEntity(record,
                UserResponse.FromEntity(record.User, roles.GetValueOrDefault(record.UserId, new List<string>())));
        }

        public async Task<ReturnBooksResponse> ReturnMultipleBooksAsync(ReturnBooksRequest request)
        {
            if (request.BorrowDetailIds == null || !request.BorrowDetailIds.Any())
                throw AppException.BadRequest("Danh sách chi tiết mượn không được để trống.");

            if (request.BorrowDetailIds.Distinct().Count() != request.BorrowDetailIds.Count)
                throw AppException.BadRequest("Danh sách chi tiết mượn chứa ID bị trùng lặp.");

            await using var tx = await context.Database.BeginTransactionAsync();
            var details = await context.BorrowDetails
                .Include(d => d.BookCopy)
                .Include(d => d.BorrowRecord)
                    .ThenInclude(r => r.BorrowDetails)
                .Where(d => request.BorrowDetailIds.Contains(d.Id))
                .ToListAsync();

            if (details.Count != request.BorrowDetailIds.Count)
            {
                var foundIds = details.Select(d => d.Id).ToHashSet();
                var missingId = request.BorrowDetailIds.First(id => !foundIds.Contains(id));
                throw AppException.NotFound($"Không tìm thấy chi tiết mượn với ID: {missingId}");
            }

            if (details.Any(d => d.Status != BorrowDetailStatus.Borrowing))
                throw AppException.Conflict("Một hoặc nhiều cuốn sách không ở trạng thái đang mượn.");

            var now = DateTime.UtcNow;
            var affectedRecords = new HashSet<BorrowRecord>();

            foreach (var detail in details)
            {
                detail.Status = BorrowDetailStatus.Returned;
                detail.ReturnDate = now;
                detail.BookCopy.Status = BookCopyStatus.Available;
                affectedRecords.Add(detail.BorrowRecord);
            }

            foreach (var record in affectedRecords)
            {
                RecalculateRecordStatus(record);
            }

            await context.SaveChangesAsync();
            await tx.CommitAsync();

            return new ReturnBooksResponse
            {
                Items = details.Select(BorrowDetailActionResponse.FromEntity).ToList()
            };
        }

        public async Task<BorrowDetailActionResponse> MarkLostBookAsync(Guid borrowDetailId)
        {
            await using var tx = await context.Database.BeginTransactionAsync();
            var detail = await context.BorrowDetails
                .Include(d => d.BookCopy)
                .Include(d => d.BorrowRecord)
                    .ThenInclude(r => r.BorrowDetails)
                .FirstOrDefaultAsync(d => d.Id == borrowDetailId);

            if (detail is null)
                throw AppException.NotFound($"Không tìm thấy chi tiết mượn với ID: {borrowDetailId}");

            if (detail.Status != BorrowDetailStatus.Borrowing)
                throw AppException.Conflict($"Chi tiết mượn không ở trạng thái đang mượn (trạng thái hiện tại: {detail.Status}).");

            detail.Status = BorrowDetailStatus.Lost;
            detail.ReturnDate = null;
            detail.BookCopy.Status = BookCopyStatus.Lost;

            RecalculateRecordStatus(detail.BorrowRecord);

            await context.SaveChangesAsync();
            await tx.CommitAsync();

            return BorrowDetailActionResponse.FromEntity(detail);
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
