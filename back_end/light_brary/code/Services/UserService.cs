using light_brary.Services.Authentication;
using light_brary.Repositories;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
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
            return users.Select(user => UserResponse.FromEntity(user, roles.GetValueOrDefault(user.Id, new List<string>()))).ToList();
        }

        public async Task<UserResponse> GetByIdAsync(Guid id)
        {
            var user = await FindUserAsync(id);
            return UserResponse.FromEntity(user, await userManager.GetRolesAsync(user));
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
            return UserResponse.FromEntity(user, await userManager.GetRolesAsync(user));
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
            return UserResponse.FromEntity(user, await userManager.GetRolesAsync(user));
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
            var userResponse = UserResponse.FromEntity(user, await userManager.GetRolesAsync(user));
            return records.Select(record => BorrowRecordResponse.FromEntity(record, userResponse)).ToList();
        }

        public async Task AdminUpdatePasswordAsync(Guid id, AdminUpdatePasswordRequest request)
        {
            var user = await FindUserAsync(id);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            (await userManager.ResetPasswordAsync(user, token, request.NewPassword)).EnsureSucceeded();
        }

        private async Task<User> FindUserAsync(Guid id)
        {
            return await userManager.FindByIdAsync(id.ToString())
                ?? throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");
        }

    }
}
