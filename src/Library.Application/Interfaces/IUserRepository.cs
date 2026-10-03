using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber, Guid? excludeUserId = null);
    Task<User> AddAsync(User user);
    Task UpdateUserAsync(User user);

    /// <summary>Thay toàn bộ vai trò của user bằng danh sách role đã được xác thực.</summary>
    Task SetUserRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<int> CountUsersInRoleAsync(string roleName);
}
