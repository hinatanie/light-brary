using Library.Domain.Entities;
namespace Library.Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
    Task<List<Role>> GetAllAsync();
}
