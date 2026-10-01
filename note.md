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
  └── 5. Thiết Kế Entity Theo Phong Cách Spring Boot (Data Annotations, Quan Hệ)
       ↓
[Giai đoạn 4: Hạ tầng Dữ liệu (Infrastructure & EF Core)]
  ├── 6. Bảng Tra Cứu Annotation (Spring Boot vs .NET) & Cơ Chế Mapping
  └── 7. Toàn Tập Về LibraryDbContext & Quản Trị Database (EF Core)
       ↓
[Giai đoạn 5: Hiện Thực Tính Năng & Bảo Mật (Features & Security)]
  ├── 8. Toàn Tập Luồng Hoạt Động CRUD Đầu Sách (Book API - Clean Architecture)
  └── 9. Chuyên Đề Xác Thực & Phân Quyền (Authentication, BCrypt, JWT & Seed Admin Lần Đầu)
       ↓
[Tra cứu]
  └── 10. Bảng Tổng Hợp Lệnh Thường Dùng (Cheatsheet)
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

## 5. Thiết Kế Entity Theo Phong Cách Spring Boot (Data Annotations)

> 🎯 **Mục tiêu:** Khai báo toàn bộ ràng buộc (Khóa chính, Not Null, Độ dài, Tên bảng, Quan hệ 1-N...) trực tiếp ngay bên trong class Entity, mang lại trải nghiệm tiện lợi và quen thuộc như JPA/Hibernate trong Spring Boot.

### 5.1. Nguyên tắc thiết kế Entity theo Data Annotations
1. **Nằm tại:** `src/Library.Domain/Entities/`.
2. **Tất cả ràng buộc nằm trọn trong Entity:** Sử dụng các Data Annotations có sẵn trong `System.ComponentModel.DataAnnotations` và `System.ComponentModel.DataAnnotations.Schema` (thư viện chuẩn của .NET, không cần cài thêm package DB nào vào `Domain`).
3. **Không cần tạo file cấu hình riêng:** Toàn bộ metadata đã nằm trên từng thuộc tính, không cần phải tạo thêm các file `IEntityTypeConfiguration` rườm rà.
4. **Hỗ trợ Audit:** Có các trường theo dõi thời gian tạo và cập nhật (`CreatedAt`, `UpdatedAt`).

### 5.2. Code mẫu hoàn chỉnh: `Category.cs` & `Book.cs`

Tạo file [src/Library.Domain/Entities/Category.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Domain/Entities/Category.cs):
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

[Table("categories")] // Tương đương @Table(name = "categories")
public class Category
{
    [Key] // Tương đương @Id
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tương đương @GeneratedValue(strategy = GenerationType.IDENTITY)
    public int Id { get; set; }

