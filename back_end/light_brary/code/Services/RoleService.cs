using light_brary.Constants;
using light_brary.Data;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;
using light_brary.Repositories;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Services
{
    public class RoleService(
        RoleRepository roleRepository,
        UserManager<User> userManager,
        UserService userService,
        AppDbContext context)
    {
        private static readonly string[] SystemRoles = { RoleNames.ADMIN, RoleNames.LIBRARIAN, RoleNames.USER };

        public async Task<IEnumerable<RoleResponse>> GetAllAsync()
        {
            var roles = await roleRepository.GetByNamesAsync(SystemRoles);
            return roles.Select(RoleResponse.FromEntity).ToList();
        }

        public async Task<IEnumerable<RoleResponse>> GetUserRolesAsync(Guid userId)
        {
            var user = await userService.GetRequiredUserAsync(userId);
            var names = await userManager.GetRolesAsync(user);
            var roles = await roleRepository.GetByNamesAsync(names.Intersect(SystemRoles));
            return roles.Select(RoleResponse.FromEntity).ToList();
        }

        public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            var role = await FindRoleAsync(roleId);
            var user = await userService.GetRequiredUserAsync(userId);
            if (await userManager.IsInRoleAsync(user, role.Name!)) return;

            (await userManager.AddToRoleAsync(user, role.Name!)).ThrowIfFailed();
            (await userManager.UpdateSecurityStampAsync(user)).ThrowIfFailed();
            await transaction.CommitAsync();
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            var role = await FindRoleAsync(roleId);
            var user = await userService.GetRequiredUserAsync(userId);
            if (!await userManager.IsInRoleAsync(user, role.Name!)) return;

            (await userManager.RemoveFromRoleAsync(user, role.Name!)).ThrowIfFailed();
            (await userManager.UpdateSecurityStampAsync(user)).ThrowIfFailed();
            await transaction.CommitAsync();
        }

        private async Task<IdentityRole<Guid>> FindRoleAsync(Guid id)
        {
            var role = await roleRepository.GetByIdForUpdateAsync(id)
                ?? throw AppException.NotFound($"Không tìm thấy vai trò với ID: {id}");

            if (!SystemRoles.Contains(role.Name))
                throw AppException.BadRequest("Chỉ được phép gán hoặc gỡ các vai trò Admin, Librarian và User.");

            return role;
        }
    }
}
