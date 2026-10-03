using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LibraryDbContext _context;

    public UserRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber, Guid? excludeUserId = null)
    {
        return await _context.Users
            .AnyAsync(u => u.PhoneNumber == phoneNumber && (excludeUserId == null || u.Id != excludeUserId));
    }

    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task SetUserRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds)
    {
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        _context.UserRoles.RemoveRange(existingRoles);

        foreach (var roleId in roleIds)
        {
            await _context.UserRoles.AddAsync(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role!.Name)
            .ToListAsync();
    }

    public async Task<int> CountUsersInRoleAsync(string roleName)
    {
        return await _context.UserRoles
            .CountAsync(ur => ur.Role != null && ur.Role.Name == roleName);
    }
}