    [Required] // Tương đương @Column(nullable = false)
    [MaxLength(100)] // Tương đương @Column(length = 100)
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    // Navigation property: Một thể loại có nhiều quyển sách (Tương đương @OneToMany(mappedBy = "category"))
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
```

Tạo file [src/Library.Domain/Entities/Book.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Domain/Entities/Book.cs):
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

[Table("books")] // Tương đương @Table(name = "books")
public class Book
{
    [Key] // Tương đương @Id
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] // Tương đương @Column(nullable = false)
    [MaxLength(250)] // Tương đương @Column(length = 250)
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Column("author")]
    public string Author { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("isbn")]
    public string Isbn { get; set; } = string.Empty;

    [Column("published_year")]
    public int PublishedYear { get; set; }

    [Column("available_copies")]
    public int AvailableCopies { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Khóa ngoại (Foreign Key) trỏ tới Category
    [Column("category_id")]
    public int CategoryId { get; set; }

    // Navigation property: Trỏ ngược về Category (Tương đương @ManyToOne + @JoinColumn(name = "category_id"))
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }
}
```

---

## 6. Bảng Tra Cứu Annotation (Spring Boot JPA vs .NET EF Core)

> 🎯 **Mục tiêu:** Giúp lập trình viên từ Spring Boot tra cứu nhanh các annotation tương ứng trong C# .NET để thiết kế Entity mà không cần bỡ ngỡ.

### 6.1. Bảng đối chiếu chi tiết

| Tính năng | Spring Boot (JPA / Hibernate) | .NET (EF Core Data Annotations) | Namespace trong C# |
| :--- | :--- | :--- | :--- |
| **Khai báo Entity** | `@Entity` | *Không cần* (Class POCO thông thường) | — |
| **Tên bảng** | `@Table(name = "books")` | `[Table("books")]` | `System.ComponentModel.DataAnnotations.Schema` |
| **Khóa chính** | `@Id` | `[Key]` | `System.ComponentModel.DataAnnotations` |
| **Tự sinh giá trị** | `@GeneratedValue(strategy = IDENTITY)` | `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` | `System.ComponentModel.DataAnnotations.Schema` |
| **Tên cột** | `@Column(name = "book_title")` | `[Column("book_title")]` | `System.ComponentModel.DataAnnotations.Schema` |
| **Bắt buộc / Not null** | `@Column(nullable = false)` | `[Required]` hoặc thuộc tính non-nullable `string` | `System.ComponentModel.DataAnnotations` |
| **Giới hạn độ dài** | `@Column(length = 200)` | `[MaxLength(200)]` hoặc `[StringLength(200)]` | `System.ComponentModel.DataAnnotations` |
| **Kiểu cột dữ liệu** | `@Column(columnDefinition = "TEXT")` | `[Column(TypeName = "text")]` | `System.ComponentModel.DataAnnotations.Schema` |
| **Độ chính xác số tiền**| `@Column(precision = 18, scale = 2)` | `[Column(TypeName = "decimal(18,2)")]` | `System.ComponentModel.DataAnnotations.Schema` |
| **Bỏ qua không lưu DB**| `@Transient` | `[NotMapped]` | `System.ComponentModel.DataAnnotations.Schema` |
| **Quan hệ N - 1** | `@ManyToOne` + `@JoinColumn(name = "category_id")` | `[ForeignKey(nameof(CategoryId))]` | `System.ComponentModel.DataAnnotations.Schema` |
| **Quan hệ 1 - N** | `@OneToMany(mappedBy = "category")` | `public ICollection<Book> Books { get; set; }` | Tự động nhận diện qua Navigation Property |

### 6.2. Ưu điểm khi dùng Data Annotations
1. **Trực quan, quen thuộc:** Nhìn vào class Entity là thấy ngay tên bảng, tên cột, độ dài chuỗi và các ràng buộc y hệt như code Entity trong Spring Boot.
2. **Không tốn công tạo nhiều file:** Bỏ qua hoàn toàn việc phải tạo hàng chục file `XxxConfiguration.cs`.
3. **Tái sử dụng Validation:** Các annotation như `[Required]`, `[MaxLength]` có thể tái sử dụng ngay trong các lớp DTO để validate dữ liệu đầu vào của API.

> [!NOTE]
> **Xử lý Unique Index:** Với các ràng buộc đặc thù mà Data Annotation cơ bản chưa hỗ trợ trực tiếp (như đánh chỉ mục duy nhất cho `Isbn`), ta chỉ cần khai báo ngắn gọn trong hàm `OnModelCreating` của [LibraryDbContext.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Infrastructure/Persistence/LibraryDbContext.cs) mà không cần tạo file Configuration riêng rẽ.

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
DbContext cần biết nó sẽ quản lý những thực thể nào (đã được định nghĩa và gắn Data Annotations theo phong cách Spring Boot ở mục 5 tại `src/Library.Domain/Entities/` như `Book.cs`, `Category.cs`).

#### Bước 3: Khởi tạo file `LibraryDbContext.cs`
Tạo file tại `src/Library.Infrastructure/Persistence/LibraryDbContext.cs`:

```csharp
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

        // EF Core tự động nhận diện toàn bộ Data Annotations ([Table], [Key], [Column], [ForeignKey]...) trên các Entity.
        // Tại đây chỉ cần cấu hình thêm các ràng buộc đặc thù mà Data Annotation chưa hỗ trợ (ví dụ: Index duy nhất cho ISBN):
        modelBuilder.Entity<Book>()
            .HasIndex(b => b.Isbn)
            .IsUnique();
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
# 1. Tạo file migration ghi nhận thay đổi từ code C#
dotnet ef migrations add InitialLibrarySchema \
  --project src/Library.Infrastructure \
  --startup-project src/Library.Api \
  --output-dir Persistence/Migrations

# 2. Đẩy cấu trúc bảng vào PostgreSQL thật (xem chi tiết mục 7.3)
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

### 7.3. Chuyên Sâu Về Lệnh `dotnet ef database update`

> 🎯 **Mục tiêu:** Hiểu rõ bản chất ngầm bên dưới của lệnh, lý do tách biệt `--project` và `--startup-project`, cách đọc hiểu log và các thao tác quản trị nâng cao (Rollback, Script).

#### 1. Bóc tách cú pháp câu lệnh

```bash
dotnet ef database update \
  --project src/Library.Infrastructure/ \
  --startup-project src/Library.Api/
```

$$\underbrace{\text{dotnet ef database update}}_{\text{Hành động chính}} \quad \underbrace{\text{--project src/Library.Infrastructure/}}_{\text{Nơi chứa code Database}} \quad \underbrace{\text{--startup-project src/Library.Api/}}_{\text{Nơi chứa cấu hình chạy}}$$

| Thành phần | Vai trò | Tại sao cần thiết trong Clean Architecture? |
| :--- | :--- | :--- |
| **`dotnet ef database update`** | Lệnh chính | Yêu cầu EF Core đồng bộ các file migration C# vào database thật. |
| **`--project src/Library.Infrastructure/`** | Target Project (Nơi chứa code DB) | Chỉ định project chứa file `LibraryDbContext.cs` và thư mục `Migrations/`. EF Core cần tìm vào đây để đọc code migration. |
| **`--startup-project src/Library.Api/`** | Startup Project (Nơi chứa cấu hình) | `Infrastructure` chỉ là thư viện DLL không tự chạy được và không có chuỗi kết nối. EF Core cần mượn `Program.cs` và `appsettings.Development.json` từ `Api` để biết Host, Port, User, Password kết nối vào PostgreSQL. |

#### 2. Thao tác chính diễn ra bên dưới (Under the Hood)

Khi bạn gõ lệnh, EF Core âm thầm thực hiện tuần tự **5 bước**:

```text
[ File C# Migration ]               [ Bảng __EFMigrationsHistory ]
(Trong Library.Infrastructure)         (Trong Database PostgreSQL)
         │                                       │
         └─────────────► 1. SO SÁNH ◄────────────┘
                         (Tìm file migration nào CHƯA ĐƯỢC CHẠY)
                                │
                                ▼
                         2. DỊCH SANG SQL
                         (Dịch hàm Up() trong C# thành CREATE TABLE, INDEX...)
                                │
                                ▼
                         3. THỰC THI TRANSACTION
                         (Chạy SQL vào PostgreSQL để tạo bảng thật)
                                │
                                ▼
                         4. GHI NHẬN LỊCH SỬ
                         (Chèn tên migration vừa chạy vào __EFMigrationsHistory)
```

1. **Kiểm tra lịch sử:** EF Core kết nối vào PostgreSQL và truy vấn bảng `__EFMigrationsHistory`.
2. **So sánh (Diff):** So khớp các file migration trong code với các bản ghi trong DB để tìm những migration **chưa được áp dụng**.
3. **Dịch C# sang SQL DDL:** Mở hàm `Up()` của các file migration chưa chạy, tự động dịch thành câu lệnh SQL chuẩn PostgreSQL (`CREATE TABLE`, `CREATE INDEX`, `ALTER TABLE`...).
4. **Thực thi Transaction:** Mở một Database Transaction và chạy các câu lệnh SQL. Nếu xảy ra lỗi ở bất kỳ bảng nào, toàn bộ sẽ được rollback ngay lập tức để không làm hỏng database.
5. **Đánh dấu hoàn tất:** Chèn một bản ghi mới (gồm `MigrationId` và `ProductVersion`) vào bảng `__EFMigrationsHistory`.

#### 3. Giải thích log `fail: ... FROM "__EFMigrationsHistory"` khi chạy lần đầu
Khi chạy trên database mới tinh, bạn sẽ thấy thông báo:
```text
fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
      Failed executing DbCommand (20ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "MigrationId", "ProductVersion"
      FROM "__EFMigrationsHistory"
      ORDER BY "MigrationId";
```
> [!NOTE]
> Đây là **hành vi hoàn toàn bình thường**. Vì database mới chưa có bảng lịch sử `__EFMigrationsHistory`, lệnh `SELECT` thăm dò sẽ trả về lỗi "bảng chưa tồn tại". EF Core nhận diện điều này và sẽ tự động tạo bảng này trước khi chạy migration. Khi thấy dòng **`Done.`** ở cuối nghĩa là toàn bộ bảng đã tạo thành công!

#### 4. Các lệnh mở rộng hữu ích trong thực tế

```bash
# 1. Rollback về một migration cũ cụ thể (hạ phiên bản DB):
dotnet ef database update <TenMigrationCu> \
  --project src/Library.Infrastructure \
  --startup-project src/Library.Api

# 2. Xóa sạch tất cả các bảng (Rollback về trạng thái ban đầu, tương đương Drop schema):
dotnet ef database update 0 \
  --project src/Library.Infrastructure \
  --startup-project src/Library.Api

# 3. Xuất toàn bộ câu lệnh SQL ra file để kiểm tra trước khi chạy:
dotnet ef migrations script \
  --project src/Library.Infrastructure \
  --startup-project src/Library.Api \
  --output script.sql
```

#### 5. So sánh với hệ sinh thái Spring Boot

| Khái niệm | Spring Boot (Flyway / Liquibase) | .NET (Entity Framework Core) |
| :--- | :--- | :--- |
| **Bảng lưu lịch sử migration** | `flyway_schema_history` | `__EFMigrationsHistory` |
| **File migration** | `V1__Initial_Setup.sql` | `20261001154949_InitialLibrarySchema.cs` |
| **Nơi cung cấp chuỗi kết nối** | `application.yml` / `application.properties` | `--startup-project src/Library.Api` (`appsettings.Development.json`) |
| **Thực thi migration** | Tự động chạy khi Spring Boot khởi động | Chạy qua lệnh `dotnet ef database update` |

## 8. Toàn Tập Luồng Hoạt Động CRUD Đầu Sách (Book API - Clean Architecture)

> 🎯 **Mục tiêu:** Nắm vững trọn vẹn luồng xử lý của một API hoàn chỉnh từ khi Client gửi request qua Middleware → Controller → Service → Repository → Database và ngược lại, cách tổ chức DTOs, Interface và xử lý lỗi tập trung.

### 8.1. Sơ Đồ Luồng Hoạt Động Tổng Thể (Runtime Flow)

Khi Client gửi một request (ví dụ: tạo mới sách), luồng dữ liệu sẽ đi qua các tầng như sau:

$$\text{Client} \xrightarrow{\text{1}} \text{Middleware} \xrightarrow{\text{2}} \text{BooksController} \xrightarrow{\text{3}} \text{BookService} \xrightarrow{\text{4}} \text{BookRepository} \xrightarrow{\text{5}} \text{PostgreSQL}$$

```mermaid
flowchart TD
    Client(["Client (Postman / Swagger / Frontend)"])

    subgraph ApiLayer ["Tầng Presentation (Library.Api)"]
        MW["ExceptionHandlingMiddleware\n(Bắt lỗi tập trung 400, 404, 500)"]
        Ctrl["BooksController\n(Đón nhận HTTP Request & trả HTTP Status)"]
    end

    subgraph AppLayer ["Tầng Application (Library.Application)"]
        IBookSvc["IBookService"]
        BookSvc["BookService\n(Xử lý nghiệp vụ, kiểm tra CategoryId, Map DTO)"]
        DTOs["DTOs (CreateBookRequest, BookResponse...)"]
    end

    subgraph InfraLayer ["Tầng Infrastructure (Library.Infrastructure)"]
        IBookRepo["IBookRepository"]
        BookRepo["BookRepository\n(Thực thi LINQ, Include Category, SaveChanges)"]
        DbContext["LibraryDbContext"]
    end

    subgraph DBLayer ["Cơ Sở Dữ Liệu"]
        Postgres[("PostgreSQL Database\n(Bảng Books & Categories)")]
    end

    Client -->|1. HTTP Request| MW
    MW -->|2. Forward Request| Ctrl
    Ctrl -->|3. Gọi Service| IBookSvc
    IBookSvc -.-> BookSvc
    BookSvc -.-> DTOs
    BookSvc -->|4. Kiểm tra & Gọi Repo| IBookRepo
    IBookRepo -.-> BookRepo
    BookRepo -->|5. EF Core LINQ| DbContext
    DbContext -->|6. SQL DDL / DML| Postgres

    Postgres -->|7. Dữ liệu thô| DbContext
    DbContext -->|8. Entity Book| BookRepo
    BookRepo -->|9. Entity Book| BookSvc
    BookSvc -->|10. BookResponse DTO| Ctrl
    Ctrl -->|11. HTTP Response (200, 201, 204)| Client

    BookSvc -.->|Ném lỗi NotFoundException| MW
    MW -.->|JSON chuẩn { status: 404, message }| Client
```

### 8.2. Danh Sách Các File & Trách Nhiệm Từng Tầng Trong Luồng

#### A. Tầng `Library.Application` (Cốt lõi logic nghiệp vụ)
1. **[NotFoundException.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Application/Exceptions/NotFoundException.cs):** Exception nghiệp vụ ném ra khi không tìm thấy sách hoặc thể loại (`Category`).
2. **[CreateBookRequest.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Application/DTOs/Books/CreateBookRequest.cs):** DTO nhận dữ liệu tạo sách, có validation Data Annotations (`[Required]`, `[MaxLength]`, `[Range]`).
3. **[UpdateBookRequest.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Application/DTOs/Books/UpdateBookRequest.cs):** DTO nhận dữ liệu sửa sách.
4. **[BookResponse.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Application/DTOs/Books/BookResponse.cs):** DTO trả về cho client kèm tên thể loại (`CategoryName`).
5. **[IBookRepository.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Application/Interfaces/IBookRepository.cs):** Hợp đồng truy cập dữ liệu (Abstraction).
6. **[IBookService.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Application/Interfaces/IBookService.cs):** Hợp đồng use case của ứng dụng.
7. **[BookService.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Application/Services/BookService.cs):** Triển khai toàn bộ logic nghiệp vụ (kiểm tra `CategoryId` tồn tại, map DTO $\leftrightarrow$ Entity).

#### B. Tầng `Library.Infrastructure` (Kết nối cơ sở dữ liệu)
1. **[BookRepository.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Infrastructure/Repositories/BookRepository.cs):** Triển khai `IBookRepository` bằng EF Core LINQ (`_context.Books.Include(b => b.Category).AsNoTracking()`).
2. **[LibraryDbContext.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Infrastructure/Persistence/LibraryDbContext.cs):** Thêm **Data Seeding** cho 3 thể loại mặc định (`Id: 1 - Công nghệ thông tin`, `Id: 2 - Kinh tế & Quản trị`, `Id: 3 - Văn học & Nghệ thuật`).
3. **Migration [SeedCategories](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Infrastructure/Persistence/Migrations/20261001182939_SeedCategories.cs):** Nạp dữ liệu mẫu vào PostgreSQL.

#### C. Tầng `Library.Api` (Giao tiếp HTTP & Entry Point)
1. **[BooksController.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Controllers/BooksController.cs):** Endpoint `/api/v1/books` tiếp nhận HTTP request, trả về status code chuẩn RESTful.
2. **[ExceptionHandlingMiddleware.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Middleware/ExceptionHandlingMiddleware.cs):** Bắt lỗi tập trung, trả về JSON `{ status: 404/400/500, message: "..." }`.
3. **[Program.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Program.cs):** Đăng ký Dependency Injection (`IBookRepository`, `IBookService`) và gắn Middleware vào pipeline.
4. **[books.http](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/books.http):** File kịch bản kiểm thử 7 request nhanh ngay trên VS Code / IDE.

---

### 8.3. Bảng Tổng Hợp Các Endpoint Trong `BooksController`

| Thao tác | HTTP Method | Route URL | Mã HTTP Thành Công | Mã HTTP Thất Bại | Ý nghĩa |
| :--- | :--- | :--- | :---: | :---: | :--- |
| **Lấy danh sách** | `GET` | `/api/v1/books` | `200 OK` | `500` | Lấy tất cả sách kèm tên thể loại. |
| **Lấy chi tiết** | `GET` | `/api/v1/books/{id}` | `200 OK` | `404 Not Found` | Lấy 1 cuốn theo ID, nếu không có trả về 404. |
| **Tạo mới** | `POST` | `/api/v1/books` | `201 Created` | `400 Bad Request` / `404` | Tạo sách mới, trả về header `Location: /api/v1/books/{id}`. |
| **Cập nhật** | `PUT` | `/api/v1/books/{id}` | `200 OK` | `400 Bad Request` / `404` | Cập nhật thông tin toàn bộ cuốn sách. |
| **Xóa sách** | `DELETE` | `/api/v1/books/{id}` | `204 No Content` | `404 Not Found` | Xóa sách khỏi hệ thống, không cần trả về body. |

---

### 8.4. Quy Tắc "Vàng" Khi Triển Khai Module Tiếp Theo (Quy Trình Chuẩn)

Khi bạn muốn code thêm bất kỳ tính năng nào khác (ví dụ: Quản lý Thể loại, Mượn trả, Người dùng), **hãy lặp lại đúng 7 bước này**:

```text
Bước 1 (Domain):         Tạo Entity & Enum
  ↓
Bước 2 (Application):    Tạo DTOs (Request / Response)
  ↓
Bước 3 (Application):    Tạo IRepository & IService
  ↓
Bước 4 (Application):    Viết logic trong Service
  ↓
Bước 5 (Infrastructure): Viết truy vấn DB trong Repository
  ↓
Bước 6 (Api):            Tạo Controller kế thừa ControllerBase
  ↓
Bước 7 (Api):            Đăng ký AddScoped trong Program.cs & Test
```

---

## 9. Chuyên Đề Xác Thực & Phân Quyền: Toàn Tập Về Auth & JWT (JSON Web Token)

> 🎯 **Mục tiêu:** Nắm vững bản chất cơ chế xác thực không trạng thái (Stateless Authentication) với JWT, thuật toán băm mật khẩu an toàn BCrypt, quy trình chi tiết từng bước khi sinh token (Token Generation) và khi thẩm định token (Token Validation / Verification) trong mô hình Clean Architecture của ASP.NET Core.

---

### 9.1. Tổng Quan Về Kiến Trúc Auth Trong Hệ Thống (Clean Architecture)

Trong hệ thống thư viện LightBrary, nghiệp vụ xác thực (Authentication) tuân thủ nghiêm ngặt nguyên lý đảo ngược phụ thuộc (Dependency Inversion) của Clean Architecture:

```text
src/
├── Library.Domain/
│   └── Entities/
│       ├── User.cs               # Thực thể người dùng (Id, Email, FullName, PasswordHash...)
│       ├── Role.cs               # Vai trò hệ thống (Admin, Librarian, Member...)
│       └── UserRole.cs           # Bảng liên kết nhiều-nhiều User <-> Role
│
├── Library.Application/
│   ├── Interfaces/
│   │   ├── IAuthService.cs       # Hợp đồng Use Case: RegisterAsync, LoginAsync
│   │   ├── IUserRepository.cs    # Hợp đồng truy xuất User từ DB
│   │   └── Authentication/
│   │       ├── IPasswordHasher.cs    # Hợp đồng băm & kiểm tra mật khẩu
│   │       └── IJwtTokenService.cs   # Hợp đồng sinh chuỗi JWT Token
│   ├── DTOs/Auth/
│   │   ├── RegisterRequest.cs    # DTO đăng ký độc giả
│   │   ├── LoginRequest.cs       # DTO đăng nhập (Email + Password)
│   │   └── AuthResponse.cs       # Trả về Token + User Info + Expiration
│   └── Services/
│       └── AuthService.cs        # Triển khai logic nghiệp vụ điều phối
│
├── Library.Infrastructure/
│   ├── Authentication/
│   │   ├── PasswordHasher.cs     # Triển khai bằng thư viện BCrypt.Net-Next
│   │   └── JwtTokenService.cs    # Triển khai bằng System.IdentityModel.Tokens.Jwt
│   └── Repositories/
│       └── UserRepository.cs     # Thao tác DbSet<User> qua EF Core
│
└── Library.Api/
    ├── Controllers/
    │   └── AuthController.cs     # Endpoint POST /api/v1/auth/register & /login
    └── appsettings.json          # Cấu hình SecretKey, Issuer, Audience, Expiration
```

#### Phân chia vai trò rõ ràng:
- **Tầng Application:** Định nghĩa *hợp đồng* (`IPasswordHasher`, `IJwtTokenService`) mà **không phụ thuộc** vào bất kỳ thư viện bên ngoài nào.
- **Tầng Infrastructure:** Nắm giữ chi tiết kỹ thuật (dùng thuật toán BCrypt gì, dùng thư viện JWT nào). Sau này nếu muốn đổi từ BCrypt sang Argon2, hoặc đổi từ JWT đối xứng sang RSA bất đối xứng, ta chỉ việc viết class mới trong Infrastructure mà không làm thay đổi một dòng code nghiệp vụ nào ở Application!

---

### 9.2. Cơ Chế Băm Mật Khẩu An Toàn Với BCrypt

> [!CAUTION]
> **Quy tắc bất di bất dịch:** Tuyệt đối **không bao giờ lưu mật khẩu dạng văn bản thô (Plain Text)** hoặc dùng các hàm băm cũ đã bị phá vỡ như MD5, SHA-1 vào cơ sở dữ liệu.

#### 1. Bản chất của BCrypt
BCrypt là hàm băm mật khẩu một chiều (One-Way Hashing) có các đặc tính vượt trội:
- **Tự động sinh Muối (Salt):** Mỗi lần băm cùng một mật khẩu `"123456"`, BCrypt sẽ tự động sinh ra một chuỗi Salt ngẫu nhiên khác nhau. Nhờ đó, hai người dùng đặt cùng một mật khẩu sẽ có hai chuỗi hash hoàn toàn khác nhau trong DB $\rightarrow$ vô hiệu hóa kỹ thuật tấn công bằng bảng tra cứu trước (Rainbow Table).
- **Hệ số làm chậm (Work Factor / Cost Factor):** Dự án sử dụng `workFactor: 12` ($2^{12} = 4096$ vòng tính toán lặp). Việc này cố tình làm phép toán mất khoảng 200–300ms, khiến hacker không thể dùng GPU/máy đào để dò vét cạn (Brute-force) hàng triệu mật khẩu mỗi giây.

#### 2. So sánh quá trình Hash vs Verify

| Thao tác | Hàm gọi | Mục đích | Kết quả |
| :--- | :--- | :--- | :--- |
| **Băm khi Đăng ký** | `BCrypt.HashPassword(password, 12)` | Tạo chuỗi băm kèm salt để lưu vào cột `PasswordHash` | Ví dụ: `$2a$12$e8k...Z8u` |
| **Đối soát khi Đăng nhập**| `BCrypt.Verify(rawPassword, hashFromDb)` | Tách Salt từ hash cũ trong DB, băm mật khẩu người dùng vừa nhập rồi so sánh | `true` (khớp) hoặc `false` |

---

### 9.3. Cấu Trúc Bản Chất Của Một JWT (JSON Web Token)

JWT là một chuẩn mở (RFC 7519) dùng để truyền tải thông tin an toàn giữa client và server dưới dạng một chuỗi JSON được ký điện tử (Digitally Signed).

Một chuỗi JWT hoàn chỉnh gồm **3 phần** ngăn cách nhau bởi dấu chấm (`.`):

$$\mathbf{JWT} = \underbrace{\text{eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9}}_{\text{1. Header}} \;.\; \underbrace{\text{eyJzdWIiOiIxMjM0NTYiLCJuYW1lIjoiQW5oIFZ1In0}}_{\text{2. Payload (Claims)}} \;.\; \underbrace{\text{SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c}}_{\text{3. Signature}}$$

```text
┌────────────────────────────────────────────────────────────────────────┐
│ 1. HEADER (Base64Url Encoded)                                          │
│    { "alg": "HS256", "typ": "JWT" }                                    │
│    -> Khai báo thuật toán mã hóa (HMAC-SHA256) và loại token           │
├────────────────────────────────────────────────────────────────────────┤
│ 2. PAYLOAD / CLAIMS (Base64Url Encoded)                                │
│    {                                                                   │
│      "sub": "u-1234",          // Subject: Khóa chính UserId          │
│      "email": "user@gmail.com",// Email người dùng                     │
│      "name": "Nguyễn Văn A",   // Họ tên người dùng                    │
│      "jti": "guid-token-id",   // JWT ID: Mã duy nhất của token        │
│      "exp": 1759363200,        // Hạn hết hạn (Unix timestamp)         │
│      "iss": "LightBraryAPI",   // Bên phát hành token (Issuer)         │
│      "aud": "LightBraryClient" // Đối tượng sử dụng token (Audience)   │
│    }                                                                   │
├────────────────────────────────────────────────────────────────────────┤
│ 3. SIGNATURE (Chữ ký số chống giả mạo)                                 │
│    HMACSHA256(                                                         │
│      base64UrlEncode(Header) + "." + base64UrlEncode(Payload),         │
│      SecretKey // Chỉ có Server nắm giữ bí mật này!                    │
│    )                                                                   │
└────────────────────────────────────────────────────────────────────────┘
```

> [!WARNING]
> **Payload chỉ được Encode Base64 chứ KHÔNG MÃ HÓA:** Bất kỳ ai có token đều có thể decode tại [jwt.io](https://jwt.io) để đọc toàn bộ dữ liệu trong Payload. Do đó, **tuyệt đối không để mật khẩu, số căn cước hoặc dữ liệu nhạy cảm vào Claims của JWT!**
> Chữ ký (`Signature`) đảm bảo rằng nếu hacker cố tình sửa nội dung Payload (ví dụ sửa `sub` sang UserId khác), chữ ký tính lại bằng `SecretKey` của server sẽ không khớp và token lập tức bị từ chối.

---

### 9.4. Quá Trình Tạo Token (Token Generation Flow)

Quá trình này diễn ra khi người dùng gọi API **Đăng nhập (`/api/v1/auth/login`)** hoặc **Đăng ký (`/api/v1/auth/register`)**:

```mermaid
sequenceDiagram
    autonumber
    actor Client as Client (Frontend / Swagger)
    participant Ctrl as AuthController
    participant Svc as AuthService
    participant Repo as UserRepository
    participant Hasher as PasswordHasher (BCrypt)
    participant Jwt as JwtTokenService
    participant DB as PostgreSQL (AspNetUsers)

    Client->>Ctrl: POST /api/v1/auth/login (Email, Password)
    Ctrl->>Svc: LoginAsync(request)
    Svc->>Repo: GetByEmailAsync(email)
    Repo->>DB: SELECT * FROM AspNetUsers WHERE Email = ...
    DB-->>Repo: User Entity (kèm PasswordHash)
    Repo-->>Svc: User Entity

    alt User không tồn tại hoặc sai Email
        Svc-->>Ctrl: throw UnauthorizedException("Email hoặc mật khẩu không đúng")
        Ctrl-->>Client: 401 Unauthorized
    end

    Svc->>Hasher: Verify(rawPassword, user.PasswordHash)
    alt Mật khẩu không khớp
        Hasher-->>Svc: false
        Svc-->>Ctrl: throw UnauthorizedException("Email hoặc mật khẩu không đúng")
        Ctrl-->>Client: 401 Unauthorized
    else Mật khẩu chính xác
        Hasher-->>Svc: true
    end

    Note over Svc,Jwt: Mật khẩu hợp lệ -> Bắt đầu sinh JWT Token
    Svc->>Jwt: GenerateToken(user)
    
    rect rgb(240, 248, 255)
        Note over Jwt: Các bước xử lý bên trong JwtTokenService:
        Note over Jwt: 1. Đọc Jwt:SecretKey từ appsettings.json
        Note over Jwt: 2. Tạo SymmetricSecurityKey(bytes) & SigningCredentials (HMAC-SHA256)
        Note over Jwt: 3. Khởi tạo danh sách Claims (sub: Id, email, name: FullName, jti)
        Note over Jwt: 4. Tạo JwtSecurityToken(issuer, audience, claims, expires, credentials)
        Note over Jwt: 5. Dùng JwtSecurityTokenHandler().WriteToken() -> Chuỗi JWT thô
    end

    Jwt-->>Svc: Chuỗi JWT string
    Svc->>Jwt: GetExpiration()
    Jwt-->>Svc: Thời gian hết hạn (DateTime)
    Svc-->>Ctrl: AuthResponse (Token, UserId, FullName, Email, ExpiresAt)
    Ctrl-->>Client: 200 OK + JSON Token
```

#### Phân tích chi tiết code tạo token trong [JwtTokenService.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Infrastructure/Authentication/JwtTokenService.cs):
```csharp
public string GenerateToken(User user)
{
    // 1. Lấy khóa bí mật từ cấu hình (tối thiểu 32 ký tự / 256 bits)
    var secretKey = _configuration["Jwt:SecretKey"]!;
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    
    // 2. Chỉ định thuật toán ký đối xứng: HMAC-SHA256
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // 3. Đóng gói thông tin danh tính (Claims) vào Payload
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),              // Khóa chính User
        new(JwtRegisteredClaimNames.Email, user.Email),          // Email độc giả
        new(JwtRegisteredClaimNames.Name, user.FullName),        // Họ tên đầy đủ
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID ngẫu nhiên của token
    };

    var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

    // 4. Lắp ráp các thành phần lại thành token hoàn chỉnh
    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],       // Ai cấp token?
        audience: _configuration["Jwt:Audience"],   // Dành cho ai dùng?
        claims: claims,                             // Thông tin Payload
        expires: DateTime.UtcNow.AddMinutes(expirationMinutes), // Thời hạn sống
        signingCredentials: credentials             // Chữ ký bảo mật
    );

