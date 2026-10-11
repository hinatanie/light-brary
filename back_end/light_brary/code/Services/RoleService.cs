using light_brary.Constants;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Services
{
    public class RoleService
    {
        private static readonly string[] SystemRoles = { RoleNames.ADMIN, RoleNames.LIBRARIAN, RoleNames.USER };
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleService(RoleManager<IdentityRole<Guid>> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IEnumerable<RoleResponse>> GetAllAsync()
        {
            return await _roleManager.Roles.Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name!
            }).ToListAsync();
        }

        public async Task<RoleResponse> GetByIdAsync(Guid id)
        {
            var role = await FindRoleAsync(id);
            return new RoleResponse { Id = role.Id, Name = role.Name! };
        }

        public async Task<RoleResponse> CreateAsync(SaveRoleRequest request)
        {
            var name = request.Name.Trim();
            if (await _roleManager.RoleExistsAsync(name))
                throw new ConflictException($"Vai trò '{name}' đã tồn tại.");

            var role = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = name };
            (await _roleManager.CreateAsync(role)).EnsureSucceeded();
            return new RoleResponse { Id = role.Id, Name = role.Name! };
        }

        public async Task<RoleResponse> UpdateAsync(Guid id, SaveRoleRequest request)
        {
            var role = await FindRoleAsync(id);
            if (SystemRoles.Any(name => name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
                throw new BusinessException("Không được phép chỉnh sửa tên các vai trò mặc định của hệ thống.");

            var name = request.Name.Trim();
            var existing = await _roleManager.FindByNameAsync(name);
            if (existing is not null && existing.Id != id)
                throw new ConflictException($"Vai trò '{name}' đã tồn tại.");

            role.Name = name;
            (await _roleManager.UpdateAsync(role)).EnsureSucceeded();
            return new RoleResponse { Id = role.Id, Name = role.Name! };
        }

        public async Task DeleteAsync(Guid id)
        {
            var role = await FindRoleAsync(id);
            if (SystemRoles.Any(name => name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
                throw new BusinessException("Không được phép xóa các vai trò mặc định của hệ thống.");

            if ((await _userManager.GetUsersInRoleAsync(role.Name!)).Any())
                throw new ConflictException("Vai trò đang được gán cho người dùng, không thể xóa.");

            (await _roleManager.DeleteAsync(role)).EnsureSucceeded();
        }

        public async Task<IEnumerable<RoleResponse>> GetUserRolesAsync(Guid userId)
        {
            var user = await FindUserAsync(userId);
            var names = (await _userManager.GetRolesAsync(user)).ToArray();
            return await _roleManager.Roles.Where(role => names.Contains(role.Name!))
                .Select(role => new RoleResponse { Id = role.Id, Name = role.Name! }).ToListAsync();
        }

        public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            var user = await FindUserAsync(userId);
            var role = await FindRoleAsync(roleId);
            if (await _userManager.IsInRoleAsync(user, role.Name!)) return;

            (await _userManager.AddToRoleAsync(user, role.Name!)).EnsureSucceeded();
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            var user = await FindUserAsync(userId);
            var role = await FindRoleAsync(roleId);
            if (!await _userManager.IsInRoleAsync(user, role.Name!)) return;

            if (role.Name!.Equals(RoleNames.ADMIN, StringComparison.OrdinalIgnoreCase) &&
                (await _userManager.GetUsersInRoleAsync(RoleNames.ADMIN)).Count <= 1)
                throw new BusinessException("Không thể gỡ vai trò Admin khỏi quản trị viên cuối cùng trong hệ thống.");

            (await _userManager.RemoveFromRoleAsync(user, role.Name!)).EnsureSucceeded();
        }

        private async Task<User> FindUserAsync(Guid id)
        {
            return await _userManager.FindByIdAsync(id.ToString())
                ?? throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");
        }

        private async Task<IdentityRole<Guid>> FindRoleAsync(Guid id)
        {
            return await _roleManager.FindByIdAsync(id.ToString())
                ?? throw new NotFoundException($"Không tìm thấy vai trò với ID: {id}");
        }
    }
}
