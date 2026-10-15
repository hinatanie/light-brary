using light_brary.Constants;
using light_brary.Data;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Services
{
    public class RoleService(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<User> userManager,
        UserService userService,
        AppDbContext context)
    {
        private static readonly string[] SystemRoles = { RoleNames.ADMIN, RoleNames.LIBRARIAN, RoleNames.USER };

        public async Task<IEnumerable<RoleResponse>> GetAllAsync()
        {
            return await roleManager.Roles.Select(role => new RoleResponse
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
            if (await roleManager.RoleExistsAsync(name))
                throw AppException.Conflict($"Vai trò '{name}' đã tồn tại.");

            var role = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = name };
            (await roleManager.CreateAsync(role)).ThrowIfFailed();
            return new RoleResponse { Id = role.Id, Name = role.Name! };
        }

        public async Task<RoleResponse> UpdateAsync(Guid id, SaveRoleRequest request)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            var role = await FindRoleAsync(id, forUpdate: true);
            if (SystemRoles.Any(name => name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
                throw AppException.BadRequest("Không được phép chỉnh sửa tên các vai trò mặc định của hệ thống.");

            var name = request.Name.Trim();
            var existing = await roleManager.FindByNameAsync(name);
            if (existing is not null && existing.Id != id)
                throw AppException.Conflict($"Vai trò '{name}' đã tồn tại.");

            if (role.Name == name)
                return new RoleResponse { Id = role.Id, Name = role.Name! };

            var users = await userManager.GetUsersInRoleAsync(role.Name!);
            role.Name = name;
            (await roleManager.UpdateAsync(role)).ThrowIfFailed();
            foreach (var user in users)
                (await userManager.UpdateSecurityStampAsync(user)).ThrowIfFailed();
            await transaction.CommitAsync();
            return new RoleResponse { Id = role.Id, Name = role.Name! };
        }

        public async Task DeleteAsync(Guid id)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            var role = await FindRoleAsync(id, forUpdate: true);
            if (SystemRoles.Any(name => name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
                throw AppException.BadRequest("Không được phép xóa các vai trò mặc định của hệ thống.");

            if ((await userManager.GetUsersInRoleAsync(role.Name!)).Any())
                throw AppException.Conflict("Vai trò đang được gán cho người dùng, không thể xóa.");

            (await roleManager.DeleteAsync(role)).ThrowIfFailed();
            await transaction.CommitAsync();
        }

        public async Task<IEnumerable<RoleResponse>> GetUserRolesAsync(Guid userId)
        {
            var user = await userService.GetRequiredUserAsync(userId);
            var names = (await userManager.GetRolesAsync(user)).ToArray();
            return await roleManager.Roles.Where(role => names.Contains(role.Name!))
                .Select(role => new RoleResponse { Id = role.Id, Name = role.Name! }).ToListAsync();
        }

        public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            var role = await FindRoleAsync(roleId, forUpdate: true);
            var user = await userService.GetRequiredUserAsync(userId);
            if (await userManager.IsInRoleAsync(user, role.Name!)) return;

            (await userManager.AddToRoleAsync(user, role.Name!)).ThrowIfFailed();
            (await userManager.UpdateSecurityStampAsync(user)).ThrowIfFailed();
            await transaction.CommitAsync();
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            var role = await FindRoleAsync(roleId, forUpdate: true);
            var user = await userService.GetRequiredUserAsync(userId);
            if (!await userManager.IsInRoleAsync(user, role.Name!)) return;

            if (role.Name!.Equals(RoleNames.ADMIN, StringComparison.OrdinalIgnoreCase) &&
                (await userManager.GetUsersInRoleAsync(RoleNames.ADMIN)).Count <= 1)
                throw AppException.BadRequest("Không thể gỡ vai trò Admin khỏi quản trị viên cuối cùng trong hệ thống.");

            (await userManager.RemoveFromRoleAsync(user, role.Name!)).ThrowIfFailed();
            (await userManager.UpdateSecurityStampAsync(user)).ThrowIfFailed();
            await transaction.CommitAsync();
        }

        private async Task<IdentityRole<Guid>> FindRoleAsync(Guid id, bool forUpdate = false)
        {
            // Serialize membership changes with renames/deletion before reading the role or its users.
            var roles = forUpdate
                ? context.Roles.FromSqlInterpolated($"SELECT * FROM \"AspNetRoles\" WHERE \"Id\" = {id} FOR UPDATE")
                : roleManager.Roles;
            return await roles.SingleOrDefaultAsync(role => role.Id == id)
                ?? throw AppException.NotFound($"Không tìm thấy vai trò với ID: {id}");
        }
    }
}
