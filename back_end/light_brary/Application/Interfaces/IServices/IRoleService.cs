using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.Roles;

namespace light_brary.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponse>> GetAllAsync();
        Task<RoleResponse> GetByIdAsync(int id);
        Task<RoleResponse> CreateAsync(SaveRoleRequest request);
        Task<RoleResponse> UpdateAsync(int id, SaveRoleRequest request);
        Task DeleteAsync(int id);
        Task<IEnumerable<RoleResponse>> GetUserRolesAsync(int userId);
        Task AssignRoleToUserAsync(int userId, int roleId);
        Task RemoveRoleFromUserAsync(int userId, int roleId);
    }
}