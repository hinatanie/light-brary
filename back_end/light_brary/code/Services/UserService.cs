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
            var user = await GetRequiredUserAsync(id);

            return UserResponse.FromEntity(
                user,
                await userManager.GetRolesAsync(user)
            );
        }

        public async Task<UserResponse> CreateAsync(
            string userName,
            string fullName,
            string phoneNumber,
            string address,
            string password)
        {
            var existingUser = await userManager.FindByNameAsync(userName);
            if (existingUser is not null)
                throw AppException.Conflict($"Tên đăng nhập '{userName}' đã tồn tại.");

            if (await userRepository.ExistsByPhoneNumberAsync(phoneNumber.Trim()))
                throw AppException.Conflict("Số điện thoại này đã được sử dụng.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                PhoneNumber = phoneNumber.Trim(),
                FullName = fullName.Trim(),
                Address = address.Trim()
            };

            using var transaction = await context.Database.BeginTransactionAsync();

            (await userManager.CreateAsync(user, password)).ThrowIfFailed();
            (await userManager.AddToRoleAsync(user, RoleNames.USER)).ThrowIfFailed();

            await transaction.CommitAsync();

            return UserResponse.FromEntity(
                user,
                await userManager.GetRolesAsync(user)
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await GetRequiredUserAsync(id);

            (await userManager.DeleteAsync(user)).ThrowIfFailed();
        }

        public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await GetRequiredUserAsync(userId);
            user.Address = request.Address.Trim();
            (await userManager.UpdateAsync(user)).ThrowIfFailed();
            return UserResponse.FromEntity(
                user,
                await userManager.GetRolesAsync(user)
            );
        }

        public async Task<IEnumerable<BorrowRecordResponse>> GetUserBorrowRecordsAsync(Guid userId)
        {
            var user = await GetRequiredUserAsync(userId);
            var records = await borrowRecordRepository.GetByUserIdAsync(userId);
            var userResponse = UserResponse.FromEntity(user, await userManager.GetRolesAsync(user));
            return records.Select(record => BorrowRecordResponse.FromEntity(record, userResponse)).ToList();
        }

        public async Task<User> GetRequiredUserAsync(Guid id)
        {
            return await userManager.FindByIdAsync(id.ToString())
                ?? throw AppException.NotFound($"Không tìm thấy người dùng với ID: {id}");
        }

    }
}
