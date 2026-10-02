using Library.Application.DTOs.Users;
using Library.Application.Exceptions;
using Library.Application.Interfaces;

namespace Library.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToResponse).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException($"Không tìm thấy người dùng với mã ID: {id}");

        return MapToResponse(user);
    }

    private static UserResponse MapToResponse(Library.Domain.Entities.User u)
    {
        var roles = u.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>();
        if (roles.Count == 0)
        {
            roles.Add("Member");
        }
        return new UserResponse(
            u.Id,
            u.FullName,
            u.Email,
            u.PhoneNumber,
            u.Address,
            u.CreatedAt,
            roles
        );
    }

    public async Task UpdateUserRolesAsync(string id, UpdateUserRolesRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");
        await _userRepository.UpdateUserRolesAsync(id, request.RoleNames);
    }

    public async Task<List<string>> GetUserRolesAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");
        return await _userRepository.GetUserRolesAsync(id);
    }

    public async Task<UserResponse> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");
        
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.Address = request.Address?.Trim();
        
        await _userRepository.UpdateUserAsync(user);
        return MapToResponse(user);
    }
}
