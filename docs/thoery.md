# Lý Thuyết Ôn Thi ASP.NET Core — Dự án light-brary

> Tài liệu này được viết dựa trên **code thực tế** của dự án light-brary (.NET 10).
> Mỗi phần lý thuyết đi kèm ví dụ lấy trực tiếp từ file trong dự án — không phải ví dụ chung chung.

---

## Mục lục

1. [Luồng xử lý một Request](#1-luồng-xử-lý-một-request)q
2. [Middleware Pipeline và Thứ tự](#2-middleware-pipeline-và-thứ-tự)
3. [Dependency Injection — Vòng đời Service](#3-dependency-injection--vòng-đời-service)
4. [Routing và Model Binding](#4-routing-và-model-binding)
5. [EF Core — DbContext, Include, N+1](#5-ef-core--dbcontext-include-n1)
6. [Authentication và Authorization (JWT + Policy)](#6-authentication-và-authorization-jwt--policy)
7. [Xử lý lỗi nhất quán — Middleware Exception](#7-xử-lý-lỗi-nhất-quán--middleware-exception)
8. [Cấu hình — appsettings, Secrets, Biến môi trường](#8-cấu-hình--appsettings-secrets-biến-môi-trường)
9. [Middleware vs Filter — Phân biệt](#9-middleware-vs-filter--phân-biệt)
10. [Câu hỏi tự kiểm và lời giải](#10-câu-hỏi-tự-kiểm-và-lời-giải)
11. [So sánh ASP.NET Core với các Framework khác](#11-so-sánh-aspnet-core-với-các-framework-khác)

---

## 1. Luồng xử lý một Request

### Lý thuyết

Khi client gửi `GET /api/books/some-guid`, request sẽ đi qua **Middleware Pipeline** trước khi chạm đến Controller. Pipeline hoạt động theo mô hình **ống nước hai chiều**: mỗi middleware có thể xử lý logic **trước** khi gọi `next()` và **sau** khi `next()` trả về.

### Luồng thực tế trong dự án (lấy `GET /api/books/{id}`)

```
Client
  │
  ▼
[1] ExceptionHandlingMiddleware  ← Bọc try/catch toàn bộ pipeline
  │
  ▼
[2] HttpsRedirection             ← Chuyển HTTP → HTTPS
  │
  ▼
[3] CorsMiddleware               ← Kiểm tra và xử lý CORS header
  │
  ▼
[4] JwtAuthenticationMiddleware  ← Đọc Bearer token, gán User vào HttpContext
  │
  ▼
[5] AuthorizationMiddleware      ← Kiểm tra [Authorize] / [Authorize(Policy="...")]
  │
  ▼
[6] BooksController.GetById()    ← Endpoint nhận request
  │
  ▼
[7] BookService   ← Xử lý nghiệp vụ
  │
  ▼
[8] BookRepository ← Truy vấn EF Core
  │
  ▼
[9] AppDbContext → PostgreSQL     ← Câu lệnh SQL thực sự chạy
  │
  ◄─────────── Kết quả trả ngược lại qua từng tầng ───────────
  │
  ▼
Client nhận JSON Response
```

**Đoạn code trong `Program.cs` thể hiện thứ tự pipeline:**

```csharp
app.UseExceptionHandlingMiddleware(); // [1] Bọc ngoài cùng để bắt mọi lỗi
app.UseHttpsRedirection();            // [2]
app.UseCors("AllowFrontend");         // [3]
app.UseJwtAuthenticationMiddleware(); // [4] Xác thực (Authentication)
app.UseAuthorization();              // [5] Phân quyền (Authorization)
app.MapControllers();                // [6] Định tuyến đến Controller
```

---

## 2. Middleware Pipeline và Thứ tự

### Lý thuyết

Middleware là một thành phần nằm trong pipeline HTTP, mỗi cái có thể:

- Xử lý logic **trước** `await _next(context)` (đi vào)
- Gọi middleware tiếp theo bằng `await _next(context)`
- Xử lý logic **sau** `await _next(context)` (đi ra)

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // --- TRƯỚC next() ---
    // Logic chạy trước khi request đi sâu hơn vào pipeline

    await _next(context); // Chuyển sang middleware tiếp theo

    // --- SAU next() ---
    // Logic chạy sau khi response đã được tạo từ tầng dưới
}
```

### Câu hỏi quan trọng: **Đảo thứ tự Authentication và Authorization thì sao?**

Dự án đang đặt đúng thứ tự:

```csharp
app.UseJwtAuthenticationMiddleware(); // [4] — Xác thực: "Bạn là ai?"
app.UseAuthorization();              // [5] — Phân quyền: "Bạn có quyền làm điều này không?"
```

**Nếu đảo ngược** (`UseAuthorization` lên trước `UseJwtAuthenticationMiddleware`):

- `UseAuthorization` chạy trước nhưng `HttpContext.User` vẫn đang **rỗng** (chưa được nạp từ JWT).
- `User.Identity.IsAuthenticated` = `false` → Authorization luôn từ chối request với `401 Unauthorized`.
- **Hệ quả**: Mọi endpoint có `[Authorize]` đều sẽ bị chặn kể cả khi client gửi token hợp lệ.

### Middleware `ExceptionHandlingMiddleware` trong dự án

File: `back_end/light_brary/code/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
public async Task InvokeAsync(HttpCo`ntext context)
{
    try
    {
        await _next(context); // Gọi toàn bộ pipeline bên trong
    }
    catch (Exception ex)
    {
        // Nếu BẤT KỲ middleware hoặc controller nào bên dưới throw exception
        // → middleware này bắt lại và trả JSON lỗi nhất quán
        _logger.LogError(ex, "Đã xảy ra ngoại lệ: {Message}", ex.Message);
        await HandleExceptionAsync(context, ex);
    }
}
```

Đây là lý do middleware Exception phải đặt **đầu tiên** — để bọc được toàn bộ các middleware khác.

---

## 3. Dependency Injection — Vòng đời Service

### Lý thuyết: 3 loại Lifetime

| Lifetime            | Đăng ký           | Tạo instance khi nào               | Bao nhiêu instance                                          |
| :------------------ | :------------------- | :----------------------------------- | :----------------------------------------------------------- |
| **Singleton** | `AddSingleton<>()` | Lần đầu tiên được yêu cầu   | **1 instance duy nhất** suốt vòng đời ứng dụng  |
| **Scoped**    | `AddScoped<>()`    | Mỗi HTTP Request                    | **1 instance / request**, chia sẻ trong cùng request |
| **Transient** | `AddTransient<>()` | Mỗi lần được yêu cầu (inject) | **1 instance mới mỗi lần inject**                   |

### Thực tế trong dự án (`Program.cs`)

```csharp
// Tất cả Service và Repository đều là Scoped — đúng cho Web API
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddDbContext<AppDbContext>(...); // DbContext cũng mặc định là Scoped
```

**Tại sao chọn `Scoped` cho Service và Repository?**

- `AppDbContext` của EF Core **bắt buộc phải là Scoped** (không phải Singleton) vì nó theo dõi thay đổi entity (Change Tracking). Nếu là Singleton, dữ liệu từ request trước có thể "rò rỉ" sang request sau.
- Service và Repository được inject `AppDbContext` → chúng cũng phải là Scoped hoặc Transient để vòng đời không dài hơn `AppDbContext`.

### Hậu quả khi đăng ký sai: **Captive Dependency (Service Scoped bị Singleton giữ)**

```csharp
// SAI: Singleton giữ một Scoped service
builder.Services.AddSingleton<IMyService, MyService>(); // MyService inject AppDbContext (Scoped)
```

**Hậu quả:**

1. Lần đầu tiên `IMyService` được inject, ASP.NET Core tạo một instance `MyService` và giữ mãi mãi.
2. Instance `MyService` này đang giữ một `AppDbContext` từ request **đầu tiên** (đã bị disposed sau request đó).
3. Từ request thứ 2 trở đi: `AppDbContext` đã chết → mọi truy vấn DB đều throw `ObjectDisposedException`.
4. ASP.NET Core sẽ phát hiện và throw lỗi ngay lúc khởi động trong môi trường `Development`.

### DbContext sau mỗi Request

> **Điều gì xảy ra với `DbContext` sau mỗi request?**

Vì `AppDbContext` được đăng ký là `Scoped`, sau khi HTTP request kết thúc:

1. DI Container **gọi `Dispose()`** trên `AppDbContext`.
2. Toàn bộ dữ liệu đang được theo dõi (Change Tracking) bị xóa sạch.
3. Kết nối Database được trả về **connection pool** (không đóng cứng).
4. Request tiếp theo sẽ nhận một `AppDbContext` **hoàn toàn mới**.

---

## 4. Routing và Model Binding

### Attribute Routing trong dự án

File: `back_end/light_brary/code/Controllers/BooksController.cs`

```csharp
[ApiController]
[Route("api/[controller]")] // → "api/books" (tự lấy tên class bỏ chữ "Controller")
public class BooksController : ControllerBase
{
    [HttpGet]                        // GET  /api/books
    [HttpGet("{id:guid}")]           // GET  /api/books/{id}  — ràng buộc kiểu guid
    [HttpPost]                       // POST /api/books
    [HttpPut("{id:guid}")]           // PUT  /api/books/{id}
    [HttpDelete("{id:guid}")]        // DELETE /api/books/{id}
}
```

### Các nguồn Model Binding

| Attribute        | Lấy dữ liệu từ đâu             | Ví dụ thực tế trong dự án                   |
| :--------------- | :----------------------------------- | :------------------------------------------------ |
| `[FromRoute]`  | Đường dẫn URL`/api/books/{id}` | `Guid id` trong `GetById(Guid id)`            |
| `[FromQuery]`  | Query string`?keyword=abc`         | `string? keyword` trong `GetAll(...)`         |
| `[FromBody]`   | Body của request (JSON)             | `SaveBookRequest request` trong `Create(...)` |
| `[FromHeader]` | HTTP Header                          | Token JWT (`Authorization: Bearer ...`)         |

**Ví dụ thực tế:**

```csharp
// GET /api/books?keyword=harry&categoryId=xxx&onlyAvailable=true
public async Task<ActionResult<IEnumerable<BookResponse>>> GetAll(
    [FromQuery] string? keyword = null,      // từ query string
    [FromQuery] Guid? categoryId = null,
    [FromQuery] bool? onlyAvailable = null)
```

```csharp
// POST /api/books  (body: JSON)
public async Task<ActionResult<BookResponse>> Create([FromBody] SaveBookRequest request)
```

### `[ApiController]` làm gì?

- Tự động trả `400 Bad Request` nếu Model Validation thất bại (không cần kiểm tra `ModelState.IsValid` thủ công).
- Tự động binding body từ JSON mà không cần ghi `[FromBody]`.
- Tự động binding route param khi tên parameter khớp với `{id}` trong route template.

---

## 5. EF Core — DbContext, Include, N+1

### DbSet và AppDbContext trong dự án

File: `back_end/light_brary/code/Data/AppDbContext.cs`

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Book> Books => Set<Book>();          // Bảng Books
    public DbSet<BookCopy> BookCopies => Set<BookCopy>(); // Bảng BookCopies
    public DbSet<Category> Categories => Set<Category>();
    // ...
}
```

`DbSet<T>` đại diện cho một bảng trong database. Các truy vấn LINQ trên `DbSet` sẽ được EF Core dịch sang SQL.

### Include — Eager Loading (Tải dữ liệu liên kết ngay)

File: `back_end/light_brary/code/Repositories/BookRepository.cs`

```csharp
public async Task<Book?> GetWithDetailsAsync(Guid id)
{
    return await _context.Books
        .Include(b => b.Category)    // JOIN bảng Categories
        .Include(b => b.BookCopies)  // JOIN bảng BookCopies
        .FirstOrDefaultAsync(b => b.Id == id);
}
```

EF Core dịch đoạn trên thành SQL tương tự:

```sql
SELECT b.*, c.*, bc.*
FROM "Books" b
LEFT JOIN "Categories" c ON b."CategoryId" = c."Id"
LEFT JOIN "BookCopies" bc ON bc."BookId" = b."Id"
WHERE b."Id" = @id
LIMIT 1;
```

→ Chỉ **1 câu SQL** — hiệu quả.

### Vấn đề N+1 Query — Cần biết nhận ra

**N+1 xảy ra khi**: load danh sách N bản ghi, sau đó **trong vòng lặp** lại query database thêm 1 lần cho mỗi bản ghi → tổng N+1 câu SQL.

**Ví dụ bị N+1 (SAI):**

```csharp
var books = await _context.Books.ToListAsync(); // 1 câu SQL lấy N sách
foreach (var book in books)
{
    // MỖI lần truy cập book.Category → EF Core lại bắn 1 câu SQL!
    Console.WriteLine(book.Category.Name); // → N câu SQL nữa → tổng N+1
}
```

**Cách sửa — dùng `Include`:**

```csharp
var books = await _context.Books
    .Include(b => b.Category) // Load Category cùng lúc — chỉ 1 câu SQL
    .ToListAsync();
```

### AsNoTracking — Dùng khi chỉ đọc dữ liệu

```csharp
var books = await _context.Books
    .AsNoTracking() // Không theo dõi thay đổi → nhanh hơn, ít bộ nhớ hơn
    .ToListAsync();
```

Nên dùng `AsNoTracking()` cho các truy vấn **chỉ đọc** (GET). Nếu cần sửa rồi lưu lại thì **không dùng**.

### Migration

```bash
# Tạo migration mới khi thay đổi Entity
dotnet ef migrations add TenMigration

# Áp dụng migration lên database
dotnet ef database update
```

Trong dự án này, migration được **tự động áp dụng** lúc khởi động app (`Program.cs`):

```csharp
await context.Database.MigrateAsync(); // Chạy tất cả migration chưa được áp dụng
```

---

## 6. Authentication và Authorization (JWT + Policy)

### Sơ đồ luồng xác thực trong dự án

```
Client gửi: Authorization: Bearer eyJhbGc...
    │
    ▼
[JwtAuthenticationMiddleware]
    ├── Trích xuất token từ Header
    ├── Validate token (chữ ký, issuer, audience, hết hạn)
    ├── Nếu hợp lệ → tạo ClaimsPrincipal và gán vào context.User
    └── Gọi _next(context) để chuyển tiếp
    │
    ▼
[AuthorizationMiddleware — UseAuthorization()]
    ├── Đọc context.User (đã có claims từ bước trên)
    ├── Kiểm tra [Authorize] hoặc [Authorize(Policy="...")]
    ├── Nếu không đủ quyền → 403 Forbidden
    └── Nếu đủ quyền → chuyển tiếp vào Controller
    │
    ▼
[Controller Action]
```

### Authorization Policy trong dự án

File: `back_end/light_brary/Program.cs`

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",        p => p.RequireRole("Admin"));
    options.AddPolicy("LibrarianOrAdmin", p => p.RequireRole("Admin", "Librarian"));
    options.AddPolicy("UserOrAbove",      p => p.RequireRole("Admin", "Librarian", "User"));
});
```

**Áp dụng Policy tại Controller:**

```csharp
[HttpPost]
[Authorize(Policy = "LibrarianOrAdmin")] // Chỉ Librarian hoặc Admin mới POST được
public async Task<ActionResult<BookResponse>> Create([FromBody] SaveBookRequest request)

[HttpDelete("{id:guid}")]
[Authorize(Policy = "AdminOnly")]        // Chỉ Admin mới DELETE được
public async Task<IActionResult> Delete(Guid id)
```

### Phân quyền quyết định ở đâu?

1. **Quyết định phân quyền** diễn ra tại `AuthorizationMiddleware` (`app.UseAuthorization()`).
2. Middleware này đọc thông tin `ClaimsPrincipal` đã được gán bởi `JwtAuthenticationMiddleware`.
3. Nó so khớp Role/Claim của user với Policy được khai báo trong `[Authorize(Policy="...")]`.
4. Nếu không thỏa → `403 Forbidden` (đã xác thực nhưng không đủ quyền) hoặc `401 Unauthorized` (chưa đăng nhập).

### Kết quả khi 2 vai trò truy cập cùng endpoint `DELETE /api/books/{id}`

| Người dùng         | Role          | Kết quả                              |
| :-------------------- | :------------ | :------------------------------------- |
| Không đăng nhập   | —            | `401 Unauthorized`                   |
| Đăng nhập (User)   | `User`      | `403 Forbidden`                      |
| Thủ thư (Librarian) | `Librarian` | `403 Forbidden`                      |
| Quản trị (Admin)    | `Admin`     | `204 No Content` (xóa thành công) |

---

## 7. Xử lý lỗi nhất quán — Middleware Exception

### Vấn đề nếu không có middleware xử lý lỗi

- Mỗi Controller phải tự `try/catch` → code trùng lặp.
- Lỗi không được bắt trả về HTML trang lỗi mặc định của .NET (không phải JSON) → Frontend không xử lý được.

### Giải pháp trong dự án: `ExceptionHandlingMiddleware`

File: `back_end/light_brary/code/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
var (statusCode, code, message) = exception switch
{
    NotFoundException     => (404, "NOT_FOUND",         ex.Message),
    ConflictException     => (409, "CONFLICT",          ex.Message),
    BusinessException     => (400, "BAD_REQUEST",       ex.Message),
    UnauthorizedException => (401, "UNAUTHORIZED",      ex.Message),
    ForbiddenException    => (403, "FORBIDDEN",         ex.Message),
    DbUpdateException     => (409, "DATABASE_CONFLICT", "Xung đột dữ liệu."),
    _                     => (500, "INTERNAL_ERROR",    "Lỗi hệ thống.")
};
```

**Response JSON trả về (nhất quán):**

```json
{
  "status": 404,
  "code": "NOT_FOUND",
  "message": "Không tìm thấy đầu sách với ID: ..."
}
```

**Luồng hoạt động:**

1. `BookService.GetByIdAsync()` throw `NotFoundException`.
2. Exception bay qua Controller, Repository, Service — không ai bắt.
3. `ExceptionHandlingMiddleware` (đặt đầu pipeline) bắt được.
4. Trả về JSON `404` nhất quán → Frontend xử lý dễ dàng.

---

## 8. Cấu hình — appsettings, Secrets, Biến môi trường

### Thứ tự ưu tiên nạp cấu hình (từ thấp → cao)

```
appsettings.json
    ↓ (ghi đè)
appsettings.{Environment}.json   (Development, Production)
    ↓ (ghi đè)
User Secrets (chỉ Development)
    ↓ (ghi đè)
Environment Variables
    ↓ (ghi đè)
Command-line Arguments
```

### Cấu hình thực tế trong dự án

`appsettings.json` chứa cấu hình chung (không chứa secret):

```json
{
  "Jwt": {
    "Issuer": "light-brary-api",
    "Audience": "light-brary-client"
    // SecretKey KHÔNG được đặt ở đây
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=light_brary;..." // Lấy từ env
  }
}
```

**Đọc cấu hình trong `Program.cs`:**

```csharp
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"]; // Đọc từ môi trường hoặc User Secrets
```

### Nguyên tắc bảo mật

- **KHÔNG** commit `appsettings.Development.json` chứa password, JWT Secret Key, Connection String thật vào Git.
- Dùng `dotnet user-secrets` ở môi trường local development.
- Dùng biến môi trường (`ENV`) trên server production.

```bash
# Thêm secret khi dev local
dotnet user-secrets set "Jwt:SecretKey" "super-secret-key-here"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=..."
```

---

## 9. Middleware vs Filter — Phân biệt

Đây là câu hỏi **hay bị hỏi** vì nhiều người nhầm lẫn:

| Tiêu chí                      | Middleware                                                                         | Filter (Action Filter, Exception Filter)                    |
| :------------------------------ | :--------------------------------------------------------------------------------- | :---------------------------------------------------------- |
| **Vị trí**              | Nằm trong HTTP Pipeline (cấp ứng dụng)                                         | Nằm trong MVC Pipeline (cấp Controller/Action)            |
| **Phạm vi**              | Áp dụng cho**mọi request** (kể cả request đến file tĩnh, Swagger...) | Chỉ áp dụng cho request đến**Controller Action** |
| **Truy cập**             | `HttpContext`                                                                    | `ActionContext`, `ActionArguments`, `ActionResult`    |
| **Thứ tự**              | Trước khi vào MVC                                                               | Sau khi routing xác định được Controller/Action       |
| **Ví dụ trong dự án** | `ExceptionHandlingMiddleware`, `JwtAuthenticationMiddleware`                   | (Dự án chưa dùng Filter)                                |
| **Dùng khi**             | Logic cần áp dụng sớm nhất, trước MVC (auth, CORS, logging)                 | Logic cần biết thông tin về Action/Controller cụ thể  |

### Khi nào dùng Filter thay vì Middleware?

Dùng **Filter** khi cần:

- Ghi log tên Controller và Action cụ thể bị gọi.
- Kiểm tra input/output của Action (chỉ trong MVC).
- Cần truy cập `ModelState`, `ActionArguments`.

Dùng **Middleware** khi cần:

- Logic áp dụng toàn cục trước MVC (Authentication, Exception Handling, CORS).
- Xử lý các request **không đi qua Controller** (ví dụ: request đến file tĩnh).

---

## 10. Câu hỏi tự kiểm và lời giải

### ❓ Câu 1: Thứ tự middleware trong pipeline của dự án có ý nghĩa gì? Đảo hai cái thì sao?

**Lời giải:**
Trong `Program.cs`, thứ tự là:

```
ExceptionHandling → HTTPS → CORS → JwtAuth → Authorization → Controllers
```

- Nếu đặt `Authorization` **trước** `JwtAuthentication`: User luôn bị `401` vì `HttpContext.User` chưa được nạp.
- Nếu đặt `CORS` **sau** `Authorization`: Request từ browser bị Authorization chặn trước khi CORS xử lý, browser nhận lỗi không có CORS header → `CORS policy error`.
- Nếu đặt `ExceptionHandling` **không phải đầu tiên**: Các exception xảy ra ở middleware trước nó sẽ không được bắt và xử lý nhất quán.

---

### ❓ Câu 2: Service này nên Scoped hay Singleton? Vì sao?

**Lời giải:**

| Service                             | Lifetime đúng                | Lý do                                                                 |
| :---------------------------------- | :----------------------------- | :--------------------------------------------------------------------- |
| `AppDbContext`                    | **Scoped**               | Chứa dữ liệu trạng thái của 1 request; phải dispose sau request |
| `BookService`, `BookRepository` | **Scoped**               | Phụ thuộc`AppDbContext` (Scoped) → phải cùng lifetime           |
| `PasswordHasher`                  | Scoped/Singleton đều được | Không có state, không phụ thuộc DbContext                         |
| `JwtTokenService`                 | Singleton được              | Chỉ dùng config và thư viện, không có state                     |

---

### ❓ Câu 3: Authorization của endpoint `DELETE /api/books/{id}` được quyết định ở đâu?

**Lời giải:**

1. **Định nghĩa policy**: Tại `builder.Services.AddAuthorization(...)` trong `Program.cs`.
2. **Khai báo yêu cầu**: Attribute `[Authorize(Policy = "AdminOnly")]` trên method `Delete()` trong `BooksController`.
3. **Quyết định thực thi**: Tại `AuthorizationMiddleware` (được kích hoạt bởi `app.UseAuthorization()`), middleware này đọc `context.User` và đối chiếu với policy.
4. **Thông tin user được lấy từ**: `JwtAuthenticationMiddleware` đã giải mã JWT và gán `ClaimsPrincipal` vào `context.User` trước đó.

---

### ❓ Câu 4: Điều gì xảy ra với DbContext sau mỗi request?

**Lời giải:**

1. `AppDbContext` được đăng ký là `AddScoped` → **1 instance duy nhất cho toàn bộ request**.
2. Khi HTTP request kết thúc (response đã gửi về client), DI Container tự động gọi `appDbContext.Dispose()`.
3. `Dispose()` giải phóng kết nối database về **connection pool** (không đóng hẳn).
4. Mọi entity đang được theo dõi (tracked) bị xóa sạch khỏi Change Tracker.
5. Request tiếp theo → DI Container tạo **instance `AppDbContext` mới** hoàn toàn.

→ Đây là lý do tại sao **tuyệt đối không** đăng ký `DbContext` là `Singleton`.

---

## 11. So sánh ASP.NET Core với các Framework khác

| Framework             | Ngôn ngữ | Ưu điểm                                                   | Nhược điểm so với ASP.NET Core           |
| :-------------------- | :--------- | :----------------------------------------------------------- | :-------------------------------------------- |
| **Spring Boot** | Java       | Kiến trúc rất giống (DI, phân tầng), cộng đồng lớn | Khởi động chậm hơn, tốn RAM hơn        |
| **NestJS**      | TypeScript | Cú pháp decorator giống Angular, quen với FE dev         | Kiểu động hơn, hiệu năng thấp hơn     |
| **FastAPI**     | Python     | Viết cực nhanh, tự gen OpenAPI docs                       | Kiểu lỏng lẻo hơn, hiệu năng thấp hơn |
| **Go (Gin)**    | Go         | Cực nhẹ, khởi động nhanh, ít bộ nhớ                  | Không có ORM, DI, Auth tích hợp sẵn      |

### Khi KHÔNG nên dùng ASP.NET Core (dựa trên trải nghiệm dự án)

1. **Nhóm chưa quen C# và .NET**: Thời gian học ngôn ngữ, công cụ, quy ước đặt tên, async/await... sẽ chiếm nhiều thời gian hơn là làm tính năng. Nên chọn FastAPI (Python) hoặc Express (JS) nếu nhóm quen.
2. **Dịch vụ siêu nhỏ chỉ 2-3 endpoint đơn giản**: Cấu hình `Program.cs`, DI, middleware... vẫn nhiều boilerplate hơn so với FastAPI hay Express khi dự án quá nhỏ.
3. **Môi trường không hỗ trợ .NET Runtime**: Một số cloud/hosting rẻ tiền không có sẵn .NET runtime → phải dùng Docker → tốn thêm dung lượng và chi phí setup. Go hoặc Node.js nhẹ hơn nhiều trong trường hợp này.

### Lý do chọn ASP.NET Core cho dự án light-brary

- Nhóm quen C#, tận dụng được type-safety chặt chẽ (phát hiện lỗi lúc compile thay vì runtime).
- EF Core + PostgreSQL tích hợp mượt mà, migration tự động.
- DI tích hợp sẵn, không cần thư viện bên ngoài.
- Middleware pipeline mạnh mẽ, dễ thêm cross-cutting concerns (auth, logging, exception handling).
- Hiệu năng cao (top các benchmark web framework).
