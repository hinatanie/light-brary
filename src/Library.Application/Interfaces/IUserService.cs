using Library.Application.DTOs.Users;

namespace Library.Application.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse> GetByIdAsync(string id);
}
