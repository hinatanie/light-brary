using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.Interfaces
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