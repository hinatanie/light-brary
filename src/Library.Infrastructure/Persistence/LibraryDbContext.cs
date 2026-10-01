using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();
    public DbSet<BorrowDetail> BorrowDetails => Set<BorrowDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Composite Primary Key cho UserRole
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // 2. Indexes & Unique Constraints
        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => bc.Barcode)
            .IsUnique();

        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => bc.BookId);

        modelBuilder.Entity<Book>()
            .HasIndex(b => b.CategoryId);

        modelBuilder.Entity<BorrowRecord>()
            .HasIndex(br => br.UserId);

        modelBuilder.Entity<BorrowDetail>()
            .HasIndex(bd => bd.BorrowRecordId);

        modelBuilder.Entity<BorrowDetail>()
            .HasIndex(bd => bd.BookCopyId);

        // 3. Foreign Key Delete Behaviors
        // Books -> Categories: ON DELETE RESTRICT
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // BookCopies -> Books: ON DELETE CASCADE
        modelBuilder.Entity<BookCopy>()
            .HasOne(bc => bc.Book)
            .WithMany(b => b.BookCopies)
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // BorrowRecords -> Users: ON DELETE RESTRICT (NO ACTION)
        modelBuilder.Entity<BorrowRecord>()
            .HasOne(br => br.User)
            .WithMany(u => u.BorrowRecords)
            .HasForeignKey(br => br.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // BorrowDetails -> BorrowRecords: ON DELETE CASCADE
        modelBuilder.Entity<BorrowDetail>()
            .HasOne(bd => bd.BorrowRecord)
            .WithMany(br => br.BorrowDetails)
            .HasForeignKey(bd => bd.BorrowRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // BorrowDetails -> BookCopies: ON DELETE RESTRICT (NO ACTION)
        modelBuilder.Entity<BorrowDetail>()
            .HasOne(bd => bd.BookCopy)
            .WithMany(bc => bc.BorrowDetails)
            .HasForeignKey(bd => bd.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);

        // 4. Data Seeding cho Category để có sẵn dữ liệu mẫu liên kết
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Công nghệ thông tin", Description = "Sách lập trình, kiến trúc phần mềm và cơ sở dữ liệu" },
            new Category { Id = 2, Name = "Kinh tế & Quản trị", Description = "Sách tài chính, khởi nghiệp và quản lý kinh doanh" },
            new Category { Id = 3, Name = "Văn học & Nghệ thuật", Description = "Tiểu thuyết, truyện ngắn và tác phẩm văn học kinh điển" }
        );
    }
}
