# 📚 Sổ Tay Lập Trình & Lộ Trình Kỹ Thuật (LightBrary)

Tài liệu được sắp xếp theo **lộ trình chiều sâu kiến thức từ cơ bản đến nâng cao**, giúp người mới dễ dàng tiếp cận theo từng giai đoạn phát triển dự án ASP.NET Core & Clean Architecture.

---

## 🗺️ Lộ Trình Kiến Thức (Learning Roadmap)

```text
[Giai đoạn 1: Cơ bản]
  ├── 1. Cấu trúc Thư mục & Vai trò từng File (Nhận diện dự án)
  └── 2. Các Lệnh .NET CLI Cốt Lõi (Build, Run, dotnet add)
       ↓
[Giai đoạn 2: Công cụ & Môi trường Dev]
  ├── 3. Hạ tầng Container & Biến Môi trường (Docker, .env)
  └── 4. Tích hợp Giao diện Test API (Swagger UI)
       ↓
[Giai đoạn 3: Nghiệp vụ Lõi (Domain)]
  └── 5. Thiết kế Entity Chuẩn Clean Architecture (POCO, Quan hệ)
       ↓
[Giai đoạn 4: Hạ tầng Dữ liệu (Infrastructure & EF Core)]
  ├── 6. Cấu hình Database nâng cao (Fluent API Configuration)
  └── 7. Kết nối PostgreSQL & Quản trị Migration (DbContext, EF CLI)
       ↓
[Tra cứu]
  └── 8. Bảng Tổng Hợp Lệnh Thường Dùng (Cheatsheet)
```

---

## 1. Cấu trúc Thư mục & Vai trò từng File

> 🎯 **Mục tiêu:** Hiểu rõ bức tranh tổng thể dự án, biết từng file sinh ra để làm gì và nằm ở đâu.

Dự án áp dụng mô hình **Clean Architecture (Onion Architecture)** giúp tách biệt rõ ràng các tầng trách nhiệm:

```text
light-brary/
├── compose.yml              # Quản lý container PostgreSQL
├── .env.example             # Mẫu biến môi trường
├── Library.slnx             # File Solution liên kết các project
├── CONTRIBUTING.md          # Bộ quy tắc coding convention của team
├── note.md                  # Sổ tay kỹ thuật & học tập
└── src/                     # Toàn bộ mã nguồn chính thức
    ├── Library.Domain/      # Tầng 1: Cốt lõi nghiệp vụ (Entities, Enums)
    ├── Library.Application/ # Tầng 2: Use cases (Services, DTOs, Validators)
    ├── Library.Infrastructure/# Tầng 3: Hạ tầng (EF Core DbContext, Repositories)
    └── Library.Api/         # Tầng 4: Presentation / REST API
```

### 1.1. Bốn tầng trong `src/`

| Project | Trách nhiệm | Chứa những gì? |
| :--- | :--- | :--- |
| **`Library.Domain`** | Tầng trung tâm, độc lập hoàn toàn, không phụ thuộc thư viện ngoài hay DB. | `Entities/`, `Enums/`, Value Objects, Exceptions lõi. |
| **`Library.Application`**| Chứa logic các ca sử dụng (Use Cases). Phụ thuộc `Domain`. | `Services/`, `DTOs/`, `Validators/`, `Exceptions/`. |
| **`Library.Infrastructure`**| Chịu trách nhiệm về mặt kỹ thuật, kết nối DB, bên thứ ba. | `Persistence/` (DbContext, Migrations), `Repositories/`, `Authentication/`. |
| **`Library.Api`** | Entry point đón nhận request HTTP và trả dữ liệu về client. | `Program.cs`, `Controllers/`, `Middleware/`, cấu hình `appsettings`. |

### 1.2. Chi tiết các file trong project thực thi `src/Library.Api/`

