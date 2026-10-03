using Library.Application.DTOs.Users;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Application.Services;
using Library.Domain.Constants;
using Library.Domain.Entities;
using NSubstitute;

namespace Library.UnitTests;

public class UserServiceTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IRoleRepository _roleRepo = Substitute.For<IRoleRepository>();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(_userRepo, _roleRepo);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_WhenSelfDemotingAdmin_ThrowsBusinessException()
    {
        var adminId = Guid.NewGuid();
        var adminUser = new User
        {
            Id = adminId,
            UserRoles = new List<UserRole>
            {
                new() { Role = new Role { Name = RoleNames.Admin } }
            }
        };

        _userRepo.GetByIdAsync(adminId).Returns(adminUser);
        _roleRepo.GetByNamesAsync(Arg.Any<IEnumerable<string>>())
            .Returns(new List<Role> { new() { Id = RoleNames.MemberId, Name = RoleNames.Member } });

        var request = new UpdateUserRolesRequest(new List<string> { RoleNames.Member });

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _service.UpdateUserRolesAsync(adminId, request, actingUserId: adminId));

        Assert.Contains("không thể tự gỡ quyền Admin", ex.Message);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_WhenDemotingLastAdmin_ThrowsBusinessException()
    {
        var targetAdminId = Guid.NewGuid();
        var actingAdminId = Guid.NewGuid();

        var targetAdminUser = new User
        {
            Id = targetAdminId,
            UserRoles = new List<UserRole>
            {
                new() { Role = new Role { Name = RoleNames.Admin } }
            }
        };

        _userRepo.GetByIdAsync(targetAdminId).Returns(targetAdminUser);
        _userRepo.CountUsersInRoleAsync(RoleNames.Admin).Returns(1); // Only 1 admin in system

        _roleRepo.GetByNamesAsync(Arg.Any<IEnumerable<string>>())
            .Returns(new List<Role> { new() { Id = RoleNames.StaffId, Name = RoleNames.Staff } });

        var request = new UpdateUserRolesRequest(new List<string> { RoleNames.Staff });

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _service.UpdateUserRolesAsync(targetAdminId, request, actingUserId: actingAdminId));

        Assert.Contains("quản trị viên cuối cùng", ex.Message);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_WhenUnknownRoleRequested_ThrowsBusinessException()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        _userRepo.GetByIdAsync(userId).Returns(user);
        _roleRepo.GetByNamesAsync(Arg.Any<IEnumerable<string>>()).Returns(new List<Role>());

        var request = new UpdateUserRolesRequest(new List<string> { "SuperSaiyan" });

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => _service.UpdateUserRolesAsync(userId, request, actingUserId: Guid.NewGuid()));

        Assert.Contains("Vai trò không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenPhoneTakenByAnotherUser_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var user = new User { Id = userId, PhoneNumber = "0912345678" };
        _userRepo.GetByIdAsync(userId).Returns(user);
        _userRepo.PhoneNumberExistsAsync("0987654321", excludeUserId: userId).Returns(true);

        var request = new UpdateUserRequest("Nguyễn Văn A", "0987654321", "Hà Nội");

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateUserAsync(userId, request));
    }
}
