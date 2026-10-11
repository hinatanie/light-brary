using light_brary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Exceptions;
using light_brary.Entities;

namespace light_brary.Services
{
    public class RoleService
    {
        private static readonly string[] SystemRoles = { Role.ADMIN, Role.LIBRARIAN, Role.USER };

        private readonly RoleRepository _roleRepository;
        private readonly UserRoleRepository _userRoleRepository;
        private readonly UserRepository _userRepository;

        public RoleService(
            RoleRepository roleRepository,
            UserRoleRepository userRoleRepository,
            UserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<RoleResponse>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleResponse { Id = r.Id, Name = r.Name }).ToList();
        }

        public async Task<RoleResponse> GetByIdAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role is null)
                throw new NotFoundException($"Không tìm thấy vai trò với ID: {id}");

            return new RoleResponse { Id = role.Id, Name = role.Name };
        }

        public async Task<RoleResponse> CreateAsync(SaveRoleRequest request)
        {
            var name = request.Name.Trim();
            var existing = await _roleRepository.GetByNameAsync(name);
            if (existing is not null)
                throw new ConflictException($"Vai trò '{name}' đã tồn tại.");

            var role = new Role { Name = name };
            await _roleRepository.AddAsync(role);

            return new RoleResponse { Id = role.Id, Name = role.Name };
        }

        public async Task<RoleResponse> UpdateAsync(Guid id, SaveRoleRequest request)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role is null)
                throw new NotFoundException($"Không tìm thấy vai trò với ID: {id}");

            if (SystemRoles.Any(sr => sr.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
                throw new BusinessException("Không được phép chỉnh sửa tên các vai trò mặc định của hệ thống.");

            var name = request.Name.Trim();
            var existing = await _roleRepository.GetByNameAsync(name);
            if (existing is not null && existing.Id != id)
                throw new ConflictException($"Vai trò '{name}' đã tồn tại.");

            role.Name = name;
            await _roleRepository.UpdateAsync(role);

            return new RoleResponse { Id = role.Id, Name = role.Name };
        }

        public async Task DeleteAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role is null)
                throw new NotFoundException($"Không tìm thấy vai trò với ID: {id}");

            if (SystemRoles.Any(sr => sr.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
                throw new BusinessException("Không được phép xóa các vai trò mặc định của hệ thống.");

            var userRoles = await _userRoleRepository.GetByRoleIdAsync(id);
            if (userRoles.Any())
                throw new ConflictException("Vai trò đang được gán cho người dùng, không thể xóa.");

            await _roleRepository.DeleteAsync(role);
        }

        public async Task<IEnumerable<RoleResponse>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
            return userRoles
                .Where(ur => ur.Role != null)
                .Select(ur => new RoleResponse { Id = ur.Role.Id, Name = ur.Role.Name })
                .ToList();
        }

        public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role is null)
                throw new NotFoundException($"Không tìm thấy vai trò với ID: {roleId}");

            var exists = await _userRoleRepository.ExistsAsync(userId, roleId);
            if (exists)
                return; // Idempotent: đã gán rồi thì không cần báo lỗi

            await _userRoleRepository.AddAsync(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role is null)
                throw new NotFoundException($"Không tìm thấy vai trò với ID: {roleId}");

            // Chặn gỡ role Admin nếu là Admin cuối cùng
            if (role.Name.Equals(Role.ADMIN, StringComparison.OrdinalIgnoreCase))
            {
                var adminUsers = await _userRoleRepository.GetByRoleIdAsync(roleId);
                if (adminUsers.Count() <= 1 && adminUsers.Any(ur => ur.UserId == userId))
                {
                    throw new BusinessException("Không thể gỡ vai trò Admin khỏi quản trị viên cuối cùng trong hệ thống.");
                }
            }

            var userRole = await _userRoleRepository.GetByUserAndRoleIdAsync(userId, roleId);
            if (userRole is not null)
            {
                await _userRoleRepository.DeleteAsync(userRole);
            }
        }
    }
}