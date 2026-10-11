using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;

namespace light_brary.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponse>> GetAllAsync();
        Task<RoleResponse> GetByIdAsync(Guid id);
        Task<RoleResponse> CreateAsync(SaveRoleRequest request);
        Task<RoleResponse> UpdateAsync(Guid id, SaveRoleRequest request);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<RoleResponse>> GetUserRolesAsync(Guid userId);
        Task AssignRoleToUserAsync(Guid userId, Guid roleId);
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    }
}