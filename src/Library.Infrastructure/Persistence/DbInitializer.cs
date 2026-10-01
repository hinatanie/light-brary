using Library.Application.Interfaces.Authentication;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Persistence;

public static class DbInitializer
{
    /// <summary>
    /// Kiểm tra nếu hệ thống chưa có người dùng nào (chạy lần đầu) thì tự động tạo tài khoản Admin mặc định
    /// </summary>
    public static async Task SeedAsync(LibraryDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        try
        {
            // Kiểm tra xem bảng AspNetUsers đã có bất kỳ User nào chưa
            var hasUsers = await context.Users.AnyAsync();
            if (!hasUsers)
            {
                logger.LogInformation("🚀 Phát hiện hệ thống chạy lần đầu (chưa có người dùng). Đang tạo tài khoản Admin mặc định...");

                // 1. Tạo Role 'Admin' nếu chưa có
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    adminRole = new Role
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Admin"
                    };
                    await context.Roles.AddAsync(adminRole);
                }

                // 2. Tạo tài khoản Admin theo yêu cầu
                var adminUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FullName = "Quản trị viên (Admin)",
                    Email = "admin@lightbrary.vn",
                    PhoneNumber = "0916114536",
                    PasswordHash = passwordHasher.Hash("Asdf1234!"),
                    Address = "Hệ thống Thư viện LightBrary",
                    CreatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(adminUser);

                // 3. Gán Role Admin cho User
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });

                await context.SaveChangesAsync();
                logger.LogInformation("✅ Đã tạo thành công tài khoản Admin: SĐT '{PhoneNumber}', Email '{Email}', Mật khẩu 'Asdf1234!'.", 
                    adminUser.PhoneNumber, adminUser.Email);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "⚠️ Không thể kiểm tra/khởi tạo dữ liệu người dùng ban đầu. Có thể cơ sở dữ liệu chưa sẵn sàng hoặc chưa apply migration.");
        }
    }
}