    // 5. Serialize thành chuỗi string dạng: Header.Payload.Signature
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

---

### 9.5. Quá Trình Xác Thực Token (Token Verification / Authentication Flow)

Khi Client đã có token, họ sẽ gửi kèm token vào mỗi request gọi đến các API cần bảo vệ (như tạo phiếu mượn, xem lịch sử, trả sách) qua HTTP Header chuẩn:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Dưới đây là sơ đồ chi tiết **cơ chế chặn và xác thực tự động** của ASP.NET Core Middleware trước khi request kịp chạm đến Controller:

```mermaid
flowchart TD
    Client(["Client gửi Request\nHeader: Authorization: Bearer <Token>"])
    
    subgraph Pipeline ["ASP.NET Core HTTP Request Pipeline"]
        ExMW["ExceptionHandlingMiddleware"]
        AuthMW["AuthenticationMiddleware\n(UseAuthentication)"]
        JwtHandler["JwtSecurityTokenHandler / JwtBearerOptions"]
        AuthzMW["AuthorizationMiddleware\n(UseAuthorization)"]
        Controller["BorrowRecordsController\nAction Method"]
    end

    Client --> ExMW
    ExMW --> AuthMW
    AuthMW -->|Trích xuất chuỗi Token| JwtHandler

    %% Các bước kiểm tra tính hợp lệ của Token
    subgraph Checks ["5 Bước Thẩm Định Chữ Ký & Dữ Liệu"]
        C1{"1. Format đúng 3 phần\nvà tiền tố 'Bearer'?"}
        C2{"2. Chữ ký hợp lệ?\n(Signature == HMAC(H+P, SecretKey))"}
        C3{"3. Token còn hạn?\n(exp > DateTime.UtcNow)"}
        C4{"4. Đúng Issuer\nvà Audience?"}
        C5["5. Giải mã Payload Claims\n-> Tạo ClaimsIdentity\n-> Gán vào HttpContext.User"]
    end

    JwtHandler --> C1
    C1 -- Không --> Fail401["Xác thực thất bại\n(AuthenticateResult.Fail)"]
    C1 -- Đúng --> C2
    C2 -- Bị sửa đổi/Sai Key --> Fail401
    C2 -- Đúng Key --> C3
    C3 -- Hết hạn --> Fail401
    C3 -- Còn hạn --> C4
    C4 -- Sai --> Fail401
    C4 -- Đúng --> C5

    C5 --> AuthzMW
    Fail401 --> AuthzMW

    AuthzMW --> IsAuth{"Endpoint có yêu cầu [Authorize]\nhoặc quyền hạn gì không?"}
    IsAuth -- "Không hợp lệ / Chưa login" --> Resp401(["Trả về 401 Unauthorized\n(hoặc 403 Forbidden nếu thiếu Role)"])
    IsAuth -- "Hợp lệ" --> Controller

    Controller --> Resp200(["Xử lý logic thành công\nTrả về 200 OK / 201 Created"])
```

#### 5 bước kiểm tra ngầm định kỳ của JWT Bearer:
1. **Trích xuất chuỗi:** Lấy token từ header `Authorization`, cắt bỏ tiền tố `Bearer `.
2. **Kiểm tra chữ ký (Signature Verification):** Lấy Header và Payload nhận được, băm lại bằng `SecretKey` đã cấu hình trên server. So sánh kết quả tính ra với chữ ký đính kèm trong token. Nếu không khớp $\rightarrow$ Token đã bị kẻ xấu giả mạo nội dung $\rightarrow$ Bác bỏ ngay.
3. **Kiểm tra thời hạn (`exp` - Lifetime):** So khớp `exp` với giờ hiện tại của hệ thống (`DateTime.UtcNow`). Nếu token đã quá hạn $\rightarrow$ Bác bỏ.
4. **Kiểm tra bên cấp & bên nhận (`iss`, `aud`):** Đảm bảo token này do chính hệ sinh thái LightBrary phát hành chứ không phải token trôi nổi từ hệ thống khác.
5. **Khởi tạo danh tính (`ClaimsPrincipal`):** Nếu vượt qua tất cả bài kiểm tra, ASP.NET Core tự động phân rã các Claims (`sub`, `email`, `name`) thành đối tượng `ClaimsPrincipal` và gán vào thuộc tính:
   ```csharp
   HttpContext.User
   ```
   Nhờ vậy, ở bất kỳ Controller nào, bạn đều có thể dễ dàng lấy ID người đang gọi API bằng cách:
   ```csharp
   var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
   ```

---

### 9.6. Hướng Dẫn Kích Hoạt JWT Authentication Chuẩn Vào `Program.cs`

Để kích hoạt toàn bộ cơ chế bảo vệ tự động bằng JWT, ta cấu hình trong dự án:

#### 1. Cài đặt Package xác thực
```bash
dotnet add src/Library.Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

#### 2. Cấu hình khóa trong [appsettings.json](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/appsettings.json)
```json
"Jwt": {
  "Issuer": "LightBraryAPI",
  "Audience": "LightBraryClient",
  "SecretKey": "DayLaChuoiBiMatSieuDaiToiThieu32KyTuChoThuatToanHMACSHA256!",
  "ExpirationMinutes": "60"
}
```

#### 3. Đăng ký Middleware trong [Program.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Program.cs)
```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// 1. Đăng ký dịch vụ Authentication & quy định tham số thẩm định Token
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    // Đặt JWT Bearer làm chuẩn xác thực mặc định
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Không cho phép độ trễ thời gian sau khi hết hạn
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ...
// LƯU Ý THỨ TỰ: UseAuthentication PHẢI ĐỨNG TRƯỚC UseAuthorization!
app.UseAuthentication(); // 1. Bạn là ai? (Giải mã JWT)
app.UseAuthorization();  // 2. Bạn có quyền làm việc này không? (Kiểm tra Role/Policy)
// ...
```

#### 4. Sử dụng Attribute `[Authorize]` trên Controller
```csharp
[ApiController]
[Route("api/v1/borrow-records")]
[Authorize] // Bắt buộc phải đăng nhập và gửi Token hợp lệ mới được gọi API này!
public class BorrowRecordsController : ControllerBase
{
    // Chỉ Admin mới được thực hiện thao tác này
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) { ... }
}
```

---

### 9.7. Quy Trình Tự Động Khởi Tạo Admin Mặc Định Khi Chạy Lần Đầu (First-Run Database Seeding)

> 🎯 **Mục tiêu:** Hiểu rõ bài toán "con gà và quả trứng" khi khởi tạo hệ thống mới, cách kiểm tra trạng thái cơ sở dữ liệu khi ứng dụng vừa bật lên (Startup Check), quy trình băm mật khẩu và gán quyền tự động cho tài khoản Quản trị viên (Admin).

#### 1. Bài toán đặt ra
Khi triển khai dự án lần đầu tiên (hoặc vừa reset sạch cơ sở dữ liệu), hệ thống chưa có bất kỳ tài khoản nào trong bảng `AspNetUsers`. Nếu không có tài khoản thì không ai có quyền đăng nhập vào phân hệ quản trị hoặc mượn sách.

Do đó, ta cần một cơ chế **Seed dữ liệu tự động (Idempotent Initialization)**:
- Chỉ chạy và tạo tài khoản khi trong database **chưa có bất kỳ người dùng nào**.
- Nếu đã có ít nhất 1 người dùng (từ lần chạy thứ hai trở đi), hệ thống sẽ **bỏ qua hoàn toàn**, không tạo trùng lặp hay ghi đè dữ liệu cũ.

#### 2. Sơ đồ luồng hoạt động khi khởi động (Startup Flow)

```mermaid
sequenceDiagram
    autonumber
    participant App as Program.cs (ASP.NET Core Host)
    participant Scope as IServiceScope
    participant Seeder as DbInitializer (Infrastructure)
    participant Hasher as PasswordHasher (BCrypt)
    participant DB as PostgreSQL (LibraryDbContext)

    App->>App: Build WebApplication & Services
    App->>Scope: CreateScope() (Mở phạm vi dịch vụ tạm thời)
    Scope->>Seeder: DbInitializer.SeedAsync(context, passwordHasher, logger)
    
    Seeder->>DB: await context.Users.AnyAsync()
    DB-->>Seeder: Trả về kết quả (true/false)

    alt hasUsers == true (Đã có người dùng trong DB)
        Note over Seeder: Không làm gì cả, hệ thống đã hoạt động bình thường
    else hasUsers == false (Hệ thống mới tinh, chạy lần đầu!)
        Note over Seeder: 🚀 Bắt đầu quy trình cấp phát Admin mặc định
        
        Seeder->>DB: Kiểm tra Role 'Admin' trong AspNetRoles
        alt Chưa có Role Admin
            Seeder->>DB: context.Roles.Add(new Role { Name = "Admin" })
        end
        
        Seeder->>Hasher: Hash("Asdf1234!") với BCrypt (workFactor: 12)
        Hasher-->>Seeder: Chuỗi mật khẩu băm ($2a$12$...)
        
        Note over Seeder: Khởi tạo User với SĐT 0916114536
        Seeder->>DB: context.Users.Add(User: FullName, SĐT, Email, PasswordHash)
        Seeder->>DB: context.UserRoles.Add(UserRole: UserId <-> RoleId)
        Seeder->>DB: await context.SaveChangesAsync()
        
        Note over Seeder: Ghi log thông báo khởi tạo thành công ✅
    end

    Scope-->>App: Giải phóng scope
    App->>App: app.Run() (Lắng nghe HTTP Request trên port 5048)
