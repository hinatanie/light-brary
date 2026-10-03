using Library.Application.DTOs.Users;

namespace Library.Application.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse> GetByIdAsync(Guid id);
    Task UpdateUserRolesAsync(Guid id, UpdateUserRolesRequest request, Guid actingUserId);
    Task<List<string>> GetUserRolesAsync(Guid id);
    Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request);
}
