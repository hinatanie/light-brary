using Library.Application.DTOs.Users;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Application.Validation;
using Library.Domain.Constants;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToResponse).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException($"Không tìm thấy người dùng với mã ID: {id}");

        return MapToResponse(user);
    }

    public async Task UpdateUserRolesAsync(Guid id, UpdateUserRolesRequest request, Guid actingUserId)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");

        var requested = request.RoleNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requested.Count == 0)
            throw new BusinessException("Người dùng phải có ít nhất một vai trò.");

        var roles = await _roleRepository.GetByNamesAsync(requested);
        var unknown = requested.Except(roles.Select(r => r.Name), StringComparer.OrdinalIgnoreCase).ToList();
        if (unknown.Count > 0)
        {
            throw new BusinessException(
                $"Vai trò không hợp lệ: {string.Join(", ", unknown)}. Các vai trò hợp lệ: {string.Join(", ", RoleNames.All)}.");
        }

        var hadAdmin = user.UserRoles.Any(ur => string.Equals(ur.Role?.Name, RoleNames.Admin, StringComparison.OrdinalIgnoreCase));
        var willHaveAdmin = roles.Any(r => string.Equals(r.Name, RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        if (hadAdmin && !willHaveAdmin)
        {
            if (id == actingUserId)
                throw new BusinessException("Bạn không thể tự gỡ quyền Admin của chính mình.");

            if (await _userRepository.CountUsersInRoleAsync(RoleNames.Admin) <= 1)
                throw new BusinessException("Không thể gỡ quyền Admin của quản trị viên cuối cùng.");
        }

        await _userRepository.SetUserRolesAsync(id, roles.Select(r => r.Id).ToList());
    }

    public async Task<List<string>> GetUserRolesAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");

        return await _userRepository.GetUserRolesAsync(id);
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Không tìm thấy người dùng với ID: {id}");

        var phone = PhoneNumberNormalizer.Normalize(request.PhoneNumber);
        if (await _userRepository.PhoneNumberExistsAsync(phone, excludeUserId: id))
            throw new ConflictException($"Số điện thoại '{phone}' đã được đăng ký bởi tài khoản khác.");

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = phone;
        user.Address = request.Address?.Trim();

        await _userRepository.UpdateUserAsync(user);
        return MapToResponse(user);
    }

    private static UserResponse MapToResponse(User u) => new(
        u.Id,
        u.FullName,
        u.Email,
        u.PhoneNumber,
        u.Address,
        u.CreatedAt,
        u.UserRoles
            .Select(ur => ur.Role?.Name)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .ToList());
}