```

#### 3. Chi tiết mã nguồn triển khai

##### A. Tầng Infrastructure — [DbInitializer.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Infrastructure/Persistence/DbInitializer.cs)
```csharp
public static class DbInitializer
{
    public static async Task SeedAsync(LibraryDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        try
        {
            // 1. Kiểm tra xem đã có bất kỳ User nào trong cơ sở dữ liệu chưa
            var hasUsers = await context.Users.AnyAsync();
            if (!hasUsers)
            {
                logger.LogInformation("🚀 Phát hiện hệ thống chạy lần đầu. Đang tạo tài khoản Admin mặc định...");

                // 2. Tạo Role Admin nếu chưa có
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    adminRole = new Role { Id = Guid.NewGuid().ToString(), Name = "Admin" };
                    await context.Roles.AddAsync(adminRole);
                }

                // 3. Tạo tài khoản Admin với SĐT 0916114536 và mật khẩu băm của Asdf1234!
                var adminUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FullName = "Quản trị viên (Admin)",
                    Email = "admin@lightbrary.vn",
                    PhoneNumber = "0916114536",
                    PasswordHash = passwordHasher.Hash("Asdf1234!"), // Băm bằng BCrypt
                    Address = "Hệ thống Thư viện LightBrary",
                    CreatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(adminUser);

                // 4. Gán quyền Admin vào bảng liên kết AspNetUserRoles
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });

