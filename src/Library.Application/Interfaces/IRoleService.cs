using Library.Application.DTOs.Roles;

namespace Library.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync();
}
