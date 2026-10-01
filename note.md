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
[Giai đoạn 5: Hiện Thực Tính Năng Đầu Cuối (Full-Flow Feature)]
  └── 8. Toàn Tập Luồng Hoạt Động CRUD Đầu Sách (Book API - Clean Architecture)
       ↓
[Tra cứu]
  └── 9. Bảng Tổng Hợp Lệnh Thường Dùng (Cheatsheet)
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

## 9. Bảng Tổng Hợp Lệnh Thường Dùng (Cheatsheet)

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