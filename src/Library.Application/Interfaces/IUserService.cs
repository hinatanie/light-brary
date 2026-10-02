using Library.Application.DTOs.Users;

namespace Library.Application.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse> GetByIdAsync(string id);
    Task UpdateUserRolesAsync(string id, UpdateUserRolesRequest request);
    Task<List<string>> GetUserRolesAsync(string id);
    Task<UserResponse> UpdateUserAsync(string id, UpdateUserRequest request);
}
