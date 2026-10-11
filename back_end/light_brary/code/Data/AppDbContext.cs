using Microsoft.EntityFrameworkCore;
using light_brary.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace light_brary.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookCopy> BookCopies => Set<BookCopy>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();
        public DbSet<BorrowDetail> BorrowDetails => Set<BorrowDetail>();
        public DbSet<Fine> Fines => Set<Fine>();
        public DbSet<FineDetail> FineDetails => Set<FineDetail>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().Property(user => user.PhoneNumber)
                .IsRequired()
                .HasMaxLength(10);
        }

    }
}
