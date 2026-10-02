using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByEmailOrPhoneAsync(string identifier);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber);
    Task<User> AddAsync(User user);
}
