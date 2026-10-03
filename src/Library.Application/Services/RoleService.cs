using Library.Application.DTOs.Roles;
using Library.Application.Interfaces;

namespace Library.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<RoleResponse>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        return roles.Select(r => new RoleResponse(r.Id, r.Name)).ToList();
    }
}
