# 📚 Sổ Tay Lập Trình & Hướng Dẫn Kỹ Thuật (LightBrary)

Tài liệu tóm tắt kiến trúc dự án, hướng dẫn vận hành, cấu hình công cụ (Swagger, Database) và quy chuẩn viết code Entity theo Clean Architecture trong .NET.

---

## 📑 Mục lục
1. [Kiến trúc & Cấu trúc Thư mục Dự án](#1-kiến-trúc--cấu-trúc-thư-mục-dự-án)
   - [Thư mục `src/`](#11-thư-mục-src-mã-nguồn-chính)
   - [Thư mục `server/` (Legacy)](#12-thư-mục-server-cũ--legacy)
2. [Hạ tầng & Cấu hình Môi trường](#2-hạ-tầng--cấu-hình-môi-trường)
   - [`compose.yml` (Docker PostgreSQL)](#21-composeyml--chạy-database-qua-docker)
   - [`.env.example` & `.env`](#22-envexample--quản-lý-biến-môi-trường)
   - [`appsettings.Development.json`](#23-appsettingsdevelopmentjson--chuỗi-kết-nối)
3. [Hướng dẫn Cấu hình Swagger UI](#3-hướng-dẫn-cấu-hình-swagger-ui)
4. [Hướng dẫn Cấu hình Kết nối Database (PostgreSQL & EF Core)](#4-hướng-dẫn-cấu-hình-kết-nối-database-postgresql--ef-core)
   - [Cài đặt Package](#bước-1-cài-đặt-các-package-cần-thiết)
   - [Khởi tạo `LibraryDbContext`](#bước-2-khởi-tạo-librarydbcontext)
   - [Đăng ký DbContext trong `Program.cs`](#bước-3-đăng-ký-dbcontext-trong-programcs)
   - [Tạo và chạy Migration](#bước-4-tạo-và-áp-dụng-migration)
5. [Cách Code một Entity Chuẩn theo Clean Architecture](#5-cách-code-một-entity-chuẩn-theo-clean-architecture)
   - [Quy tắc thiết kế Entity trong tầng Domain](#51-quy-tắc-thiết-kế-entity-trong-tầng-domain)
   - [Code mẫu Entity: `Book.cs` & `Category.cs`](#52-code-mẫu-entity-domain)
   - [Cấu hình Fluent API (`IEntityTypeConfiguration`)](#53-cấu-hình-fluent-api-persistenceconfigurations)
6. [Bảng Tra Cứu Lệnh Thường Dùng (Cheatsheet)](#6-bảng-tra-cứu-lệnh-thường-dùng-cheatsheet)

---

## 1. Kiến trúc & Cấu trúc Thư mục Dự án

Dự án áp dụng mô hình **Clean Architecture (Onion Architecture)** giúp tách biệt rõ ràng các tầng trách nhiệm, dễ bảo trì và mở rộng.

```text
light-brary/
├── compose.yml              # Quản lý container PostgreSQL
├── .env.example             # Mẫu biến môi trường
├── Library.slnx             # Solution file
├── CONTRIBUTING.md          # Bộ quy tắc lập trình của nhóm
└── src/                     # Toàn bộ mã nguồn chính
    ├── Library.Domain/      # Tầng 1: Cốt lõi nghiệp vụ (Entities, Enums)
    ├── Library.Application/ # Tầng 2: Use cases (Services, DTOs, Validators)
    ├── Library.Infrastructure/# Tầng 3: Hạ tầng (EF Core DbContext, Repositories)
    └── Library.Api/         # Tầng 4: Presentation / REST API
```

### 1.1. Thư mục `src/` (Mã nguồn chính)

| Project | Nhiệm vụ chính | Các thành phần chứa bên trong |
| :--- | :--- | :--- |
| **`Library.Domain`** | Tầng trung tâm, không phụ thuộc vào bất kỳ thư viện ngoài hay DB nào. | `Entities/`, `Enums/`, Value Objects, Exceptions lõi. |
| **`Library.Application`** | Chứa logic của từng Use Case (nghiệp vụ). Phụ thuộc vào `Domain`. | `Services/`, `DTOs/`, `Validators/`, `Exceptions/`. |
| **`Library.Infrastructure`** | Triển khai kỹ thuật, giao tiếp cơ sở dữ liệu, dịch vụ bên ngoài. | `Persistence/` (DbContext, Migrations, Configurations), `Repositories/`, `Authentication/` (JWT). |
| **`Library.Api`** | Entry point của hệ thống, đón nhận HTTP request và trả kết quả. | `Program.cs`, `Controllers/`, `Middleware/`, `appsettings.json`. |

### 1.2. Thư mục `server/` (Cũ / Legacy)

- Là cấu trúc ban đầu khi mới khởi tạo dự án (toàn bộ code dồn trong 1 project).
- Sau khi được refactor sang Clean Architecture (`src/`), toàn bộ code trong `server/` đã được chuyển và xóa khỏi Git.
- Các thư mục `bin/` và `obj/` còn sót lại trên máy là do Git bỏ qua (`.gitignore`). Bạn hoàn toàn có thể xóa an toàn:
  ```bash
  rm -rf server
  ```

---

## 2. Hạ tầng & Cấu hình Môi trường

### 2.1. `compose.yml` — Chạy Database qua Docker
- Chạy container PostgreSQL 17 độc lập, không cần cài PostgreSQL trực tiếp vào máy thật.
- Có cấu hình volume `postgres_data` để giữ dữ liệu bền vững khi tắt/mở máy.
- Lệnh khởi động container:
  ```bash
  docker compose up -d
  ```

### 2.2. `.env.example` — Quản lý biến môi trường
- Chứa các biến mẫu: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `POSTGRES_PORT`.
- **Cách dùng:** Tạo file `.env` cá nhân trước khi chạy:
  ```bash
  cp .env.example .env
  ```
- File `.env` thật đã được đưa vào [.gitignore](file:///home/dww/Personal/Code/Csharp/light-brary/.gitignore) để tuyệt đối không đẩy mật khẩu lên GitHub.

### 2.3. `appsettings.Development.json` — Chuỗi kết nối
Chỉ định cho backend .NET biết cách kết nối tới PostgreSQL đang chạy trong Docker:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=lightbrary_db;Username=postgres;Password=Asdf1234!"
}
```

---

## 3. Hướng dẫn Cấu hình Swagger UI

Trong .NET mới (.NET 9 / 10), thư viện mặc định `Microsoft.AspNetCore.OpenApi` chỉ xuất ra file tài liệu JSON thô tại `/openapi/v1.json`. Để có giao diện tương tác trực quan **Swagger UI**, thực hiện 2 bước sau:

### Bước 1: Cài đặt package SwaggerUI vào `Library.Api`
```bash
dotnet add src/Library.Api package Swashbuckle.AspNetCore.SwaggerUI
```

### Bước 2: Cấu hình middleware trong `src/Library.Api/Program.cs`
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 1. Tạo endpoint schema JSON: /openapi/v1.json
    app.MapOpenApi();

    // 2. Kích hoạt giao diện đồ họa Swagger UI
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Library API v1");
        options.RoutePrefix = "swagger"; // Truy cập bằng: http://localhost:5048/swagger
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

> [!NOTE]
> Khởi động ứng dụng bằng `dotnet run --project src/Library.Api` và truy cập trình duyệt tại:
> 👉 **[http://localhost:5048/swagger](http://localhost:5048/swagger)**

---

## 4. Hướng dẫn Cấu hình Kết nối Database (PostgreSQL & EF Core)

### Bước 1: Cài đặt các Package cần thiết

1. **Cài PostgreSQL Provider vào `Library.Infrastructure`:**
   ```bash
   dotnet add src/Library.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
   ```
2. **Cài công cụ Design & Migration vào `Library.Api` và `Library.Infrastructure`:**
   ```bash
   dotnet add src/Library.Infrastructure package Microsoft.EntityFrameworkCore.Design
   dotnet add src/Library.Api package Microsoft.EntityFrameworkCore.Design
   ```

### Bước 2: Khởi tạo `LibraryDbContext`
Tạo file tại: `src/Library.Infrastructure/Persistence/LibraryDbContext.cs`:

```csharp
using System.Reflection;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    // Khai báo các DbSet đại diện cho các bảng
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Tự động quét và áp dụng tất cả IEntityTypeConfiguration trong assembly này
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### Bước 3: Đăng ký DbContext trong `Program.cs`
Trong file `src/Library.Api/Program.cs`:

```csharp
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// Lấy connection string từ appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký LibraryDbContext sử dụng PostgreSQL
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### Bước 4: Tạo và áp dụng Migration

Khi bạn tạo mới hoặc thay đổi Entity, thực hiện các lệnh sau:

1. **Tạo Migration mới:**
   ```bash
   dotnet ef migrations add InitialCreate \
     --project src/Library.Infrastructure \
     --startup-project src/Library.Api \
     --output-dir Persistence/Migrations
   ```
2. **Cập nhật database (áp dụng thay đổi vào PostgreSQL):**
   ```bash
   dotnet ef database update \
     --project src/Library.Infrastructure \
     --startup-project src/Library.Api
   ```

---

## 5. Cách Code một Entity Chuẩn theo Clean Architecture

### 5.1. Quy tắc thiết kế Entity trong tầng Domain
Theo tôn chỉ của Clean Architecture:
- **Nằm tại:** `src/Library.Domain/Entities/`.
- **Pure C# (POCO):** Không kế thừa các class của Entity Framework, không lạm dụng Data Annotations (`[Key]`, `[Column]`, `[ForeignKey]`) trực tiếp trên Entity nếu muốn giữ Domain độc lập. Thay vào đó, dùng **Fluent API** ở tầng Infrastructure để cấu hình database.
- **Tính đóng gói:** Đặt kiểu dữ liệu chính xác, hỗ trợ thuộc tính audit (`CreatedAt`, `UpdatedAt`), sử dụng `Guid` hoặc `int` cho khóa chính.

### 5.2. Code mẫu Entity: Domain

Tạo file: `src/Library.Domain/Entities/Category.cs`:
```csharp
namespace Library.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property: Một thể loại có nhiều sách
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
```

Tạo file: `src/Library.Domain/Entities/Book.cs`:
```csharp
namespace Library.Domain.Entities;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Khóa ngoại liên kết tới Category
    public int CategoryId { get; set; }
    // Navigation property
    public Category? Category { get; set; }
}
```

### 5.3. Cấu hình Fluent API (`Persistence/Configurations`)
Theo quy định mục 12 trong `CONTRIBUTING.md`, mọi quy tắc ràng buộc database (độ dài, khóa ngoại, unique index, tên bảng) được tách riêng ở tầng Infrastructure.

Tạo file: `src/Library.Infrastructure/Persistence/Configurations/BookConfiguration.cs`:
```csharp
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        // Tên bảng
        builder.ToTable("Books");

        // Khóa chính
        builder.HasKey(b => b.Id);

        // Các thuộc tính
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Isbn)
            .IsRequired()
            .HasMaxLength(20);

        // Đảm bảo mã ISBN là duy nhất
        builder.HasIndex(b => b.Isbn).IsUnique();

        // Ràng buộc quan hệ 1-N (Category - Book)
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## 6. Bảng Tra Cứu Lệnh Thường Dùng (Cheatsheet)

| Tác vụ | Câu lệnh Terminal |
| :--- | :--- |
| **Bật PostgreSQL** | `docker compose up -d` |
| **Dừng PostgreSQL** | `docker compose down` |
| **Kiểm tra trạng thái DB** | `docker compose ps` |
| **Build solution** | `dotnet build` |
| **Chạy Web API** | `dotnet run --project src/Library.Api` |
| **Chạy chế độ Hot Reload** | `dotnet watch --project src/Library.Api` |
| **Tạo Migration mới** | `dotnet ef migrations add <TenMigration> --project src/Library.Infrastructure --startup-project src/Library.Api --output-dir Persistence/Migrations` |
| **Áp dụng Migration vào DB** | `dotnet ef database update --project src/Library.Infrastructure --startup-project src/Library.Api` |
| **Truy cập Swagger** | `http://localhost:5048/swagger` |