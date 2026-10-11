using Microsoft.EntityFrameworkCore;
using light_brary.Services.Interfaces;
using light_brary.Entities;

namespace light_brary.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            // 1. Khởi tạo các Role cơ bản nếu chưa có (Admin, Librarian, User)
            var defaultRoles = new[] { Role.ADMIN, Role.LIBRARIAN, Role.USER };
            foreach (var roleName in defaultRoles)
            {
                if (!await context.Roles.AnyAsync(r => r.Name == roleName))
                {
                    context.Roles.Add(new Role { Name = roleName });
                }
            }
            await context.SaveChangesAsync();

            // 2. Kiểm tra nếu chưa có người dùng nào trong database thì tạo tài khoản Admin đầu tiên
            const string adminPhone = "0999999999";
            if (!await context.Users.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == Role.ADMIN);

                var adminUser = new User
                {
                    FullName = "Administrator",
                    PhoneNumber = adminPhone,
                    Address = "Hệ thống LightBrary",
                    PasswordHash = passwordHasher.Hash("Asdf1234!"),
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                await context.SaveChangesAsync();
            }
        }
    }
}

