using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.BorrowRecords;
using light_brary.Application.DTOs.Users;

namespace light_brary.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllAsync(string? keyword = null, string? role = null);
        Task<UserResponse> GetByIdAsync(Guid id);
        Task<UserResponse> CreateAsync(CreateUserRequest request);
        Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request);
        Task DeleteAsync(Guid id);
        Task<UserResponse> GetProfileAsync(Guid userId);
        Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request);
        Task<IEnumerable<BorrowRecordResponse>> GetUserBorrowRecordsAsync(Guid userId);
        Task<IEnumerable<BorrowedBookResponse>> GetUserBorrowedBooksAsync(Guid userId);
        Task AdminUpdatePasswordAsync(Guid id, AdminUpdatePasswordRequest request);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }
}