- **[Program.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Program.cs):** Điểm khởi động ứng dụng (.NET chạy file này đầu tiên). Đăng ký dịch vụ (DI) và thiết lập luồng Middleware xử lý request.
- **[Controllers/](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Controllers):** Chứa các controller đón nhận HTTP request (GET/POST/PUT/DELETE) và gọi sang tầng Application.
- **[Library.Api.csproj](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Library.Api.csproj):** File cấu hình project: khai báo phiên bản .NET (`net10.0`), danh sách thư viện NuGet đã cài và liên kết tới các project khác.
- **[Library.Api.http](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Library.Api.http):** File kịch bản HTTP dùng để click gửi test API trực tiếp từ VS Code mà không cần mở Postman.
- **[Properties/launchSettings.json](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Properties/launchSettings.json):** Quy định cổng port chạy ở máy cá nhân (`localhost:5048`, `localhost:7282`) và môi trường `Development`.
- **[appsettings.json](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/appsettings.json):** Cấu hình chung cho toàn bộ ứng dụng (Logging level, cài đặt mặc định).
- **[appsettings.Development.json](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/appsettings.Development.json):** Cấu hình riêng cho máy lập trình viên (chứa chuỗi kết nối Database local). File này sẽ ghi đè `appsettings.json` khi chạy ở môi trường Dev.
- **[WeatherForecast.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/WeatherForecast.cs):** Class dữ liệu mẫu của template .NET, có thể xóa khi bắt đầu code tính năng chính thức.
- **`bin/` và `obj/`:** Thư mục chứa file nhị phân sau khi biên dịch (`.dll`, `.pdb`). Trình biên dịch tự sinh ra, không sửa tay và đã chặn trong `.gitignore`.

> [!NOTE]
> **Thư mục `server/` (Legacy):** Là cấu trúc cũ lúc mới tạo dự án khi chưa phân tách Clean Architecture. Toàn bộ code đã được dời sang `src/`. Các file tạm `bin/`, `obj/` còn lại trong `server/` có thể xóa an toàn bằng lệnh: `rm -rf server`.

---

## 2. Các Lệnh .NET CLI Cốt Lõi

> 🎯 **Mục tiêu:** Nắm vững các thao tác dòng lệnh thiết yếu để build, run và quản lý thư viện/phụ thuộc.

### 2.1. Biên dịch và Chạy ứng dụng

```bash
# 1. Biên dịch toàn bộ dự án kiểm tra lỗi cú pháp
dotnet build

# 2. Khởi chạy Web API thông thường
dotnet run --project src/Library.Api

# 3. Chạy với chế độ Hot Reload (tự động cập nhật khi sửa code)
dotnet watch --project src/Library.Api
```

### 2.2. Lệnh `dotnet add` — Quản trị Dependencies

Lệnh `dotnet add` dùng để **thêm thành phần phụ thuộc** vào dự án (tương tự `npm install` hay `pip install`). Tự động chỉnh sửa file `.csproj` và tải thư viện về máy.

