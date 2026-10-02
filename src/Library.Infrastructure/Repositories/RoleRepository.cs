using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly LibraryDbContext _context;

    public RoleRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _context.Roles.AsNoTracking().ToListAsync();
    }
}
