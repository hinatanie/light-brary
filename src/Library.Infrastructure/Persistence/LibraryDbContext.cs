using Library.Application.Exceptions;
using Library.Domain.Constants;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kích hoạt extension citext trong PostgreSQL để so sánh chuỗi không phân biệt hoa/thường ở tầng DB
        modelBuilder.HasPostgresExtension("citext");

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
            .IsUnique()
            .HasDatabaseName("UX_BookCopies_Barcode");

        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => bc.BookId);

        modelBuilder.Entity<Book>()
            .HasIndex(b => b.CategoryId);

        // Filtered unique index cho ISBN (chỉ áp dụng khi ISBN khác NULL)
        modelBuilder.Entity<Book>()
            .HasIndex(b => b.Isbn)
            .IsUnique()
            .HasFilter("\"ISBN\" IS NOT NULL")
            .HasDatabaseName("UX_Books_Isbn");

        modelBuilder.Entity<BorrowRecord>()
            .HasIndex(br => br.UserId);

        modelBuilder.Entity<BorrowDetail>()
            .HasIndex(bd => bd.BorrowRecordId);

        modelBuilder.Entity<BorrowDetail>()
            .HasIndex(bd => bd.BookCopyId);

        // Invariant DB quan trọng: Mỗi bản sao sách tại một thời điểm chỉ được phép có tối đa 1 phiếu mượn đang mượn (Status = 0 = Borrowing)
        modelBuilder.Entity<BorrowDetail>()
            .HasIndex(bd => bd.BookCopyId)
            .IsUnique()
            .HasFilter("\"Status\" = 0")
            .HasDatabaseName("UX_BorrowDetails_ActiveCopy");

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

        // RefreshTokens -> Users: ON DELETE CASCADE
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 4. Data Seeding cố định cho Roles (sử dụng Guid cố định)
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = RoleNames.AdminId, Name = RoleNames.Admin },
            new Role { Id = RoleNames.StaffId, Name = RoleNames.Staff },
            new Role { Id = RoleNames.MemberId, Name = RoleNames.Member }
        );

        // Data Seeding cho Category
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Công nghệ thông tin", Description = "Sách lập trình, kiến trúc phần mềm và cơ sở dữ liệu" },
            new Category { Id = 2, Name = "Kinh tế & Quản trị", Description = "Sách tài chính, khởi nghiệp và quản lý kinh doanh" },
            new Category { Id = 3, Name = "Văn học & Nghệ thuật", Description = "Tiểu thuyết, truyện ngắn và tác phẩm văn học kinh điển" }
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            throw MapPostgresException(pg, ex);
        }
    }

    public override int SaveChanges()
    {
        try
        {
            return base.SaveChanges();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            throw MapPostgresException(pg, ex);
        }
    }

    private static Exception MapPostgresException(PostgresException pg, DbUpdateException original)
    {
        // 23505: unique_violation
        if (pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            var message = pg.ConstraintName switch
            {
                "UX_Users_Email" => "Email này đã được đăng ký trong hệ thống.",
                "UX_Users_PhoneNumber" => "Số điện thoại này đã được đăng ký trong hệ thống.",
                "UX_Roles_Name" => "Tên vai trò đã tồn tại.",
                "UX_Categories_Name" => "Tên thể loại sách đã tồn tại.",
                "UX_Books_Isbn" => "Mã ISBN này đã tồn tại trên một đầu sách khác.",
                "UX_BookCopies_Barcode" => "Mã vạch bản sao sách này đã tồn tại.",
                "UX_BorrowDetails_ActiveCopy" => "Bản sao sách này hiện đang có một phiếu mượn khác đang hoạt động.",
                "UX_RefreshTokens_TokenHash" => "Phiên làm việc bị xung đột token.",
                _ => "Dữ liệu bị trùng lặp với một bản ghi đã tồn tại trong hệ thống."
            };
            return new ConflictException(message);
        }

        // 23503: foreign_key_violation
        if (pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return new ConflictException("Không thể thực hiện do dữ liệu đang được tham chiếu hoặc liên kết với mục khác.");
        }

        // 23514: check_violation
        if (pg.SqlState == PostgresErrorCodes.CheckViolation)
        {
            return new BusinessException("Dữ liệu không thỏa mãn điều kiện hợp lệ của hệ thống.");
        }

        return original;
    }
}