                // 5. Lưu toàn bộ trong một Transaction duy nhất
                await context.SaveChangesAsync();
                logger.LogInformation("✅ Đã tạo thành công tài khoản Admin: SĐT '{PhoneNumber}', Email '{Email}', Mật khẩu 'Asdf1234!'.",
                    adminUser.PhoneNumber, adminUser.Email);
            }
        }
        catch (Exception ex)
        {
            // Bắt lỗi an toàn nếu DB Docker chưa bật hoặc chưa chạy migration
            logger.LogWarning(ex, "⚠️ Không thể kiểm tra/khởi tạo dữ liệu người dùng ban đầu. Có thể DB chưa sẵn sàng.");
        }
    }
}
```

##### B. Tầng Api — Gọi Seeder trong [Program.cs](file:///home/dww/Personal/Code/Csharp/light-brary/src/Library.Api/Program.cs)
Trong ASP.NET Core, các dịch vụ `DbContext` và `IPasswordHasher` được đăng ký với vòng đời **Scoped** (chỉ sống trong một phạm vi request). Do hàm `Main` / `Program.cs` chạy ở cấp độ Root Container, ta phải tạo một phạm vi tạm thời bằng `app.Services.CreateScope()`:

```csharp
// ─── Khởi tạo dữ liệu ban đầu (First Run / Seed Admin) ─────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<LibraryDbContext>();
    var passwordHasher = services.GetRequiredService<IPasswordHasher>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    await DbInitializer.SeedAsync(context, passwordHasher, logger);
}

