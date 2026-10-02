using Library.Application.Interfaces.Authentication;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Persistence;

public static class DbInitializer
{
    /// <summary>
    /// Tự động áp dụng migration và khởi tạo tài khoản Admin mặc định khi chạy lần đầu
    /// </summary>
    public static async Task SeedAsync(LibraryDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        try
        {
            // 1. Tự động áp dụng tất cả các migrations còn thiếu vào database
            logger.LogInformation("🔄 Đang kiểm tra và cập nhật cấu trúc cơ sở dữ liệu (Auto Migration)...");
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ Cập nhật cấu trúc cơ sở dữ liệu thành công!");

            // 2. Kiểm tra xem bảng AspNetUsers đã có bất kỳ User nào chưa
            var hasUsers = await context.Users.AnyAsync();
            if (!hasUsers)
            {
                logger.LogInformation("🚀 Phát hiện hệ thống chạy lần đầu (chưa có người dùng). Đang tạo tài khoản Admin mặc định...");

                // Tạo Role 'Admin' nếu chưa có
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

                var staffRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Staff");
                if (staffRole == null)
                {
                    staffRole = new Role { Id = Guid.NewGuid().ToString(), Name = "Staff" };
                    await context.Roles.AddAsync(staffRole);
                }

                var memberRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Member");
                if (memberRole == null)
                {
                    memberRole = new Role { Id = Guid.NewGuid().ToString(), Name = "Member" };
                    await context.Roles.AddAsync(memberRole);
                }

                // Tạo tài khoản Admin theo yêu cầu
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

                // Gán Role Admin cho User
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
            logger.LogWarning(ex, "⚠️ Không thể tự động migrate hoặc khởi tạo dữ liệu ban đầu. Vui lòng kiểm tra kết nối PostgreSQL.");
        }
    }
}
