using light_brary.Services.Authentication;
using light_brary.Repositories;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Enums;
using light_brary.Constants;
using light_brary.Data;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Services
{
    public class UserService(
        UserRepository userRepository,
        BorrowRecordRepository borrowRecordRepository,
        UserManager<User> userManager,
        AppDbContext context)
    {
        public async Task<IEnumerable<UserResponse>> GetAllAsync(string? keyword = null, string? role = null)
        {
            var users = (await userRepository.GetAllFilteredAsync(keyword, role)).ToList();
            var roles = await userRepository.GetRoleNamesByUserIdsAsync(users.Select(user => user.Id));
            return users.Select(user => MapToResponse(user, roles.GetValueOrDefault(user.Id, new List<string>()))).ToList();
        }

        public async Task<UserResponse> GetByIdAsync(Guid id)
        {
            var user = await FindUserAsync(id);
            return MapToResponse(user, await userManager.GetRolesAsync(user));
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            var phone = request.PhoneNumber.Trim();
            if (await userRepository.ExistsByPhoneNumberAsync(phone))
                throw new ConflictException("Số điện thoại này đã được sử dụng.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = phone,
                PhoneNumber = phone,
                FullName = request.FullName.Trim(),
                Address = request.Address.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            using var transaction = await context.Database.BeginTransactionAsync();
            (await userManager.CreateAsync(user, request.Password)).EnsureSucceeded();
            (await userManager.AddToRoleAsync(user, RoleNames.USER)).EnsureSucceeded();
            await transaction.CommitAsync();
            return MapToResponse(user, await userManager.GetRolesAsync(user));
        }

        public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await FindUserAsync(id);
            var phone = request.PhoneNumber.Trim();
            var existing = await userRepository.GetByPhoneNumberAsync(phone);
            if (existing is not null && existing.Id != id)
                throw new ConflictException("Số điện thoại này đã được sử dụng.");

            if (user.PhoneNumber != phone) user.PhoneNumberConfirmed = false;
            user.FullName = request.FullName.Trim();
            user.PhoneNumber = phone;
            user.UserName = phone;
            user.Address = request.Address.Trim();
            (await userManager.UpdateAsync(user)).EnsureSucceeded();
            return MapToResponse(user, await userManager.GetRolesAsync(user));
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await FindUserAsync(id);
            if (await userRepository.HasBorrowHistoryAsync(id))
                throw new ConflictException("Người dùng đã có lịch sử mượn sách, không thể xóa.");

            if (await userManager.IsInRoleAsync(user, RoleNames.ADMIN) &&
                (await userManager.GetUsersInRoleAsync(RoleNames.ADMIN)).Count <= 1)
                throw new BusinessException("Không thể xóa tài khoản Admin cuối cùng trong hệ thống.");

            (await userManager.DeleteAsync(user)).EnsureSucceeded();
        }

        public Task<UserResponse> GetProfileAsync(Guid userId) => GetByIdAsync(userId);

        public Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request)
            => UpdateAsync(userId, request);

        public async Task<IEnumerable<BorrowRecordResponse>> GetUserBorrowRecordsAsync(Guid userId)
        {
            var user = await FindUserAsync(userId);
            var records = await borrowRecordRepository.GetByUserIdAsync(userId);
            var userResponse = MapToResponse(user, await userManager.GetRolesAsync(user));
            return records.Select(record => MapToBorrowRecordResponse(record, userResponse)).ToList();
        }

        public async Task AdminUpdatePasswordAsync(Guid id, AdminUpdatePasswordRequest request)
        {
            var user = await FindUserAsync(id);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            (await userManager.ResetPasswordAsync(user, token, request.NewPassword)).EnsureSucceeded();
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await FindUserAsync(userId);
            if (request.NewPassword == request.CurrentPassword)
                throw new BusinessException("Mật khẩu mới không được trùng với mật khẩu hiện tại.");

            (await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword)).EnsureSucceeded();
        }

        private async Task<User> FindUserAsync(Guid id)
        {
            return await userManager.FindByIdAsync(id.ToString())
                ?? throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");
        }

        private static UserResponse MapToResponse(User user, IEnumerable<string> roles)
        {
            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber!,
                Address = user.Address,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt
            };
        }

        private static BorrowRecordResponse MapToBorrowRecordResponse(BorrowRecord record, UserResponse user)
        {
            var isOverdue = record.BorrowDetails.Any(d => d.Status == BorrowDetailStatus.Borrowing) && DateTime.UtcNow > record.DueDate;

            return new BorrowRecordResponse
            {
                Id = record.Id,
                User = user,
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
    }
}