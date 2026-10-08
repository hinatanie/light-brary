using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.BorrowRecords;
using light_brary.Application.DTOs.Users;

namespace light_brary.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllAsync(string? keyword = null, string? role = null);
        Task<UserResponse> GetByIdAsync(int id);
        Task<UserResponse> CreateAsync(CreateUserRequest request);
        Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request);
        Task DeleteAsync(int id);
        Task<UserResponse> GetProfileAsync(int userId);
        Task<UserResponse> UpdateProfileAsync(int userId, UpdateUserRequest request);
        Task<IEnumerable<BorrowRecordResponse>> GetUserBorrowRecordsAsync(int userId);
        Task<IEnumerable<BorrowedBookResponse>> GetUserBorrowedBooksAsync(int userId);
    }
}