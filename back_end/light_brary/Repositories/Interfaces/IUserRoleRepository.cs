using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Entities;

namespace light_brary.Repositories.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId);
        Task<UserRole?> GetByUserAndRoleIdAsync(Guid userId, Guid roleId);
        Task<bool> ExistsAsync(Guid userId, Guid roleId);
        Task AddAsync(UserRole userRole);
        Task DeleteAsync(UserRole userRole);
    }
}