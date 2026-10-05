using Microsoft.EntityFrameworkCore;
using light_brary.Domain.Entities;

namespace light_brary.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookCopy> BookCopies => Set<BookCopy>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();
        public DbSet<BorrowDetail> BorrowDetails => Set<BorrowDetail>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình composite key và mối quan hệ cho UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
