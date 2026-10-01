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

    private static UserResponse MapToResponse(Library.Domain.Entities.User u) => new(
        u.Id,
        u.FullName,
        u.Email,
        u.PhoneNumber,
        u.Address,
        u.CreatedAt
    );
}
