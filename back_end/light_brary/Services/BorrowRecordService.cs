using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Services.Interfaces;
using light_brary.Repositories.Interfaces;
using light_brary.Entities;
using light_brary.Enums;
using light_brary.Data;

namespace light_brary.Services
{
    public class BorrowRecordService : IBorrowRecordService
    {
        private readonly AppDbContext _context;
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly IUserRepository _userRepository;

        public BorrowRecordService(
            AppDbContext context,
            IBorrowRecordRepository borrowRecordRepository,
            IUserRepository userRepository)
        {
            _context = context;
            _borrowRecordRepository = borrowRecordRepository;
            _userRepository = userRepository;
        }

        public async Task<BorrowRecordResponse> CreateAsync(CreateBorrowRecordRequest request)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(request.UserId);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {request.UserId}");

            if (request.BookCopyIds == null || !request.BookCopyIds.Any())
                throw new BusinessException("Danh sách bản sao sách không được để trống.");

            if (request.BookCopyIds.Distinct().Count() != request.BookCopyIds.Count)
                throw new BusinessException("Danh sách bản sao sách chứa ID bị trùng lặp.");

            if (request.DueDate <= DateTime.UtcNow)
                throw new BusinessException("Hạn trả sách phải sau thời điểm hiện tại.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var copies = await _context.BookCopies
                    .Include(bc => bc.Book)
                    .Where(bc => request.BookCopyIds.Contains(bc.Id))
                    .ToListAsync();

                if (copies.Count != request.BookCopyIds.Count)
                {
                    var foundIds = copies.Select(c => c.Id).ToHashSet();
                    var missingId = request.BookCopyIds.First(id => !foundIds.Contains(id));
                    throw new NotFoundException($"Không tìm thấy bản sao sách với ID: {missingId}");
                }

                foreach (var copy in copies)
                {
                    if (copy.Status != BookCopyStatus.Available)
                    {
                        throw new ConflictException($"Bản sao sách '{copy.Barcode}' không sẵn sàng để mượn (trạng thái hiện tại: {copy.Status}).");
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

                await _context.BorrowRecords.AddAsync(record);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return await GetByIdAsync(record.Id);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<BorrowRecordResponse> BorrowForMemberAsync(Guid userId, MemberBorrowRequest request)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            var dueDate = request.DueDate ?? DateTime.UtcNow.AddDays(14);
            if (dueDate <= DateTime.UtcNow)
                throw new BusinessException("Hạn trả sách phải sau thời điểm hiện tại.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                BookCopy? copy = null;
                if (request.BookCopyId.HasValue && request.BookCopyId.Value != Guid.Empty)
                {
                    copy = await _context.BookCopies
                        .Include(bc => bc.Book)
                        .FirstOrDefaultAsync(bc => bc.Id == request.BookCopyId.Value && bc.BookId == request.BookId);

                    if (copy is null)
                        throw new NotFoundException($"Không tìm thấy bản sao sách với ID: {request.BookCopyId.Value}");

                    if (copy.Status != BookCopyStatus.Available)
                        throw new ConflictException($"Bản sao sách '{copy.Barcode}' không sẵn sàng để mượn (trạng thái: {copy.Status}).");
                }
                else
                {
                    copy = await _context.BookCopies
                        .Include(bc => bc.Book)
                        .Where(bc => bc.BookId == request.BookId && bc.Status == BookCopyStatus.Available)
                        .OrderBy(bc => bc.Id)
                        .FirstOrDefaultAsync();

                    if (copy is null)
                        throw new ConflictException("Đầu sách này hiện không còn bản sao nào có sẵn trên kệ để cho mượn.");
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

                await _context.BorrowRecords.AddAsync(record);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return await GetByIdAsync(record.Id);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<BorrowRecordSummaryResponse>> GetAllAsync(Guid? userId = null, string? status = null, bool? overdue = null)
        {
            var records = await _borrowRecordRepository.GetAllWithDetailsAsync(userId, status, overdue);
            return records.Select(MapToSummaryResponse).ToList();
        }

        public async Task<BorrowRecordResponse> GetByIdAsync(Guid id)
        {
            var record = await _borrowRecordRepository.GetWithDetailsAsync(id);
            if (record is null)
                throw new NotFoundException($"Không tìm thấy phiếu mượn với ID: {id}");

            return MapToResponse(record);
        }

        public async Task<BorrowDetailActionResponse> ReturnSingleBookAsync(Guid borrowDetailId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var detail = await _context.BorrowDetails
                    .Include(d => d.BookCopy)
                    .Include(d => d.BorrowRecord)
                        .ThenInclude(r => r.BorrowDetails)
                    .FirstOrDefaultAsync(d => d.Id == borrowDetailId);

                if (detail is null)
                    throw new NotFoundException($"Không tìm thấy chi tiết mượn với ID: {borrowDetailId}");

                if (detail.Status != BorrowDetailStatus.Borrowing)
                    throw new ConflictException($"Chi tiết mượn không ở trạng thái đang mượn (trạng thái hiện tại: {detail.Status}).");

                detail.Status = BorrowDetailStatus.Returned;
                detail.ReturnDate = DateTime.UtcNow;
                detail.BookCopy.Status = BookCopyStatus.Available;

                RecalculateRecordStatus(detail.BorrowRecord);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return new BorrowDetailActionResponse
                {
                    Id = detail.Id,
                    BorrowRecordId = detail.BorrowRecordId,
                    BookCopyId = detail.BookCopyId,
                    Status = detail.Status.ToString(),
                    ReturnDate = detail.ReturnDate,
                    BookCopyStatus = detail.BookCopy.Status.ToString(),
                    BorrowRecordStatus = detail.BorrowRecord.Status.ToString()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ReturnBooksResponse> ReturnMultipleBooksAsync(ReturnBooksRequest request)
        {
            if (request.BorrowDetailIds == null || !request.BorrowDetailIds.Any())
                throw new BusinessException("Danh sách chi tiết mượn không được để trống.");

            if (request.BorrowDetailIds.Distinct().Count() != request.BorrowDetailIds.Count)
                throw new BusinessException("Danh sách chi tiết mượn chứa ID bị trùng lặp.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var details = await _context.BorrowDetails
                    .Include(d => d.BookCopy)
                    .Include(d => d.BorrowRecord)
                        .ThenInclude(r => r.BorrowDetails)
                    .Where(d => request.BorrowDetailIds.Contains(d.Id))
                    .ToListAsync();

                if (details.Count != request.BorrowDetailIds.Count)
                {
                    var foundIds = details.Select(d => d.Id).ToHashSet();
                    var missingId = request.BorrowDetailIds.First(id => !foundIds.Contains(id));
                    throw new NotFoundException($"Không tìm thấy chi tiết mượn với ID: {missingId}");
                }

                if (details.Any(d => d.Status != BorrowDetailStatus.Borrowing))
                    throw new ConflictException("Một hoặc nhiều cuốn sách không ở trạng thái đang mượn.");

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

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return new ReturnBooksResponse
                {
                    Items = details.Select(d => new BorrowDetailActionResponse
                    {
                        Id = d.Id,
                        BorrowRecordId = d.BorrowRecordId,
                        BookCopyId = d.BookCopyId,
                        Status = d.Status.ToString(),
                        ReturnDate = d.ReturnDate,
                        BookCopyStatus = d.BookCopy.Status.ToString(),
                        BorrowRecordStatus = d.BorrowRecord.Status.ToString()
                    }).ToList()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<BorrowDetailActionResponse> MarkLostBookAsync(Guid borrowDetailId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var detail = await _context.BorrowDetails
                    .Include(d => d.BookCopy)
                    .Include(d => d.BorrowRecord)
                        .ThenInclude(r => r.BorrowDetails)
                    .FirstOrDefaultAsync(d => d.Id == borrowDetailId);

                if (detail is null)
                    throw new NotFoundException($"Không tìm thấy chi tiết mượn với ID: {borrowDetailId}");

                if (detail.Status != BorrowDetailStatus.Borrowing)
                    throw new ConflictException($"Chi tiết mượn không ở trạng thái đang mượn (trạng thái hiện tại: {detail.Status}).");

                detail.Status = BorrowDetailStatus.Lost;
                detail.ReturnDate = null;
                detail.BookCopy.Status = BookCopyStatus.Lost;

                RecalculateRecordStatus(detail.BorrowRecord);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return new BorrowDetailActionResponse
                {
                    Id = detail.Id,
                    BorrowRecordId = detail.BorrowRecordId,
                    BookCopyId = detail.BookCopyId,
                    Status = detail.Status.ToString(),
                    ReturnDate = detail.ReturnDate,
                    BookCopyStatus = detail.BookCopy.Status.ToString(),
                    BorrowRecordStatus = detail.BorrowRecord.Status.ToString()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static void RecalculateRecordStatus(BorrowRecord record)
        {
            if (record.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing))
            {
                record.Status = BorrowRecordStatus.Borrowing;
            }
            else if (record.BorrowDetails.All(d => d.Status == BorrowDetailStatus.Returned))
            {
                record.Status = BorrowRecordStatus.Returned;
            }
            else
            {
                record.Status = BorrowRecordStatus.Lost;
            }
        }

        private static BorrowRecordSummaryResponse MapToSummaryResponse(BorrowRecord record)
        {
            var isOverdue = record.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing) && DateTime.UtcNow > record.DueDate;

            return new BorrowRecordSummaryResponse
            {
                Id = record.Id,
                User = MapUserResponse(record.User),
                BorrowDate = record.BorrowDate,
                DueDate = record.DueDate,
                Status = record.Status.ToString(),
                TotalBooks = record.BorrowDetails.Count,
                BorrowingCount = record.BorrowDetails.Count(d => d.Status == BorrowDetailStatus.Borrowing),
                ReturnedCount = record.BorrowDetails.Count(d => d.Status == BorrowDetailStatus.Returned),
                LostCount = record.BorrowDetails.Count(d => d.Status == BorrowDetailStatus.Lost),
                IsOverdue = isOverdue,
                Details = record.BorrowDetails.Select(d => new BorrowDetailResponse
                {
                    Id = d.Id,
                    BookCopyId = d.BookCopyId,
                    Barcode = d.BookCopy?.Barcode ?? string.Empty,
                    BookId = d.BookCopy?.BookId ?? Guid.Empty,
                    BookTitle = d.BookCopy?.Book?.Title ?? string.Empty,
                    Status = d.Status.ToString(),
                    ReturnDate = d.ReturnDate
                }).ToList()
            };
        }

        private static BorrowRecordResponse MapToResponse(BorrowRecord record)
        {
            var isOverdue = record.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing) && DateTime.UtcNow > record.DueDate;

            return new BorrowRecordResponse
            {
                Id = record.Id,
                User = MapUserResponse(record.User),
                BorrowDate = record.BorrowDate,
                DueDate = record.DueDate,
                Status = record.Status.ToString(),
                Note = record.Note,
                IsOverdue = isOverdue,
                Details = record.BorrowDetails.Select(d => new BorrowDetailResponse
                {
                    Id = d.Id,
                    BookCopyId = d.BookCopyId,
                    Barcode = d.BookCopy?.Barcode ?? string.Empty,
                    BookId = d.BookCopy?.BookId ?? Guid.Empty,
                    BookTitle = d.BookCopy?.Book?.Title ?? string.Empty,
                    Status = d.Status.ToString(),
                    ReturnDate = d.ReturnDate
                }).ToList()
            };
        }

        private static UserResponse MapUserResponse(User? user)
        {
            if (user == null) return new UserResponse();

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Roles = user.UserRoles?.Where(ur => ur.Role != null).Select(ur => ur.Role.Name).ToList() ?? new List<string>(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}