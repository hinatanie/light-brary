using light_brary.Constants;
using light_brary.Entities;
using light_brary.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager, IConfiguration configuration)
        {
            var defaultRoles = new[] { RoleNames.ADMIN, RoleNames.LIBRARIAN, RoleNames.USER };
            foreach (var roleName in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    (await roleManager.CreateAsync(new IdentityRole<Guid>
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName
                    })).EnsureSucceeded();
                }
            }

            if (await userManager.Users.AnyAsync()) return;

            var password = configuration["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Cần cấu hình Seed:AdminPassword bằng User Secrets hoặc biến môi trường Seed__AdminPassword để tạo Admin đầu tiên.");

            var phone = configuration["Seed:AdminPhone"] ?? "0999999999";
            var admin = new User
            {
                Id = Guid.NewGuid(),
                UserName = phone,
                PhoneNumber = phone,
                FullName = "Administrator",
                Address = "Hệ thống LightBrary",
                CreatedAt = DateTime.UtcNow
            };
            (await userManager.CreateAsync(admin, password)).EnsureSucceeded();
            (await userManager.AddToRoleAsync(admin, RoleNames.ADMIN)).EnsureSucceeded();
        }
    }
}