app.Run(); // Bắt đầu nhận request
```

##### C. Hỗ trợ đăng nhập linh hoạt bằng cả Số Điện Thoại & Email
Để thuận tiện khi sử dụng tài khoản vừa tạo, endpoint `POST /api/v1/auth/login` được nâng cấp để độc giả hoặc Admin có thể đăng nhập bằng **Số điện thoại (`0916114536`)** hoặc **Email (`admin@lightbrary.vn`)**:

```csharp
// Trong AuthService.cs:
public async Task<AuthResponse> LoginAsync(LoginRequest request)
{
    var identifier = request.Email.Trim(); // Người dùng có thể nhập Email hoặc SĐT
    var user = await _userRepository.GetByEmailOrPhoneAsync(identifier)
               ?? await _userRepository.GetByEmailAsync(identifier.ToLower());

    if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        throw new UnauthorizedException("Email/Số điện thoại hoặc mật khẩu không đúng.");

    var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
    // ...
}
```

#### 4. Bảng thông tin tài khoản Admin mặc định

| Thuộc tính | Giá trị thiết lập | Ghi chú |
| :--- | :--- | :--- |
| **Họ và tên** | `Quản trị viên (Admin)` | Tên hiển thị trên hệ thống và giao diện |
| **Số điện thoại** | `0916114536` | Dùng để đăng nhập hoặc liên hệ |
| **Email** | `admin@lightbrary.vn` | Dùng để đăng nhập hoặc nhận thông báo |
| **Mật khẩu thô** | `Asdf1234!` | Chỉ nhập khi đăng nhập, không lưu vào DB |
| **Mật khẩu trong DB** | Chuỗi băm BCrypt (`$2a$12$...`) | Bảo mật chống trộm cắp dữ liệu |
| **Vai trò (Role)** | `Admin` | Có toàn quyền quản trị sách, thể loại, bản sao và độc giả |

---

## 10. Bảng Tổng Hợp Lệnh Thường Dùng (Cheatsheet)

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
| | Rollback về migration cũ | `dotnet ef database update <TenMigrationCu> --project src/Library.Infrastructure --startup-project src/Library.Api` |
| | Xóa sạch bảng (Reset về 0) | `dotnet ef database update 0 --project src/Library.Infrastructure --startup-project src/Library.Api` |
| | Xuất mã SQL ra file | `dotnet ef migrations script --project src/Library.Infrastructure --startup-project src/Library.Api --output script.sql` |
| **Kiểm thử** | Xem Swagger UI | `http://localhost:5048/swagger` |
| | Xem OpenAPI Schema JSON | `http://localhost:5048/openapi/v1.json` |