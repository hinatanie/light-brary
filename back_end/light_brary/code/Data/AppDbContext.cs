using Microsoft.EntityFrameworkCore;
using light_brary.Entities;

namespace light_brary.Data
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
        public DbSet<Fine> Fines => Set<Fine>();
        public DbSet<FineDetail> FineDetails => Set<FineDetail>();

    }
}
