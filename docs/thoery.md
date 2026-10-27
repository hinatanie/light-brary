# Lý Thuyết Ôn Thi ASP.NET Core — Dự án light-brary

> Tài liệu này được viết dựa trên **code thực tế** của dự án light-brary (.NET 10).
> Mỗi phần lý thuyết đi kèm ví dụ lấy trực tiếp từ file trong dự án — không phải ví dụ chung chung.

---

## Mục lục

1. [Luồng xử lý một Request](#1-luồng-xử-lý-một-request)
2. [Middleware Pipeline và Thứ tự](#2-middleware-pipeline-và-thứ-tự)
3. [Dependency Injection — Vòng đời Service](#3-dependency-injection--vòng-đời-service)
4. [Routing và Model Binding](#4-routing-và-model-binding)
5. [EF Core — DbContext, Include, N+1](#5-ef-core--dbcontext-include-n1)
6. [Authentication và Authorization (Identity Bearer Token + Policy)](#6-authentication-và-authorization-identity-bearer-token--policy)
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
[1] ExceptionHandlingMiddleware   ← Bọc try/catch toàn bộ pipeline
  │
  ▼
[2] HttpsRedirection              ← Chuyển HTTP → HTTPS
  │
  ▼
[3] CorsMiddleware                ← Kiểm tra và xử lý CORS header ("AllowFrontend")
  │
  ▼
[4] AuthenticationMiddleware       ← Đọc Bearer token (Identity), gán User vào HttpContext
  │
  ▼
[5] ValidateUserMiddleware         ← Kiểm tra Security Stamp & Trạng thái khóa tài khoản trong DB
  │
  ▼
[6] AuthorizationMiddleware       ← Kiểm tra [Authorize] / Policy ("AdminOnly", "LibrarianOrAdmin")
  │
  ▼
[7] BooksController.GetById()     ← Endpoint nhận request
  │
  ▼
[8] BookService                   ← Xử lý nghiệp vụ & Cache Redis (Cache-Aside: books:{id})
  │
  ▼
[9] BookRepository                ← Truy vấn EF Core
  │
  ▼
[10] AppDbContext → PostgreSQL    ← Câu lệnh SQL thực sự chạy
  │
  ◄─────────── Kết quả trả ngược lại qua từng tầng ───────────
  │
  ▼
Client nhận JSON Response
```

**Đoạn code trong `Program.cs` thể hiện thứ tự pipeline:**

```csharp
app.UseExceptionHandlingMiddleware(); // [1] Bọc ngoài cùng để bắt mọi lỗi
app.UseHttpsRedirection();            // [2] Chuyển hướng HTTPS
app.UseCors("AllowFrontend");         // [3] Xử lý CORS
app.UseAuthentication();              // [4] Xác thực danh tính (Authentication)
app.UseMiddleware<ValidateUserMiddleware>(); // [5] Kiểm tra Security Stamp & Khóa tài khoản
app.UseAuthorization();               // [6] Phân quyền (Authorization)
app.MapControllers();                 // [7] Định tuyến đến Controller
app.MapHealthChecks("/health").AllowAnonymous(); // [8] Endpoint kiểm tra sức khỏe hệ thống
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

### Câu hỏi quan trọng: **Đảo thứ tự Authentication, ValidateUser và Authorization thì sao?**

Dự án đang đặt đúng thứ tự:

```csharp
app.UseAuthentication();                     // [4] — Xác thực: "Bạn là ai?" (giải mã token thành context.User)
app.UseMiddleware<ValidateUserMiddleware>();  // [5] — Kiểm tra bảo mật: "Phiên này còn hiệu lực không? Tài khoản có bị khóa không?"
app.UseAuthorization();                      // [6] — Phân quyền: "Bạn có quyền thực hiện hành động này không?"
```

**Tình huống thứ tự có ý nghĩa then chốt:**
1. **`ValidateUserMiddleware` bắt buộc phải đặt SAU `UseAuthentication`:** Vì nó cần đọc `context.User` đã được giải mã từ Bearer token để lấy `UserId`, sau đó gọi `signInManager.ValidateSecurityStampAsync(context.User)` và `userManager.IsLockedOutAsync(user)`. Nếu đặt trước `UseAuthentication`, `context.User.Identity.IsAuthenticated` luôn bằng `false` $\rightarrow$ middleware không kiểm tra được gì!
2. **`ValidateUserMiddleware` phải đặt TRƯỚC `UseAuthorization`:** Để nếu người dùng đã bị khóa tài khoản hoặc đã đăng xuất/đổi mật khẩu ở thiết bị khác (Security Stamp đổi), request sẽ bị chặn ngay lập tức với `401 Unauthorized` trước khi `UseAuthorization` kịp phê duyệt quyền.
3. **Nếu đảo ngược `UseAuthorization` lên trước `UseAuthentication`:**
   - `UseAuthorization` chạy trước nhưng `HttpContext.User` vẫn đang **rỗng** (chưa được nạp từ token).
   - `User.Identity.IsAuthenticated` = `false` $\rightarrow$ Authorization luôn từ chối request với `401 Unauthorized`.
   - **Hệ quả**: Mọi endpoint có `[Authorize]` đều sẽ bị chặn kể cả khi client gửi token hợp lệ.

### Middleware `ExceptionHandlingMiddleware` trong dự án

File: `back_end/light_brary/code/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
public async Task InvokeAsync(HttpContext context)
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
| `[FromHeader]` | HTTP Header                          | Bearer Token (`Authorization: Bearer ...`)         |

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

## 6. Authentication và Authorization (Identity Bearer Token + Policy)

### Sơ đồ luồng xác thực trong dự án

```
Client gửi: Authorization: Bearer CfDJ8M...
    │
    ▼
[AuthenticationMiddleware — UseAuthentication()]
    ├── Trích xuất token từ Header "Authorization: Bearer ..."
    ├── Giải mã và kiểm tra tính toàn vẹn (ASP.NET Core Data Protection)
    ├── Nạp Claims (UserId, UserName, Roles) vào ClaimsPrincipal
    ├── Gán ClaimsPrincipal vào context.User
    └── Chuyển sang middleware tiếp theo
    │
    ▼
[ValidateUserMiddleware]
    ├── Đọc UserId từ context.User
    ├── Kiểm tra Security Stamp trong DB (bị vô hiệu hóa nếu đã logout hoặc đổi mật khẩu)
    ├── Kiểm tra trạng thái khóa tài khoản (IsLockedOutAsync)
    └── Nếu hợp lệ → gọi _next(context)
    │
    ▼
[AuthorizationMiddleware — UseAuthorization()]
    ├── Đọc context.User đã được xác thực
    ├── Kiểm tra [Authorize] hoặc Policy ("AdminOnly", "LibrarianOrAdmin")
    ├── Nếu chưa đăng nhập → 401 Unauthorized
    ├── Nếu thiếu quyền → 403 Forbidden
    └── Nếu đủ quyền → chuyển tiếp vào Controller Action
    │
    ▼
[Controller Action]
```

### Authorization Policy trong dự án

File: `back_end/light_brary/Program.cs`

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(RoleNames.ADMIN));
    options.AddPolicy("LibrarianOrAdmin", p => p.RequireRole(RoleNames.ADMIN, RoleNames.LIBRARIAN));
});
```

**Áp dụng Policy tại Controller:**

```csharp
[HttpPost]
[Authorize(Policy = "LibrarianOrAdmin")] // Chỉ Librarian hoặc Admin mới tạo được sách
public async Task<ActionResult<BookResponse>> Create(SaveBookRequest request)

[HttpDelete("{id:guid}")]
[Authorize(Policy = "AdminOnly")]        // Chỉ Admin mới xóa được đầu sách
public async Task<IActionResult> Delete(Guid id)
```

### Phân quyền quyết định ở đâu?

1. **Quyết định phân quyền** diễn ra tại **`AuthorizationMiddleware`** (`app.UseAuthorization()`).
2. Middleware này đọc thông tin `ClaimsPrincipal` trong `context.User` (đã được `UseAuthentication` giải mã và `ValidateUserMiddleware` kiểm tra).
3. Nó so khớp Role/Claim của user với Policy được khai báo trong `[Authorize(Policy = "...")]`.
4. Nếu không thỏa $\rightarrow$ `403 Forbidden` (đã đăng nhập nhưng không đủ quyền) hoặc `401 Unauthorized` (chưa đăng nhập).

### Kết quả khi các vai trò truy cập cùng endpoint `DELETE /api/books/{id}`

| Người dùng | Role | Kết quả |
| :--- | :--- | :--- |
| Không đăng nhập | — | `401 Unauthorized` |
| Đăng nhập (Sinh viên) | `User` | `403 Forbidden` |
| Thủ thư (Librarian) | `Librarian` | `403 Forbidden` |
| Quản trị viên (Admin) | `Admin` | `204 No Content` (xóa thành công) |

---

## 7. Xử lý lỗi nhất quán — Middleware Exception

### Vấn đề nếu không có middleware xử lý lỗi

- Mỗi Controller phải tự `try/catch` $\rightarrow$ code trùng lặp, dễ sót.
- Lỗi không được bắt sẽ trả về HTML trang lỗi mặc định của ASP.NET Core $\rightarrow$ Frontend không parse được JSON.

### Giải pháp trong dự án: `ExceptionHandlingMiddleware`

File: `back_end/light_brary/code/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
var (statusCode, code, message) = exception switch
{
    AppException appEx     => (appEx.StatusCode, appEx.Code, appEx.Message),
    ArgumentException argEx => (HttpStatusCode.BadRequest, "BAD_REQUEST", argEx.Message),
    DbUpdateException      => (HttpStatusCode.Conflict, "DATABASE_CONFLICT", "Xung đột dữ liệu cơ sở dữ liệu."),
    _                      => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "Đã xảy ra lỗi hệ thống không mong muốn.")
};
```

**Response JSON trả về (nhất quán):**

```json
{
  "status": 404,
  "code": "NOT_FOUND",
  "message": "Không tìm thấy đầu sách với ID: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Luồng hoạt động:**

1. `BookService.GetByIdAsync()` ném ra `AppException.NotFound("...")`.
2. Exception lan truyền ngược qua Controller, Repository $\rightarrow$ không khối nào chặn.
3. `ExceptionHandlingMiddleware` (bọc ngoài cùng pipeline) bắt được qua khối `catch (Exception ex)`.
4. Trích xuất `StatusCode`, `Code`, `Message` và serialize thành JSON lỗi nhất quán trả về cho Client.

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

`appsettings.json` chứa cấu hình chung (không chứa secret nhạy cảm):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lightbrary_db;Username=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Seed": {
    "AdminPassword": "",
    "AdminPhone": "0999999999"
  }
}
```

**Đọc cấu hình trong `Program.cs`:**

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var redisString = builder.Configuration.GetConnectionString("Redis");
```

### Nguyên tắc bảo mật

- **KHÔNG** commit mật khẩu CSDL thật, Redis password hay admin seed password vào Git.
- Dùng `dotnet user-secrets` ở môi trường local development:
  ```bash
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Password=..." --project back_end/light_brary
  dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6381" --project back_end/light_brary
  dotnet user-secrets set "Seed:AdminPassword" "super-secret-password" --project back_end/light_brary
  ```
- Dùng biến môi trường (`ENV`) khi chạy trên Docker Compose / Server:
  - `ConnectionStrings__DefaultConnection`
  - `ConnectionStrings__Redis`
  - `Seed__AdminPassword`

---

## 9. Middleware vs Filter — Phân biệt

Đây là câu hỏi **hay bị hỏi** vì nhiều người nhầm lẫn:

| Tiêu chí                      | Middleware                                                                         | Filter (Action Filter, Exception Filter)                    |
| :------------------------------ | :--------------------------------------------------------------------------------- | :---------------------------------------------------------- |
| **Vị trí**              | Nằm trong HTTP Pipeline (cấp ứng dụng)                                         | Nằm trong MVC Pipeline (cấp Controller/Action)            |
| **Phạm vi**              | Áp dụng cho **mọi request** (kể cả request đến file tĩnh, Swagger, HealthCheck) | Chỉ áp dụng cho request đến **Controller Action** |
| **Truy cập**             | `HttpContext`                                                                    | `ActionContext`, `ActionArguments`, `ActionResult`    |
| **Thứ tự**              | Trước khi vào MVC                                                               | Sau khi routing xác định được Controller/Action       |
| **Ví dụ trong dự án** | `ExceptionHandlingMiddleware`, `ValidateUserMiddleware`                           | (Dự án dùng Middleware toàn cục thay vì Filter)             |
| **Dùng khi**             | Logic cần áp dụng sớm nhất, trước MVC (auth, CORS, exception handling)         | Logic cần biết thông tin về Action/Controller cụ thể  |

### Khi nào dùng Filter thay vì Middleware?

Dùng **Filter** khi cần:

- Ghi log tên Controller và Action cụ thể bị gọi.
- Kiểm tra input/output của Action (chỉ trong MVC).
- Cần truy cập `ModelState`, `ActionArguments`.

Dùng **Middleware** khi cần:

- Logic áp dụng toàn cục trước MVC (Authentication, Exception Handling, CORS).
- Xử lý các request **không đi qua Controller** (ví dụ: request đến file tĩnh hoặc kiểm tra sức khỏe `/health`).

---

## 10. Câu hỏi tự kiểm và lời giải

### ❓ Câu 1: Thứ tự middleware trong pipeline của dự án có ý nghĩa gì? Đảo hai cái thì sao?

**Lời giải:**
Trong `Program.cs`, thứ tự chuẩn là:

```text
ExceptionHandling → HTTPS → CORS → Authentication → ValidateUserMiddleware → Authorization → Controllers → HealthChecks
```

- **Nếu đặt `Authorization` trước `Authentication`**: `context.User` chưa được giải mã từ Bearer token $\rightarrow$ người dùng luôn bị xem là chưa đăng nhập và bị từ chối với `401 Unauthorized`.
- **Nếu đặt `ValidateUserMiddleware` sau `Authorization`**: Nếu một người dùng vừa bị Admin khóa tài khoản hoặc vừa đăng xuất (`PUT /api/auth/logout` làm mới Security Stamp), họ vẫn có thể thực thi request nếu Policy được thỏa mãn trước khi kiểm tra trạng thái khóa $\rightarrow$ gây ra lỗ hổng bảo mật nghiêm trọng.
- **Nếu đặt `CORS` sau `Authorization`**: Request Preflight `OPTIONS` từ trình duyệt sẽ bị chặn lại trước khi kịp gắn CORS Header $\rightarrow$ trình duyệt báo lỗi `CORS policy error`.
- **Nếu đặt `ExceptionHandling` không phải đầu tiên**: Các lỗi phát sinh ở các middleware đứng trước nó sẽ không được bọc trong `try/catch`, khiến lỗi bị xì ra ngoài dưới dạng HTML lỗi mặc định thay vì JSON chuẩn.

---

### ❓ Câu 2: Service này nên Scoped hay Singleton? Vì sao?

**Lời giải:**

| Service / Thành phần | Lifetime đúng | Lý do |
| :--- | :--- | :--- |
| `AppDbContext` | **Scoped** | Quản lý kết nối DB và ChangeTracker cho 1 request; phải dispose sau khi request kết thúc |
| `BookService`, `BookRepository` | **Scoped** | Phụ thuộc trực tiếp vào `AppDbContext` $\rightarrow$ bắt buộc phải là Scoped |
| `BorrowRecordService`, `UserService` | **Scoped** | Phụ thuộc `AppDbContext` và `UserManager<User>` $\rightarrow$ bắt buộc phải là Scoped |
| `ValidateUserMiddleware` | **Transient/Inline** | Được gọi thông qua pipeline, inject `UserManager<User>` (Scoped) theo từng invoke |

> **Nguy cơ Captive Dependency:** Nếu đăng ký `BookService` là **Singleton**, nó sẽ giữ một thể hiện `AppDbContext` (Scoped) tồn tại mãi mãi trong suốt vòng đời ứng dụng $\rightarrow$ Rò rỉ bộ nhớ (Memory Leak), không giải phóng connection, và gây crash hệ thống khi nhiều request đồng thời truy cập do `DbContext` không thread-safe.

---

### ❓ Câu 3: Authorization của endpoint `DELETE /api/books/{id}` được quyết định ở đâu?

**Lời giải:**

1. **Định nghĩa policy**: Tại `builder.Services.AddAuthorization(...)` trong `Program.cs` (`"AdminOnly"` yêu cầu Role `Admin`).
2. **Khai báo yêu cầu**: Attribute `[Authorize(Policy = "AdminOnly")]` trên method `Delete()` trong `BooksController`.
3. **Quyết định thực thi**: Tại `AuthorizationMiddleware` (được kích hoạt bởi `app.UseAuthorization()`).
4. **Thông tin user được lấy từ**: `AuthenticationMiddleware` (đã giải mã Bearer Token và gán `ClaimsPrincipal` vào `context.User`) và được thẩm định hợp lệ bởi `ValidateUserMiddleware` (kiểm tra tài khoản không bị khóa và Security Stamp còn hiệu lực).

---

### ❓ Câu 4: Điều gì xảy ra với DbContext sau mỗi request?

**Lời giải:**

1. `AppDbContext` được đăng ký là `AddScoped` $\rightarrow$ **1 instance duy nhất cho toàn bộ request**.
2. Khi HTTP request kết thúc (response đã gửi về client), DI Container tự động gọi `appDbContext.DisposeAsync()`.
3. `DisposeAsync()` trả kết nối database về **connection pool** của PostgreSQL (không đóng hẳn socket để tái sử dụng).
4. Mọi entity đang được theo dõi (tracked) bị xóa sạch khỏi bộ nhớ Change Tracker.
5. Request tiếp theo $\rightarrow$ DI Container tạo **instance `AppDbContext` mới** hoàn toàn.

$\rightarrow$ Đây là lý do tại sao **tuyệt đối không** đăng ký `DbContext` là `Singleton`.

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
