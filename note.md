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
  └── 7. Toàn Tập Về LibraryDbContext & Quản Trị Database (EF Core)
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

## 7. Toàn Tập Về LibraryDbContext & Quản Trị Database (EF Core)

> 🎯 **Mục tiêu:** Hiểu rõ bản chất của `LibraryDbContext` (cầu nối giữa C# và Database, Change Tracking, Unit of Work) và nắm vững quy trình 6 bước từ cài đặt, cấu hình, migration cho đến thao tác CRUD thực tế.

---

### 7.1. `LibraryDbContext` Là Gì & Mục Đích Tồn Tại Của Nó?

`LibraryDbContext` (kế thừa từ `DbContext` của Entity Framework Core) là **thành phần trung tâm của tầng Hạ tầng (`Library.Infrastructure`)**. Nó đóng vai trò là **cây cầu duy nhất** kết nối giữa mã nguồn hướng đối tượng C# và cơ sở dữ liệu quan hệ PostgreSQL.

```text
[ Ứng dụng C# ]                                                [ Database ]
Entities (Book, Category) <---> [ LibraryDbContext ] <---> Bảng PostgreSQL (Books, Categories)
                                       │
                      ┌────────────────┴────────────────┐
                      ▼                                 ▼
         Dịch LINQ thành câu lệnh SQL          Quản lý Transaction an toàn
         (SELECT, INSERT, UPDATE, DELETE)           (Mẫu Unit of Work)
```

#### 4 Nhiệm vụ cốt lõi:
1. **Quản lý phiên kết nối (Database Session):** Chịu trách nhiệm mở kết nối tới PostgreSQL khi cần, gửi lệnh thực thi và tự động đóng/giải phóng kết nối an toàn.
2. **Cung cấp các bảng dữ liệu ảo (`DbSet<T>`):** Mỗi thuộc tính `DbSet<Book>`, `DbSet<Category>` đại diện cho một bảng trong CSDL, cho phép bạn viết truy vấn bằng mã C# thuần (LINQ) thay vì phải tự viết các câu lệnh SQL dài dòng.
3. **Theo dõi sự thay đổi (Change Tracking):** Khi bạn lấy một đối tượng lên từ DB và sửa giá trị (ví dụ: `book.AvailableCopies -= 1;`), DbContext sẽ tự động ghi nhớ trạng thái này để tự sinh ra câu lệnh `UPDATE` tương ứng khi lưu.
4. **Mẫu thiết kế Unit of Work:** Khi bạn thực hiện thêm, sửa, xóa nhiều bảng cùng lúc, EF Core sẽ gom tất cả lại và chỉ thực thi trong **duy nhất một Transaction** khi bạn gọi hàm `await _context.SaveChangesAsync()`. Nếu có bất kỳ dòng nào bị lỗi, toàn bộ thay đổi sẽ được rollback (hủy bỏ), đảm bảo tính toàn vẹn dữ liệu.

> [!IMPORTANT]
> **Vòng đời của DbContext (Scoped Lifetime):**
> Trong ASP.NET Core, `LibraryDbContext` luôn được đăng ký với vòng đời **Scoped**. Điều này có nghĩa là mỗi một HTTP request gửi tới server sẽ được cấp một phiên bản (instance) `LibraryDbContext` riêng biệt và sẽ tự động giải phóng khi request đó kết thúc. Không bao giờ dùng Singleton cho DbContext vì nó không an toàn với đa luồng (thread-unsafe).

---

### 7.2. Toàn Bộ Quy Trình 6 Bước Vận Hành `LibraryDbContext`

Để đưa `LibraryDbContext` vào hoạt động hoàn chỉnh từ lúc khởi tạo đến khi thực sự truy vấn dữ liệu, ta thực hiện tuần tự 6 bước:

```text
Bước 1: Cài đặt Packages EF Core
  ↓
Bước 2: Chuẩn bị Domain Entities (Book, Category...)
  ↓
Bước 3: Viết class LibraryDbContext (DbSet, OnModelCreating)
  ↓
Bước 4: Cấu hình Connection String & Đăng ký DI trong Program.cs
  ↓
Bước 5: Chạy Migration (Sinh bảng thật trong PostgreSQL)
  ↓
Bước 6: Inject LibraryDbContext vào Repository/Service để CRUD
```

#### Bước 1: Cài đặt Packages cần thiết
Cài driver PostgreSQL cho tầng Infrastructure và công cụ Design để hỗ trợ chạy CLI migration:
```bash
# 1. Driver PostgreSQL cho Infrastructure
dotnet add src/Library.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL

# 2. Công cụ Design (cho cả Infrastructure và Api)
dotnet add src/Library.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/Library.Api package Microsoft.EntityFrameworkCore.Design
```

#### Bước 2: Chuẩn bị Domain Entities
DbContext cần biết nó sẽ quản lý những thực thể nào (đã được định nghĩa ở mục 5 tại `src/Library.Domain/Entities/` như `Book.cs`, `Category.cs`).

#### Bước 3: Khởi tạo file `LibraryDbContext.cs`
Tạo file tại `src/Library.Infrastructure/Persistence/LibraryDbContext.cs`:

```csharp
using System.Reflection;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence;

public class LibraryDbContext : DbContext
{
    // Constructor nhận cấu hình DbContextOptions (chứa connection string) được truyền từ Program.cs
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    // Khai báo các DbSet đại diện cho các bảng dữ liệu
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tự động tìm và áp dụng toàn bộ file cấu hình Fluent API (trong thư mục Configurations/)
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

#### Bước 4: Khai báo Connection String & Đăng ký Dependency Injection
1. Cấu hình thông số kết nối trong [src/Library.Api/appsettings.Development.json](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/appsettings.Development.json):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=lightbrary_db;Username=postgres;Password=Asdf1234!"
   }
   ```
2. Đăng ký DbContext vào hệ thống trong [src/Library.Api/Program.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Program.cs):
   ```csharp
   using Library.Infrastructure.Persistence;
   using Microsoft.EntityFrameworkCore;

   var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

   builder.Services.AddDbContext<LibraryDbContext>(options =>
       options.UseNpgsql(connectionString));
   ```

#### Bước 5: Tạo và Áp Dụng Migration
Đảm bảo container PostgreSQL đang chạy (`docker compose up -d`), sau đó dùng lệnh CLI để tạo bảng thật:
```bash
# 1. Tạo file migration ghi nhận thay đổi
dotnet ef migrations add InitialCreate \
  --project src/Library.Infrastructure \
  --startup-project src/Library.Api \
  --output-dir Persistence/Migrations

# 2. Đẩy cấu trúc bảng vào PostgreSQL
dotnet ef database update \
  --project src/Library.Infrastructure \
  --startup-project src/Library.Api
```

#### Bước 6: Sử dụng `LibraryDbContext` trong code thực tế (Repository / Service)
Theo nguyên lý Dependency Injection, tầng Repository sẽ nhận `LibraryDbContext` thông qua hàm khởi tạo (Constructor Injection) để thực hiện các thao tác CRUD:

```csharp
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class BookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    // 1. READ: Lấy danh sách kèm theo thông tin Category (JOIN)
    public async Task<List<Book>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Category) // Tương đương câu lệnh SQL JOIN
            .AsNoTracking()          // Tối ưu tốc độ khi chỉ đọc dữ liệu
            .ToListAsync();
    }

    // 2. READ BY ID: Tìm sách theo khóa chính
    public async Task<Book?> GetBookByIdAsync(Guid id)
    {
        return await _context.Books.FindAsync(id);
    }

    // 3. CREATE: Thêm một cuốn sách mới
    public async Task AddBookAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync(); // Sinh câu lệnh INSERT INTO
    }

    // 4. UPDATE: Cập nhật thông tin sách
    public async Task UpdateBookAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync(); // Sinh câu lệnh UPDATE ... WHERE Id = ...
    }

    // 5. DELETE: Xóa sách
    public async Task DeleteBookAsync(Guid id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync(); // Sinh câu lệnh DELETE FROM ... WHERE Id = ...
        }
    }
}
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