#### A. Cài đặt thư viện NuGet ngoài (`dotnet add package`)
* **Mục đích:** Tải thư viện từ kho [nuget.org](https://www.nuget.org) và gắn vào file `.csproj`.
* **Cú pháp:** `dotnet add [project] package <Tên_Package>`
* **Ví dụ thực tế trong dự án:**
  ```bash
  # Cài giao diện Swagger UI cho Api
  dotnet add src/Library.Api package Swashbuckle.AspNetCore.SwaggerUI

  # Cài driver kết nối PostgreSQL cho Infrastructure
  dotnet add src/Library.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
  ```

#### B. Liên kết các Project trong Solution (`dotnet add reference`)
* **Mục đích:** Cho phép project A được quyền sử dụng code (import namespace, gọi class) từ project B.
* **Cú pháp:** `dotnet add [Project-A] reference [Đường-dẫn-Project-B.csproj]`
* **Ví dụ thực tế trong dự án:**
  ```bash
  # Library.Api cần gọi use cases ở Library.Application:
  dotnet add src/Library.Api reference src/Library.Application/Library.Application.csproj

  # Library.Infrastructure cần triển khai các interface từ Library.Application:
  dotnet add src/Library.Infrastructure reference src/Library.Application/Library.Application.csproj
  ```

> [!TIP]
> Để **gỡ bỏ** một thư viện hoặc liên kết, chỉ cần đổi chữ `add` thành **`remove`**:
> - Gỡ package: `dotnet remove src/Library.Api package Swashbuckle.AspNetCore.SwaggerUI`
> - Gỡ reference: `dotnet remove src/Library.Api reference src/Library.Application/Library.Application.csproj`

---

## 3. Hạ tầng Container & Biến Môi Trường (Docker)

> 🎯 **Mục tiêu:** Tự động hóa việc dựng môi trường Database chuẩn hóa mà không cần cài đặt phần mềm nặng nhọc vào hệ điều hành thật.

### 3.1. `compose.yml` (Docker PostgreSQL)
File định nghĩa container database PostgreSQL phiên bản mới:
```bash
# Khởi động PostgreSQL ở chế độ chạy nền
docker compose up -d

# Xem trạng thái container
docker compose ps

# Dừng container khi không dùng
docker compose down
```

### 3.2. Quản lý bí mật: `.env.example` vs `.env`
- **[.env.example](file:///home/dww/Personal/Code/Csharp/light-brary/.env.example):** Bản mẫu chứa các thông số mặc định (User, Password, Port). File này được commit lên GitHub để ai tải về cũng biết cần điền những gì.
- **`.env` (Thật):** Chứa thông tin cấu hình thực tế. Tạo ra bằng lệnh:
  ```bash
  cp .env.example .env
  ```
- File `.env` thật đã được chặn trong [.gitignore](file:///home/dww/Personal/Code/Csharp/light-brary/.gitignore) để không bao giờ bị lộ mật khẩu lên mạng.

### 3.3. Cấu hình Chuỗi Kết Nối trong .NET
Trong [src/Library.Api/appsettings.Development.json](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/appsettings.Development.json), khai báo chuỗi kết nối khớp với cấu hình trong container:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=lightbrary_db;Username=postgres;Password=Asdf1234!"
}
```

---

## 4. Tích Hợp Giao Diện Test API (Swagger UI)

> 🎯 **Mục tiêu:** Tạo trang web trực quan tại `/swagger` để xem danh sách API và test gửi request dễ dàng.

Trong .NET 9/10, hàm mặc định `app.MapOpenApi()` chỉ xuất ra file JSON thô (`/openapi/v1.json`). Để hiển thị giao diện đồ họa Swagger UI, ta thực hiện:

### Bước 1: Cài đặt Package
```bash
dotnet add src/Library.Api package Swashbuckle.AspNetCore.SwaggerUI
```

### Bước 2: Cấu hình Middleware trong [src/Library.Api/Program.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Program.cs)
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        // Trỏ giao diện Swagger tới file schema OpenAPI v1
        options.SwaggerEndpoint("/openapi/v1.json", "Library API v1");
        options.RoutePrefix = "swagger"; // Đường dẫn truy cập: /swagger
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

👉 Chạy app và truy cập: **[http://localhost:5048/swagger](http://localhost:5048/swagger)**

---

## 5. Thiết Kế Entity Chuẩn Clean Architecture

> 🎯 **Mục tiêu:** Xây dựng mô hình dữ liệu (Domain Entities) chuẩn OOP, đóng gói tốt và không bị phụ thuộc vào database bên ngoài.

### 5.1. Nguyên tắc cốt lõi của Entity tầng Domain
1. **Nằm tại:** `src/Library.Domain/Entities/`.
2. **POCO thuần C#:** Không kế thừa class của EF Core, không gắn các annotation database nặng nề (`[Table]`, `[ForeignKey]`) trực tiếp lên Entity nhằm giữ Domain trong sạch.
3. **Mỗi file một class:** Tên file trùng tên class (ví dụ: `Book.cs`).
4. **Hỗ trợ Audit:** Có các trường theo dõi thời gian (`CreatedAt`, `UpdatedAt`).

### 5.2. Code mẫu hoàn chỉnh: `Category.cs` & `Book.cs`

Tạo file [src/Library.Domain/Entities/Category.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Domain/Entities/Category.cs):
```csharp
namespace Library.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property: Một thể loại có thể chứa nhiều quyển sách (Quan hệ 1-N)
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
```

Tạo file [src/Library.Domain/Entities/Book.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Domain/Entities/Book.cs):
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

    // Khóa ngoại (Foreign Key) trỏ tới Category
    public int CategoryId { get; set; }
    
    // Navigation property: Trỏ ngược về đối tượng Category
    public Category? Category { get; set; }
}
```

---

## 6. Cấu Hình Database Nâng Cao (Fluent API)

> 🎯 **Mục tiêu:** Thiết lập các ràng buộc dữ liệu (độ dài chuỗi, unique index, quan hệ bảng) ở tầng Infrastructure thay vì nhồi nhét vào Domain.

Theo mục 12 trong [CONTRIBUTING.md](file:///home/dww/Personal/Code/Csharp/light-brary/CONTRIBUTING.md), ta tạo configuration riêng cho từng Entity tại `Library.Infrastructure/Persistence/Configurations/`.

Tạo file `src/Library.Infrastructure/Persistence/Configurations/BookConfiguration.cs`:
```csharp
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        // Chỉ định tên bảng trong CSDL
        builder.ToTable("Books");

        // Thiết lập khóa chính
        builder.HasKey(b => b.Id);

        // Ràng buộc thuộc tính
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Isbn)
            .IsRequired()
            .HasMaxLength(20);

        // Đánh chỉ mục Index đảm bảo mã ISBN là duy nhất
        builder.HasIndex(b => b.Isbn).IsUnique();

        // Ràng buộc quan hệ 1 - N (Một Category có nhiều Book)
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## 7. Kết Nối PostgreSQL & Quản Trị Migration (EF Core)

> 🎯 **Mục tiêu:** Kết nối mã nguồn C# với CSDL PostgreSQL và dùng công cụ Migration để tự động sinh bảng, cập nhật cấu trúc database.

### Bước 1: Cài đặt Packages cho EF Core
```bash
# Cài Npgsql PostgreSQL Provider vào Infrastructure
dotnet add src/Library.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL

# Cài công cụ Design (hỗ trợ tạo migration) vào Infrastructure và Api
dotnet add src/Library.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/Library.Api package Microsoft.EntityFrameworkCore.Design
```

### Bước 2: Khởi tạo `LibraryDbContext`
Tạo file `src/Library.Infrastructure/Persistence/LibraryDbContext.cs`:
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

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Tự động tìm và nạp tất cả cấu hình IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### Bước 3: Đăng ký `LibraryDbContext` trong `Program.cs`
Mở [src/Library.Api/Program.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Program.cs) thêm đoạn sau:
```csharp
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### Bước 4: Tạo và Áp Dụng Migration

```bash
# 1. Tạo file migration ghi nhận thay đổi (vd: Tạo bảng lần đầu)
dotnet ef migrations add InitialCreate \
  --project src/Library.Infrastructure \
  --startup-project src/Library.Api \
  --output-dir Persistence/Migrations

# 2. Đẩy cấu trúc bảng vào Database PostgreSQL thật
dotnet ef database update \
  --project src/Library.Infrastructure \
  --startup-project src/Library.Api
```

---

## 8. Bảng Tổng Hợp Lệnh Thường Dùng (Cheatsheet)

| Danh mục | Thao tác | Câu lệnh Terminal |
| :--- | :--- | :--- |
| **Chạy ứng dụng** | Chạy API | `dotnet run --project src/Library.Api` |
| | Chạy tự reload (Hot Reload) | `dotnet watch --project src/Library.Api` |
| | Kiểm tra build lỗi | `dotnet build` |
| **Quản lý Package** | Cài thư viện NuGet | `dotnet add <project> package <Ten_Package>` |
| | Gỡ thư viện | `dotnet remove <project> package <Ten_Package>` |
| | Tạo liên kết giữa 2 project | `dotnet add <projA> reference <projB.csproj>` |
| **Database Docker** | Bật PostgreSQL | `docker compose up -d` |
| | Tắt PostgreSQL | `docker compose down` |
| | Xem trạng thái | `docker compose ps` |
| **EF Core Migration**| Tạo migration mới | `dotnet ef migrations add <TenMigration> --project src/Library.Infrastructure --startup-project src/Library.Api --output-dir Persistence/Migrations` |
| | Cập nhật database | `dotnet ef database update --project src/Library.Infrastructure --startup-project src/Library.Api` |
| **Kiểm thử** | Xem Swagger UI | `http://localhost:5048/swagger` |
| | Xem OpenAPI Schema JSON | `http://localhost:5048/openapi/v1.json` |