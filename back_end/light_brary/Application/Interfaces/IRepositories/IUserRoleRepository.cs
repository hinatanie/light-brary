using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId);
        Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId);
        Task<UserRole?> GetByUserAndRoleIdAsync(int userId, int roleId);
        Task<bool> ExistsAsync(int userId, int roleId);
        Task AddAsync(UserRole userRole);
        Task DeleteAsync(UserRole userRole);
    }
}