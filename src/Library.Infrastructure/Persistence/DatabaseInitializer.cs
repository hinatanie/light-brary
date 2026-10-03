using Library.Application.Exceptions;
using Library.Application.Interfaces.Authentication;
using Library.Application.Validation;
using Library.Domain.Constants;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Persistence;

/// <summary>Cấu hình tạo tài khoản Admin đầu tiên. Đọc từ biến môi trường (Bootstrap__AdminPhone, ...), không bao giờ nằm trong source.</summary>
public sealed class BootstrapAdminOptions
{
    public const string SectionName = "Bootstrap";

    public string? AdminPhone { get; set; }
    public string? AdminEmail { get; set; }
    public string? AdminFullName { get; set; }
    public string? AdminPassword { get; set; }

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(AdminPhone)
        && !string.IsNullOrWhiteSpace(AdminEmail)
        && !string.IsNullOrWhiteSpace(AdminPassword);
}

public static class DatabaseInitializer
{
    /// <summary>
    /// Áp dụng migration hoặc xác nhận không còn migration chờ. KHÔNG nuốt lỗi:
    /// schema hỏng thì ứng dụng phải dừng ngay thay vì chạy tiếp trong trạng thái sai.
    /// </summary>
    public static async Task ApplyMigrationsAsync(LibraryDbContext context, bool autoMigrate, ILogger logger)
    {
        if (autoMigrate)
        {
            logger.LogInformation("Đang áp dụng migration cơ sở dữ liệu (Database:AutoMigrate=true)...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Cơ sở dữ liệu đã ở phiên bản mới nhất.");
            return;
        }

        var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();
        if (pending.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cơ sở dữ liệu còn {pending.Count} migration chưa được áp dụng ({string.Join(", ", pending)}). " +
                "Hãy chạy migration trước khi khởi động ứng dụng, hoặc bật Database:AutoMigrate.");
        }
    }

    /// <summary>
    /// Tạo Admin đầu tiên khi hệ thống CHƯA có ai mang role Admin. Tài khoản bắt buộc đổi mật khẩu ở lần đăng nhập đầu.
    /// </summary>
    /// <param name="requireAdmin">True (Production): thiếu cấu hình bootstrap thì ném lỗi để ứng dụng không khởi động.</param>
    public static async Task BootstrapAdminAsync(
        LibraryDbContext context,
        IPasswordHasher passwordHasher,
        BootstrapAdminOptions? options,
        bool requireAdmin,
        ILogger logger)
    {
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Admin)
            ?? throw new InvalidOperationException("Role 'Admin' chưa được seed. Hãy chạy migration.");

        if (await context.UserRoles.AnyAsync(ur => ur.RoleId == adminRole.Id))
            return;

        if (options == null || !options.IsComplete)
        {
            const string message =
                "Hệ thống chưa có tài khoản Admin. Đặt các biến môi trường Bootstrap__AdminPhone, Bootstrap__AdminEmail, " +
                "Bootstrap__AdminPassword (và tùy chọn Bootstrap__AdminFullName) rồi khởi động lại.";

            if (requireAdmin)
                throw new InvalidOperationException(message);

            logger.LogWarning("{Message}", message);
            return;
        }

        var phone = PhoneNumberNormalizer.Normalize(options.AdminPhone);
        var email = options.AdminEmail!.Trim().ToLowerInvariant();
        PasswordPolicy.EnsureValid(options.AdminPassword, phone, email);

        if (await context.Users.AnyAsync(u => u.Email == email || u.PhoneNumber == phone))
        {
            if (await context.UserRoles.AnyAsync(ur => ur.RoleId == adminRole.Id))
                return;

            throw new InvalidOperationException(
                "Email hoặc số điện thoại trong cấu hình Bootstrap đã thuộc về một tài khoản không phải Admin. " +
                "Hãy dùng thông tin khác hoặc gán role Admin cho tài khoản đó bằng cơ sở dữ liệu.");
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = string.IsNullOrWhiteSpace(options.AdminFullName) ? "Quản trị viên" : options.AdminFullName.Trim(),
            Email = email,
            PhoneNumber = phone,
            PasswordHash = passwordHasher.Hash(options.AdminPassword!),
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });

        await context.Users.AddAsync(admin);

        try
        {
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Đã tạo tài khoản Admin khởi tạo (SĐT ***{PhoneSuffix}). Tài khoản phải đổi mật khẩu ở lần đăng nhập đầu tiên.",
                phone[^3..]);
        }
        catch (ConflictException)
        {
            // Xử lý an toàn khi nhiều instance / test fixture khởi động đồng thời
            if (await context.UserRoles.AnyAsync(ur => ur.RoleId == adminRole.Id))
            {
                logger.LogInformation("Tài khoản Admin đã được tạo bởi một tiến trình đồng thời.");
                return;
            }
            throw;
        }
    }
}
