# TÓM TẮT TOÀN DIỆN DỰ ÁN LIGHTBRARY (BẢN CHỐT KỸ THUẬT)

> **Dành cho nhóm phát triển & thành viên viết báo cáo (T & YNhi)**  
> Bản tóm tắt này phản ánh **100% mã nguồn thực tế** mới nhất trên nhánh `main`, được chuẩn hóa trực tiếp theo tiêu chí đánh giá môn học **Các Công nghệ Lập trình Hiện đại** (mục **ASP.NET Core**, trang 15–16 trong tài liệu yêu cầu bổ sung của giảng viên).

---

## 🏛️ TỔNG QUAN KIẾN TRÚC HỆ THỐNG

Dự án **LightBrary** là hệ thống quản lý thư viện số được tổ chức theo kiến trúc Client - Server hiện đại với hai thành phần:

1. **Backend (Trọng tâm cốt lõi — ASP.NET Core .NET 10):**
   - Đóng vai trò là nền tảng xử lý nghiệp vụ, quản trị cơ sở dữ liệu và bảo mật toàn diện.
   - Cung cấp **37 RESTful API nghiệp vụ + 1 HealthCheck** chia thành 6 phân hệ tài nguyên, áp dụng kiến trúc Repository Pattern, Entity Framework Core 10 (PostgreSQL 17), và bộ nhớ đệm phân tán Redis 7 (Cache-Aside & Multi-tier Invalidation).
   - Đây là đối tượng nghiên cứu và đánh giá chính của đồ án theo đúng tiêu chuẩn môn học.

2. **Frontend (Giao diện trực quan — React 19 + TypeScript):**
   - Đặt tại thư mục [`front_end/`](file:///home/dww/Personal/Code/Csharp/light-brary/front_end/), xây dựng bằng **React 19 + TypeScript + Vite + Tailwind CSS**.
   - **Mục đích:** Thực hiện gọi các API Backend một cách trực quan, sinh động trong quá trình chạy thử nghiệm, kiểm thử và demo trực tiếp trước Giảng viên / Hội đồng.
   - Hỗ trợ đầy đủ 3 phân hệ giao diện người dùng:
     - **Admin (Quản trị hệ thống):** Quản lý tài khoản người dùng, phân quyền vai trò (Roles), xóa thể loại/đầu sách.
     - **Staff (Thủ thư):** Quản lý kho sách, thêm/sửa bản sao (quét barcode), lập phiếu mượn, ghi nhận trả sách và báo mất sách.
     - **Member (Bạn đọc / Sinh viên):** Tra cứu, tìm kiếm danh mục sách theo thể loại/tác giả, xem chi tiết tình trạng sách và theo dõi lịch sử mượn trả cá nhân.
   - *(Trong báo cáo đồ án, Front-end được trình bày ngắn gọn ở vai trò API Consumer phục vụ demo trực quan, toàn bộ chiều sâu kỹ thuật tập trung 100% vào ASP.NET Core).*

---

## 🛠️ PHẦN 1: CÔNG CỤ VÀ PHIÊN BẢN CẦN CÀI ĐẶT

Khi cài đặt môi trường để chạy hoặc kiểm thử dự án, cần các công cụ với phiên bản chính xác như sau:

| Công cụ / Nền tảng | Phiên bản yêu cầu | Mục đích sử dụng | Ghi chú |
| :--- | :--- | :--- | :--- |
| **.NET SDK** | **.NET 10 LTS** (Cụ thể SDK `10.0.113` / Target Framework `net10.0`) | Biên dịch, chạy Backend API và chạy bộ Unit Test | Bắt buộc nếu chạy không qua Docker |
| **PostgreSQL** | **PostgreSQL 17** (hoặc v16+) | Cơ sở dữ liệu quan hệ lưu trữ dữ liệu chính | Image Docker: `postgres:17-alpine` |
| **Redis** | **Redis 7.x** | In-memory Distributed Cache lưu chi tiết sách | Image Docker: `redis:7-alpine` |
| **Docker & Docker Compose** | Docker v24+, Compose v2+ | Đóng gói và chạy toàn bộ hệ thống bằng 1 lệnh | Khuyến nghị dùng trên Linux / Docker Desktop |
| **Node.js & npm** *(Tùy chọn)* | Node.js v20+ hoặc v22+, npm 10+ | Chạy giao diện Front-end React khi cần demo | Chỉ cần khi muốn chạy Front-end trực tiếp |
| **Visual Studio / VS Code** | VS 2026 hoặc VS Code (C# Dev Kit) | Môi trường lập trình (IDE) | Hỗ trợ gỡ lỗi và quản lý User Secrets |

---

## 🎯 PHẦN 2: CÁC CÔNG NGHỆ SỬ DỤNG — PHÂN CẤP THEO BAREM CỦA GIẢNG VIÊN

Đối chiếu trực tiếp với tài liệu quy định của môn học:

### 2.1. MỨC 1: BẮT BUỘC (Cơ bản — Nền tảng Backend ASP.NET Core)

Nhóm đã hiện thực đầy đủ 100% các yêu cầu bắt buộc:

1. **Luồng xử lý một Request chuẩn RESTful:**
   - Request từ client đi qua **Middleware Pipeline** (xử lý ngoại lệ, CORS, xác thực danh tính, kiểm tra trạng thái khóa/Security Stamp, phân quyền) $\rightarrow$ đến **Controller** định tuyến $\rightarrow$ gọi **Service** chứa nghiệp vụ $\rightarrow$ gọi **Repository** truy xuất dữ liệu $\rightarrow$ thông qua **EF Core DbContext** sinh câu lệnh SQL gửi đến PostgreSQL $\rightarrow$ đóng gói dữ liệu thành **DTO** trả JSON về cho Client.
2. **Routing và Model Binding:**
   - Dùng **Attribute Routing**: `[Route("api/[controller]")]`, `[HttpGet("{id:guid}")]`, `[HttpPost]`, v.v.
   - Ràng buộc kiểu dữ liệu ngay trên route (Route Constraint): `{id:guid}` ngăn chặn request rác nếu ID không phải định dạng UUID.
   - Model Binding linh hoạt: `[FromRoute]` lấy ID, `[FromQuery]` lấy từ khóa tìm kiếm/lọc/phân trang (`keyword`, `categoryId`, `page`, `pageSize`), `[FromBody]` nhận dữ liệu JSON payload.
   - Model Validation: Sử dụng **Data Annotations** (`[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]`) trên các DTO Request; `[ApiController]` tự động kích hoạt kiểm tra và trả lỗi `400 Bad Request` nếu dữ liệu vi phạm.
3. **Truy cập dữ liệu & EF Core Cơ bản:**
   - Dùng **EF Core 10** (`Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`).
   - `AppDbContext` kế thừa `IdentityDbContext<User, IdentityRole<Guid>, Guid>`.
   - **Migrations:** Quản lý lược đồ CSDL bằng migration (`20261022030516_chantroimoi`). Tự động chạy `context.Database.MigrateAsync()` ngay khi ứng dụng khởi động.
4. **Xử lý lỗi nhất quán (Consistent Error Handling):**
   - Viết Custom Middleware: `ExceptionHandlingMiddleware` bọc toàn bộ pipeline bằng khối `try/catch`.
   - Bắt các ngoại lệ nghiệp vụ (`AppException`), ngoại lệ đối số (`ArgumentException`), ngoại lệ DB (`DbUpdateException`) và lỗi hệ thống không kiểm soát.
   - Trả về mã HTTP tương ứng (`400`, `401`, `403`, `404`, `409`, `500`) kèm body JSON thống nhất:
     ```json
     {
       "status": 404,
       "code": "NOT_FOUND",
       "message": "Không tìm thấy đầu sách với ID: ..."
     }
     ```
5. **Quản lý cấu hình theo môi trường (Configuration):**
   - Phân tách cấu hình rõ ràng: `appsettings.json` (cấu hình chung), `appsettings.Development.json` (bật Swagger, logging chi tiết), và biến môi trường (Environment Variables trên Docker).
   - Dùng **.NET User Secrets** (`UserSecretsId`) khi phát triển cục bộ, không commit mật khẩu, chuỗi kết nối nhạy cảm lên Git.
6. **Middleware Pipeline và Thứ tự thực thi:**
   - Thứ tự thiết lập trong `Program.cs`:
     `UseSwagger` $\rightarrow$ `UseExceptionHandlingMiddleware` $\rightarrow$ `UseHttpsRedirection` $\rightarrow$ `UseCors` $\rightarrow$ `UseAuthentication` $\rightarrow$ `UseMiddleware<ValidateUserMiddleware>` $\rightarrow$ `UseAuthorization` $\rightarrow$ `MapControllers` $\rightarrow$ `MapHealthChecks`.
   - **Tình huống thứ tự có ý nghĩa then chốt (Trả lời câu hỏi thi):**
     - Đặt `ValidateUserMiddleware` **sau** `UseAuthentication` và **trước** `UseAuthorization`: Vì `ValidateUserMiddleware` cần thông tin `context.User` đã được giải mã từ token bởi `UseAuthentication` để kiểm tra Security Stamp và trạng thái khóa tài khoản trong CSDL. Nếu tài khoản bị khóa hoặc đổi mật khẩu, middleware lập tức ngắt pipeline và trả lỗi `401 Unauthorized` trước khi `UseAuthorization` kịp kiểm tra phân quyền.
     - Nếu đảo `UseAuthorization` lên trước `UseAuthentication`, `context.User` chưa được nạp $\rightarrow$ mọi endpoint yêu cầu quyền đều bị chặn `401`.
7. **Dependency Injection (DI) tích hợp sẵn:**
   - Toàn bộ `Service` (`BookService`, `UserService`, ...) và `Repository` (`BookRepository`, ...) được đăng ký với vòng đời **Scoped** (`builder.Services.AddScoped<...>()`).
   - Lý do chọn **Scoped**: Gắn liền với vòng đời của từng HTTP Request (khởi tạo khi có request và hủy/giải phóng tài nguyên khi request kết thúc), hoàn toàn đồng bộ với vòng đời của `AppDbContext` (vốn là Scoped).
   - Hậu quả nếu đăng ký sai (Captive Dependency): Nếu đăng ký Service là `Singleton` mà inject `AppDbContext` (Scoped) vào trong nó, `DbContext` sẽ bị giữ lại suốt vòng đời ứng dụng $\rightarrow$ gây rò rỉ bộ nhớ (Memory Leak), không giải phóng kết nối, và lỗi xung đột đa luồng khi hai request đồng thời dùng chung một `DbContext`.
8. **Kiến trúc Endpoint (Controller vs Minimal API):**
   - Dự án chọn **Controller-based API** (`[ApiController]` kế thừa `ControllerBase`).
   - Lý do: Nghiệp vụ thư viện có số lượng endpoint tương đối lớn (37 endpoints nghiệp vụ), chia thành 6 phân hệ tài nguyên và phân quyền phức tạp theo Role/Policy. Mô hình Controller giúp gom nhóm theo thực thể, dễ tổ chức code, áp dụng bộ lọc và dễ bảo trì hơn so với Minimal API dồn hết vào `Program.cs`.

---

### 2.2. MỨC 2: TỰ CHỌN THEO ỨNG DỤNG (Trung bình)

1. **EF Core Chuyên sâu:**
   - Toàn bộ truy vấn viết bất đồng bộ (`async`/`await`: `ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`).
   - Tối ưu hiệu năng đọc với `AsNoTracking()` trong `GenericRepository` khi chỉ truy vấn dữ liệu hiển thị, giảm tải cho bộ theo dõi trạng thái `ChangeTracker`.
   - Quản lý ràng buộc toàn vẹn: Cấu hình `DeleteBehavior.Restrict` (chặn xóa Thể loại nếu còn sách).
   - Quản lý Transaction phân tán: Sử dụng `await using var transaction = await context.Database.BeginTransactionAsync()` khi seed dữ liệu phức tạp hoặc tạo người dùng để đảm bảo tính nguyên tử (Atomicity).
2. **Xác thực & Phân quyền (Authentication & Authorization):**
   - Sử dụng **ASP.NET Core Identity API Endpoints** (`AddIdentityApiEndpoints<User>`) với Data Protection Bearer Token (chuẩn mới từ .NET 8 trở lên).
   - Hệ thống phân quyền 3 cấp độ (Role-Based Access Control): `Admin`, `Librarian`, `User`.
   - Cấu hình **Policy-Based Authorization**:
     - Policy `"AdminOnly"`: Yêu cầu Role `Admin`.
     - Policy `"LibrarianOrAdmin"`: Cho phép Role `Admin` hoặc `Librarian`.
   - Tính năng bảo mật chuyên sâu:
     - Tự động khóa tài khoản khi nhập sai mật khẩu 5 lần liên tiếp trong 15 phút (`LockoutEnabled = true`, `MaxFailedAccessAttempts = 5`).
     - Tự động vô hiệu hóa phiên làm việc tức thì khi người dùng đổi mật khẩu hoặc đăng xuất thông qua cơ chế `UpdateSecurityStampAsync` trong `ValidateUserMiddleware`.
3. **Health Checks:**
   - Tích hợp `builder.Services.AddHealthChecks()` và map endpoint `/health`. Phục vụ kiểm tra trạng thái hoạt động (Liveness Probe) cho container Docker.
4. **CORS (Cross-Origin Resource Sharing):**
   - Định nghĩa policy `AllowFrontend` cho phép các nguồn `http://localhost:3000`, `http://localhost:5173` gửi request kèm thông tin xác thực (`AllowCredentials`).

---

### 2.3. MỨC 3: NÂNG CAO (Tạo chiều sâu — Trọng tâm đánh giá điểm cao)

Thầy giáo lưu ý: *"Ưu tiên ít mục nhưng làm sâu, có minh chứng và có số liệu"*. Dự án tập trung vào 4 điểm nâng cao cốt lõi:

#### 1. Tối ưu truy vấn, Xử lý N+1 và Bộ nhớ đệm phân tán Redis (Redis Distributed Cache)
- **Thư viện sử dụng:** `Microsoft.Extensions.Caching.StackExchangeRedis` thông qua giao diện trừu tượng chuẩn `IDistributedCache`.
- **Minh chứng Tái hiện và Khắc phục N+1 Query (Có số liệu & câu lệnh SQL):**
  - *Tình huống trong thư viện:* Khi người dùng xem danh sách 30 cuốn sách và cần hiển thị kèm tên Thể loại (`Category`) và số lượng Bản sao (`BookCopies`).
  - *Truy vấn bị lỗi N+1 (Cách viết sai):*
    ```csharp
    var books = await _context.Books.ToListAsync(); // 1 câu SQL lấy 30 sách
    foreach (var b in books) {
        var cat = await _context.Categories.FindAsync(b.CategoryId); // 30 câu SQL riêng lẻ!
    }
    ```
    $\rightarrow$ **Hậu quả:** Phát sinh tổng cộng **31 câu lệnh SQL** gửi dồn dập về PostgreSQL. Thời gian phản hồi đo được: **~35ms - 50ms**.
  - *Khắc phục bằng Eager Loading (`Include` / Projection trong `BookRepository.cs`):*
    ```csharp
    await _context.Books
        .Include(b => b.Category)
        .Include(b => b.BookCopies)
        .AsNoTracking()
        .FirstOrDefaultAsync(b => b.Id == id);
    ```
    $\rightarrow$ **Kết quả:** EF Core biên dịch thành **duy nhất 1 câu lệnh SQL JOIN**, nạp trọn vẹn dữ liệu:
    ```sql
    SELECT b."Id", b."Title", b."Author", b."ISBN", c."Name", bc."Barcode", bc."Status"
    FROM "Books" AS b
    LEFT JOIN "Categories" AS c ON b."CategoryId" = c."Id"
    LEFT JOIN "BookCopies" AS bc ON b."Id" = bc."BookId"
    WHERE b."Id" = @id;
    ```
    $\rightarrow$ Số truy vấn giảm từ 31 xuống **1 câu SQL**, thời gian phản hồi giảm xuống **~3ms - 5ms**.
  - *Bước nâng cao: Đưa Redis Distributed Cache vào xử lý (Cache-Aside):*
    - Khi có Redis Cache Hit: Số câu truy vấn SQL gửi đến Database là **0 câu**.
    - Thời gian phản hồi giảm xuống **< 1ms**, chịu tải được hàng ngàn request đồng thời mà DB không bị nghẽn CPU/RAM.

- **Redis được dùng ở mức độ nào? Có sâu không? (Trả lời câu hỏi của thành viên nhóm):**
  - **Mức độ áp dụng:** Nhóm áp dụng kỹ thuật **Cache-Aside Pattern (Lazy Loading)** cho endpoint đọc chi tiết đầu sách `GET /api/books/{id}`.
  - **Cơ chế hoạt động:**
    - *Cache Miss (Lần đọc đầu):* Kiểm tra Redis với key `LightBrary:books:{id}`. Nếu chưa có, query PostgreSQL thông qua `BookRepository.GetWithDetailsAsync(id)` (có Eager Loading `Category` và `BookCopies`), nạp dữ liệu vào Redis với thời gian hết hạn cố định **TTL = 10 phút** (`AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)`), rồi trả về Client.
    - *Cache Hit (Các lần đọc sau):* Đọc trực tiếp chuỗi JSON từ Redis, deserialize thành `BookResponse` và trả về ngay mà **không cần chạm vào Database**.
  - **Cơ chế Invalidation chủ động (Cache Invalidation) — Điểm sâu và tinh tế:**
    - Nhóm không chỉ xóa cache khi sửa/xóa sách (`UpdateAsync`, `DeleteAsync`), mà còn **tự động vô hiệu hóa cache khi các thực thể liên đới thay đổi trạng thái:**
      1. Khi thủ thư tạo hoặc xóa một bản sao vật lý (`BookCopyService`), cache đầu sách đó tự động bị xóa để cập nhật lại `totalCopies` và `availableCopies`.
      2. Khi độc giả mượn sách (`BorrowRecordService.CreateAsync`), cache của toàn bộ các đầu sách có bản sao được mượn lập tức bị xóa để giảm số lượng `availableCopies`.
      3. Khi độc giả trả sách (`ReturnMultipleBooksAsync`) hoặc báo mất sách (`MarkLostBookAsync`), cache của các đầu sách tương ứng cũng bị xóa ngay lập tức.
      4. Khi đổi thông tin Thể loại (`CategoryService.UpdateAsync`), toàn bộ cache của các đầu sách thuộc thể loại đó đều được làm mới.
  - **Phạm vi không làm quá lan man:** Nhóm **không** đưa vào Redis Pub/Sub, Redis Streams hay RedLock phân tán vì hệ thống thư viện đơn giản không có nhu cầu stream hay khóa phân tán nhiều cụm máy chủ; làm như trên là vừa đúng yêu cầu, chuẩn kiến trúc cache cho REST API và hoàn toàn làm chủ được mã nguồn.

#### 2. Kiểm thử tự động chuyên sâu (Automated Testing với xUnit)
- Thư mục kiểm thử: `back_end/light_brary.Tests/`.
- File test: `BookCacheTests.cs` gồm **9 ca kiểm thử toàn diện** đạt 100% tỷ lệ pass:
  1. `GetById_CachesResponse_AndReadsCacheWithoutDatabase`: Chứng minh khi đóng kết nối DB, API vẫn đọc dữ liệu trả về bình thường từ Cache.
  2. `GetById_MissingBook_ReturnsNotFoundWithoutCaching`: Đảm bảo không cache dữ liệu rác/lỗi 404.
  3. `GetById_CachedJsonNull_ReloadsBook`: Tự phục hồi khi cache bị lỗi dữ liệu rỗng.
  4. `UpdateBook_InvalidatesCache_AndReloadsUpdatedDetails`: Cập nhật sách làm mới cache.
  5. `DeleteBook_InvalidatesCache_AndDoesNotReturnDeletedBook`: Xóa sách xóa luôn cache.
  6. `CreateAndDeleteCopy_RefreshesCopyCounts`: Thêm/xóa bản sao sách cập nhật đúng số lượng tồn trong cache.
  7. `BorrowAndReturnBooks_RefreshesAffectedBooksOnly`: Mượn và trả sách chỉ làm mới đúng những cuốn sách bị ảnh hưởng, giữ nguyên cache của cuốn khác.
  8. `MarkLostBook_InvalidatesCache`: Báo mất sách cập nhật số lượng tồn khả dụng trong cache.
  9. `UpdateCategory_InvalidatesAllBooksInCategory`: Đổi tên thể loại làm mới cache tất cả sách thuộc thể loại đó.
- **Tính linh hoạt:** Bộ test chạy mặc định trên SQLite In-Memory + Memory Cache. Đồng thời hỗ trợ biến môi trường `LIGHTBRARY_TEST_REDIS` để kiểm thử trực tiếp trên Redis thật!

#### 3. Đóng gói Container hóa với Docker & Docker Compose
- **Multi-stage Dockerfile:** Giai đoạn build sử dụng `mcr.microsoft.com/dotnet/sdk:10.0`, giai đoạn chạy sử dụng `mcr.microsoft.com/dotnet/aspnet:10.0` chạy dưới quyền người dùng không phải root (`USER $APP_UID`) để tăng cường bảo mật.
- **Docker Compose (`compose.yml`):** Điều phối 3 containers (`api`, `postgres:17-alpine`, `redis:7-alpine`) với cấu hình `healthcheck` và `depends_on: condition: service_healthy` giúp API chỉ khởi động khi CSDL và Redis đã hoàn toàn sẵn sàng nhận kết nối.

#### 4. Tích hợp liên tục (CI với GitHub Actions)
- File cấu hình: `.github/workflows/ci.yml`.
- Tự động chạy quy trình kiểm tra mã nguồn mỗi khi có commit hoặc pull request: Cài đặt .NET 10 $\rightarrow$ `dotnet restore` $\rightarrow$ `dotnet build` $\rightarrow$ `dotnet test`.

---

## 📑 PHẦN 3: BẢNG PHỦ KỸ THUẬT (TECHNICAL COVERAGE TABLE)

Bảng đối chiếu toàn diện từng yêu cầu trong barem của Giảng viên với các file hiện thực cụ thể trong mã nguồn:

| Hạng mục trong Barem của Thầy | Mức độ | Vị trí File trong Mã Nguồn | Mô tả hiện thực cụ thể |
| :--- | :---: | :--- | :--- |
| **Pipeline & Thứ tự Middleware** | Bắt buộc | `back_end/light_brary/Program.cs` (dòng 89–98) | Thiết lập thứ tự: ExceptionHandling $\rightarrow$ HTTPS $\rightarrow$ CORS $\rightarrow$ Auth $\rightarrow$ ValidateUser $\rightarrow$ Authorize |
| **Custom Middleware: Xử lý lỗi** | Bắt buộc | `back_end/light_brary/code/Middleware/ExceptionHandlingMiddleware.cs` | Bắt `AppException`, `DbUpdateException`, trả JSON đồng nhất `{ status, code, message }` |
| **Custom Middleware: Bảo mật phiên** | Tự chọn | `back_end/light_brary/code/Middleware/ValidateUserMiddleware.cs` | Kiểm tra tức thời `SecurityStamp` và `IsLockedOutAsync` từ CSDL |
| **Routing & Route Constraints** | Bắt buộc | `back_end/light_brary/code/Controllers/` | Attribute Routing kèm constraint: `[HttpGet("{id:guid}")]` chặn UUID sai định dạng |
| **Model Binding & Data Annotations** | Bắt buộc | `back_end/light_brary/code/DTOs/Requests/` | `[Required]`, `[StringLength]`, `[Range]` trên 9 Request DTOs, tự động trả `400 Bad Request` |
| **Dependency Injection (Scoped)** | Bắt buộc | `back_end/light_brary/Program.cs` (dòng 50–65) | Đăng ký 7 Services và 8 Repositories theo vòng đời `Scoped` |
| **EF Core DbContext & Quan hệ** | Bắt buộc | `back_end/light_brary/code/Data/AppDbContext.cs` | Cấu hình quan hệ 1-N, Composite PK, Unique Phone, kế thừa `IdentityDbContext` |
| **EF Core Migrations** | Bắt buộc | `back_end/light_brary/Migrations/` | Migration `20261022030516_chantroimoi`, tự động chạy `MigrateAsync()` khi khởi động |
| **Tối ưu N+1 Query & Eager Loading** | Bắt buộc | `back_end/light_brary/code/Repositories/BookRepository.cs` | Dùng `.Include(b => b.Category).Include(b => b.BookCopies)` gom về 1 câu SQL JOIN |
| **AsNoTracking() tối ưu đọc** | Tự chọn | `back_end/light_brary/code/Repositories/GenericRepository.cs` | Áp dụng `AsNoTracking()` cho toàn bộ truy vấn chỉ đọc |
| **Quản lý Cấu hình & Secrets** | Bắt buộc | `appsettings.json`, `appsettings.Development.json` | Tách môi trường, cấu hình ConnectionStrings, User Secrets cho local dev |
| **Identity API & Bearer Token** | Tự chọn | `Program.cs` (dòng 67–76), `Services/AuthService.cs` | `AddIdentityApiEndpoints<User>`, xác thực qua Data Protection Bearer Token |
| **Policy-Based Authorization** | Tự chọn | `Program.cs` (dòng 43–47), `Controllers/` | 2 Policy: `"AdminOnly"` và `"LibrarianOrAdmin"`, gắn qua `[Authorize(Policy = "...")]` |
| **Tự động khóa tài khoản (Lockout)** | Tự chọn | `Program.cs`, `Data/DataSeeder.cs` | Khóa tài khoản sau 5 lần nhập sai trong 15 phút (`LockoutEnabled = true`) |
| **Health Checks** | Tự chọn | `Program.cs` (dòng 13, 98) | Endpoint `/health` phục vụ Docker container healthcheck |
| **CORS Policy** | Tự chọn | `Program.cs` (dòng 16–25, 91) | Policy `"AllowFrontend"` cho phép localhost:3000, localhost:5173 gửi credentials |
| **Redis Distributed Cache-Aside** | Nâng cao | `Program.cs` (dòng 36–41), `Services/BookService.cs` | Cache chi tiết sách `books:{id}`, TTL 10 phút, kiểm tra Cache-Hit/Miss |
| **Cache Invalidation đa tầng** | Nâng cao | `Services/BookCopyService.cs`, `BorrowRecordService.cs`, `CategoryService.cs` | Tự động xóa cache khi mượn, trả, báo mất, thêm/xóa bản sao, đổi thể loại |
| **Automated Testing với xUnit** | Nâng cao | `back_end/light_brary.Tests/BookCacheTests.cs` | 9 unit test phủ 100% các tình huống hit/miss/invalidation của cache |
| **Multi-stage Docker & Compose** | Nâng cao | `Dockerfile`, `compose.yml` | Build multi-stage, chạy non-root user, điều phối 3 services có dependency healthcheck |
| **CI với GitHub Actions** | Nâng cao | `.github/workflows/ci.yml` | Tự động restore, build và test trên môi trường CI GitHub |

---

## 🚀 PHẦN 4: CÁC CÁCH CHẠY HỆ THỐNG

Dự án hỗ trợ 3 cách chạy linh hoạt:

### Cách 1: Chạy toàn bộ bằng Docker Compose (Khuyên dùng nhất — 1 lệnh duy nhất)
Thích hợp nhất khi nộp bài, demo hoặc chạy trên máy người khác mà không cần cài đặt .NET SDK hay PostgreSQL:
```bash
docker compose up --build -d
```
- API chạy tại: `http://localhost:5218`
- Swagger UI tra cứu API: `http://localhost:5218/swagger`
- Kiểm tra trạng thái: `curl http://localhost:5218/health`
- Dừng hệ thống: `docker compose down` (nếu muốn xóa sạch dữ liệu để test lại từ đầu: `docker compose down -v`).

### Cách 2: Chạy kết hợp (Database & Redis trên Docker, Backend chạy trực tiếp)
Thích hợp cho lập trình viên phát triển Backend và đặt Breakpoint gỡ lỗi trong Visual Studio / VS Code:
1. Bật CSDL và Redis:
   ```bash
   docker compose up -d postgres redis
   ```
2. Chạy Backend bằng .NET CLI từ thư mục gốc:
   ```bash
   dotnet restore Library.slnx
   dotnet run --project back_end/light_brary
   ```
   *(Hệ thống sẽ tự động chạy Migration và Seed dữ liệu mẫu nếu DB trống).*

### Cách 3: Chạy Front-end (Nếu muốn mở giao diện Web để demo)
Mở một cửa sổ Terminal mới:
```bash
cd front_end
npm ci
npm run dev
```
- Giao diện web chạy tại: `http://localhost:3000` (đã cấu hình proxy tự động chuyển tiếp request `/api` sang Backend `http://localhost:5218`).

### Cách chạy Bộ Kiểm Thử Tự Động (Automated Tests):
Từ thư mục gốc dự án:
```bash
dotnet test Library.slnx
```
*(Nếu muốn test với Redis thật đang chạy trên Docker:* `LIGHTBRARY_TEST_REDIS=localhost:6381 dotnet test Library.slnx`*).*

---

## 🔑 TÀI KHOẢN MẪU KHỞI TẠO SẴN TRONG HỆ THỐNG

Khi khởi động lần đầu trong môi trường `Development`, hệ thống tự động sinh dữ liệu thực tế:
- **Tài khoản Quản trị viên (Admin):**
  - Tên đăng nhập: `0999999999` (hoặc biến `Seed:AdminUserName`)
  - Mật khẩu: `change-me-admin`
- **Tài khoản Thủ thư (Staff / Librarian):**
  - Tên đăng nhập: `thuthu`
  - Mật khẩu: `change-me-admin`
- **Tài khoản Sinh viên (Member / User):**
  - Gồm **20 sinh viên**, tên đăng nhập từ `3124560001` đến `3124560020`
  - Mật khẩu: `change-me-admin`
- **Dữ liệu kho sách:** 5 thể loại, 30 đầu sách có thật (kèm ISBN, ảnh bìa OpenLibrary), và 75 bản sao vật lý gắn mã barcode sẵn sàng cho mượn.

---

## 📋 PHẦN 5: CHỨC NĂNG HỆ THỐNG & DANH SÁCH 37 API NGHIỆP VỤ (+ 1 HEALTHCHECK)

Hệ thống được chia thành 6 phân hệ quản lý với đúng **37 API nghiệp vụ** và **1 endpoint giám sát hệ thống**:

### 1. Nhóm Xác thực & Tài khoản (`AuthController` — `/api/auth`)
| Method | Endpoint | Quyền hạn | Chức năng nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Công khai | Đăng nhập bằng mã sinh viên/username và mật khẩu; nhận Bearer Token |
| `PUT` | `/api/auth/password` | Đã đăng nhập | Đổi mật khẩu cá nhân; cập nhật Security Stamp |
| `PUT` | `/api/auth/phone-number` | Đã đăng nhập | Cập nhật số điện thoại liên hệ cá nhân (kiểm tra trùng lặp) |
| `PUT` | `/api/auth/logout` | Đã đăng nhập | Đăng xuất; làm mới Security Stamp để hủy hiệu lực mọi token cũ |

### 2. Nhóm Người dùng & Bạn đọc (`UsersController` — `/api/...`)
| Method | Endpoint | Quyền hạn | Chức năng nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/me` | Đã đăng nhập | Xem thông tin hồ sơ của chính mình kèm danh sách Roles |
| `PUT` | `/api/me` | Đã đăng nhập | Cập nhật địa chỉ liên hệ của chính mình |
| `GET` | `/api/me/borrow-records` | Đã đăng nhập | Xem toàn bộ lịch sử các phiếu mượn sách của chính mình |
| `GET` | `/api/users` | Thủ thư / Admin | Tra cứu danh sách người dùng (lọc theo từ khóa `keyword`, vai trò `role`) |
| `GET` | `/api/users/{id}` | Thủ thư / Admin | Xem thông tin chi tiết của một người dùng theo ID |
| `GET` | `/api/users/{id}/borrow-records`| Thủ thư / Admin | Tra cứu lịch sử mượn trả sách của một người dùng cụ thể |
| `DELETE` | `/api/users/{id}` | Chỉ Admin | Xóa tài khoản người dùng khỏi hệ thống |

### 3. Nhóm Phân quyền & Vai trò (`RolesController` — `/api/...`)
| Method | Endpoint | Quyền hạn | Chức năng nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/roles` | Chỉ Admin | Xem danh mục các vai trò trong hệ thống (`Admin`, `Librarian`, `User`) |
| `GET` | `/api/users/{userId}/roles` | Chỉ Admin | Xem danh sách các vai trò đã được gán cho một người dùng |
| `POST` | `/api/users/{userId}/roles/{roleId}` | Chỉ Admin | Gán một vai trò cho người dùng |
| `DELETE` | `/api/users/{userId}/roles/{roleId}`| Chỉ Admin | Gỡ bỏ vai trò khỏi người dùng |

### 4. Nhóm Thể loại sách (`CategoriesController` — `/api/categories`)
| Method | Endpoint | Quyền hạn | Chức năng nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/categories` | Công khai | Lấy toàn bộ danh sách thể loại sách |
| `GET` | `/api/categories/{id}` | Công khai | Xem thông tin chi tiết một thể loại theo ID |
| `POST` | `/api/categories` | Thủ thư / Admin | Thêm mới một thể loại sách |
| `PUT` | `/api/categories/{id}` | Thủ thư / Admin | Cập nhật tên/mô tả thể loại (tự động xóa cache sách liên quan) |
| `DELETE` | `/api/categories/{id}` | Chỉ Admin | Xóa thể loại (chặn xóa nếu còn sách thuộc thể loại) |

### 5. Nhóm Đầu sách & Bản sao vật lý (`BooksController` & `BookCopiesController`)
| Method | Endpoint | Quyền hạn | Chức năng nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/books` | Công khai | Phân trang, tìm kiếm và lọc danh mục sách (`keyword`, `author`, `categoryId`, `onlyAvailable`, `sortBy`) |
| `GET` | `/api/books/{id}` | Công khai | Xem chi tiết sách (**có áp dụng Redis Cache**, TTL 10 phút) |
| `POST` | `/api/books` | Thủ thư / Admin | Tạo đầu sách mới |
| `PUT` | `/api/books/{id}` | Thủ thư / Admin | Cập nhật thông tin đầu sách (**xóa cache Redis**) |
| `DELETE` | `/api/books/{id}` | Chỉ Admin | Xóa đầu sách nếu không còn bản sao nào (**xóa cache Redis**) |
| `GET` | `/api/book-copies/all` | Thủ thư / Admin | Xem toàn bộ danh sách các bản sao sách trong kho |
| `GET` | `/api/books/{bookId}/copies` | Công khai | Xem danh sách các bản sao vật lý của một đầu sách cụ thể |
| `POST` | `/api/books/{bookId}/copies` | Thủ thư / Admin | Thêm bản sao vật lý mới cho đầu sách (**xóa cache Redis**) |
| `GET` | `/api/book-copies/{id}` | Thủ thư / Admin | Xem chi tiết một bản sao sách |
| `GET` | `/api/book-copies/by-barcode/{barcode}` | Thủ thư / Admin | Tra cứu nhanh bản sao sách qua mã vạch Barcode (quét mã) |
| `PUT` | `/api/book-copies/{id}` | Thủ thư / Admin | Cập nhật mã barcode hoặc tình trạng ghi chú của bản sao |
| `DELETE` | `/api/book-copies/{id}` | Chỉ Admin | Xóa bản sao sách khỏi kệ (**xóa cache Redis**) |

### 6. Nhóm Mượn, Trả & Báo mất sách (`BorrowRecordsController`)
| Method | Endpoint | Quyền hạn | Chức năng nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/borrow-records` | Thủ thư / Admin | Lập phiếu mượn nhiều cuốn sách cho độc giả; chuyển trạng thái bản sao sang `Borrowed` (**xóa cache các sách liên quan**) |
| `GET` | `/api/borrow-records` | Thủ thư / Admin | Lấy danh sách phiếu mượn (lọc theo `userId`, `status`, `overdue`) |
| `GET` | `/api/borrow-records/{id}` | Đã đăng nhập | Xem chi tiết phiếu mượn (Member chỉ xem được phiếu của mình; Staff/Admin xem được tất cả) |
| `POST` | `/api/borrow-records/{id}/returns` | Thủ thư / Admin | Trả một hoặc nhiều cuốn sách; chuyển trạng thái bản sao về `Available`, tính lại trạng thái phiếu (**xóa cache sách liên quan**) |
| `POST` | `/api/borrow-details/{id}/mark-lost` | Thủ thư / Admin | Báo mất một cuốn sách đang mượn; chuyển trạng thái sang `Lost`, tính lại trạng thái phiếu (**xóa cache sách liên quan**) |

### 7. Nhóm Vận hành & Giám sát (`HealthCheck`)
| Method | Endpoint | Quyền hạn | Chức năng nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `GET` | `/health` | Công khai | Trả về chuỗi `Healthy` cho Docker và load balancer kiểm tra trạng thái sống của API |

---

## 🔄 PHẦN 6: LUỒNG DỮ LIỆU ĐIỂN HÌNH (DATA FLOW EXAMPLES)

### Ví dụ 1: Luồng đọc chi tiết sách có Cache (`GET /api/books/{id}`)
```text
[Client]
   │
   ▼ 1. Gửi HTTP GET /api/books/{id}
[ExceptionHandlingMiddleware] (Mở try/catch)
   │
   ▼ 2. Cho phép qua vì không cần xác thực
[BooksController.GetById(id)]
   │
   ▼ 3. Gọi BookService.GetByIdAsync(id)
[BookService]
   ├── 4. Gọi IDistributedCache.GetStringAsync("books:{id}")
   │        ├── [TRƯỜNG HỢP CACHE-HIT]: Tìm thấy JSON trong Redis
   │        │     └── Deserialize JSON -> trả thẳng về Client (độ trễ ~1ms, 0 query SQL)
   │        └── [TRƯỜNG HỢP CACHE-MISS]: Chưa có trong Redis
   │              ├── 5. Gọi BookRepository.GetWithDetailsAsync(id)
   │              │        └── EF Core sinh 1 câu SQL JOIN Books + Categories + BookCopies
   │              ├── 6. Lưu kết quả vào Redis với TTL = 10 phút
   │              └── 7. Trả BookResponse về Client (độ trễ ~4ms)
```

### Ví dụ 2: Luồng lập phiếu mượn sách (`POST /api/borrow-records`)
```text
[Client: Thủ thư]
   │
   ▼ 1. Gửi POST /api/borrow-records (Kèm Bearer Token và danh sách BookCopyIds)
[Authentication Middleware] -> Giải mã Bearer Token -> gán Claims vào HttpContext.User
   │
   ▼ 2. ValidateUserMiddleware -> Kiểm tra Security Stamp & Trạng thái khóa tài khoản trong DB
   │
   ▼ 3. Authorization Middleware -> Kiểm tra Policy "LibrarianOrAdmin" -> Hợp lệ
   │
   ▼ 4. BorrowRecordsController.Create(request)
   │
   ▼ 5. BorrowRecordService.CreateAsync(request)
   ├── Kiểm tra độc giả có tồn tại không (UserService)
   ├── Kiểm tra hạn trả DueDate phải lớn hơn thời điểm hiện tại
   ├── Đọc danh sách bản sao (BookCopyRepository)
   │     └── Nếu có bản sao không ở trạng thái Available -> Throw AppException.Conflict
   ├── Đổi trạng thái các bản sao thành Borrowed
   ├── Tạo thực thể BorrowRecord và các BorrowDetail tương ứng
   ├── Gọi borrowRecordRepository.AddAsync(record) -> EF Core lưu vào CSDL
   ├── Invalidation: Xóa cache Redis của tất cả các BookId liên quan: cache.RemoveAsync("books:{bookId}")
   └── Trả về CreatedAtAction (HTTP 201 Created) kèm BorrowRecordResponse
```

---

## ⚖️ PHẦN 7: CÁI NÀO TỰ VIẾT VS CÁI NÀO DÙNG CỦA FRAMEWORK?

Bảng đối chiếu cụ thể giúp thành viên trả lời tự tin trước hội đồng chấm:

| Thành phần trong dự án | Tự viết (Custom Code) | Dùng sẵn của ASP.NET Core / Thư viện |
| :--- | :--- | :--- |
| **Định tuyến & Tiếp nhận Request** | Khai báo các Controller, Route template, Action methods, DTO Requests/Responses | ASP.NET Core Routing Engine, Model Binding, Data Annotations Validator |
| **Xử lý Ngoại lệ (Exceptions)** | `ExceptionHandlingMiddleware`, lớp ngoại lệ `AppException` với các mã lỗi đặc thù | Cơ chế Middleware Pipeline (`InvokeAsync`), `ILogger<T>` ghi log |
| **Kiểm tra phiên đăng nhập** | `ValidateUserMiddleware` (kiểm tra tức thì SecurityStamp và Lockout trong DB) | `SignInManager<User>`, `UserManager<User>` của Identity |
| **Xác thực & Phân quyền** | Khai báo các Policy (`AdminOnly`, `LibrarianOrAdmin`), gán `[Authorize]` trên Controller | `AddIdentityApiEndpoints`, Bearer Token Handler, Data Protection Provider, Authorization Middleware |
| **Tầng Nghiệp vụ (Business)** | Toàn bộ 6 Services (`BookService`, `BorrowRecordService`, ...) chứa logic mượn, trả, tính trạng thái phiếu, phạt mất | Không có sẵn (đây là logic nghiệp vụ riêng của đề tài) |
| **Truy cập Dữ liệu (Data)** | `GenericRepository`, 7 Repositories cụ thể, cấu hình quan hệ bảng trong Entities | EF Core DbContext, ChangeTracker, LINQ Provider, Npgsql PostgreSQL Provider |
| **Bộ nhớ đệm (Caching)** | Logic áp dụng Cache-Aside, thuật toán vô hiệu hóa cache (Invalidation) trong Service | Thư viện `StackExchangeRedis`, interface `IDistributedCache` |
| **Dữ liệu ban đầu (Seeding)** | Thuật toán tạo 20 sinh viên, admin, thủ thư, sách và bản sao trong `LibraryDataSeeder` | Cơ chế Migration của EF Core (`Database.MigrateAsync()`) |
| **Đóng gói & Kiểm thử** | Viết `Dockerfile`, `compose.yml`, viết 9 ca kiểm thử trong `BookCacheTests.cs` | Docker Runtime, Engine xUnit, SQLite In-Memory Provider |

---

## 🔍 PHẦN 8: SO SÁNH VÀ PHẢN BIỆN (CHO PHẦN TỔNG QUAN BÁO CÁO)

Tài liệu yêu cầu của Thầy (trang 16) yêu cầu nhóm phải so sánh với các công nghệ khác và nêu rõ khi nào KHÔNG nên dùng ASP.NET Core. Hai bạn T & YNhi dùng nội dung này để đưa vào **Chương Tổng Quan** của báo cáo:

### 1. So sánh ASP.NET Core với các công nghệ Backend tương đương

| Tiêu chí | ASP.NET Core (.NET 10) | Spring Boot (Java) | Express.js / NestJS (Node.js) | Go + Gin |
| :--- | :--- | :--- | :--- | :--- |
| **Kiến trúc & Triết lý** | Middleware Pipeline chuẩn, DI tích hợp sẵn, kiểu dữ liệu chặt chẽ (Type-safe). | Kiến trúc phân tầng tương tự (DI, IoC container, Annotations phong phú). | Single-threaded Event Loop, bất đồng bộ non-blocking I/O. NestJS mô phỏng Angular. | Tối giản, cấu trúc phẳng, Goroutine xử lý đồng thời siêu nhẹ. |
| **Hiệu năng & Tốc độ** | Rất cao (nằm trong top đầu TechEmpower benchmarks). | Khá cao, nhưng tốn RAM và thời gian khởi động (JVM warmup) lâu hơn. | Trung bình khá; xử lý I/O tốt nhưng tính toán CPU nặng bị nghẽn Event Loop. | Cực cao, tiêu thụ ít tài nguyên RAM/CPU, khởi động tức thì. |
| **Hệ sinh thái & Công cụ** | Visual Studio, C# Dev Kit, EF Core, Identity dựng sẵn hoàn hảo. | Spring Data JPA, Spring Security, Maven/Gradle cực kỳ khổng lồ. | npm phong phú nhất, nhưng dễ gặp xung đột package ("dependency hell"). | Hệ sinh thái thư viện chuẩn mạnh, ít thư viện "all-in-one" như EF Core. |
| **Lý do nhóm chọn ASP.NET Core** | Cú pháp C# hiện đại, công cụ EF Core và Identity được tích hợp chặt chẽ, hiệu năng cao và có middleware pipeline rõ ràng theo barem môn học. |

### 2. Hai trường hợp KHÔNG NÊN dùng ASP.NET Core (Dựa trên trải nghiệm thực tế của nhóm)

1. **Dịch vụ siêu nhỏ (Microservice dạng Serverless/Function) chỉ có 1-2 endpoint:**
   - *Lý do:* Mặc dù .NET có Minimal API, nhưng kích thước file binary và memory footprint ban đầu của .NET runtime (khoảng 30-50MB RAM) vẫn lớn hơn nhiều so với viết bằng Go hoặc Python. Go chỉ tạo ra 1 file thực thi độc lập vài megabyte và tốn vài MB RAM, thích hợp hơn cho các tác vụ serverless khởi động nhanh (cold start nhỏ).
2. **Đội ngũ lập trình viên chỉ quen JavaScript/Python hoặc dự án cần MVP cực nhanh trong vài ngày:**
   - *Lý do:* ASP.NET Core yêu cầu hiểu biết sâu về hệ thống kiểu dữ liệu tĩnh (Static Typing), cơ chế DI Lifetime (Transient/Scoped/Singleton), cấu hình DbContext và quan hệ khóa ngoại chặt chẽ. Nếu làm ứng dụng cần thay đổi schema dữ liệu liên tục không cố định, FastAPI hoặc Express kết hợp MongoDB sẽ linh hoạt hơn.

---

## 🛠️ PHẦN 9: HƯỚNG DẪN 3 BÀI THỰC HÀNH TẠI CHỖ (CHUẨN BỊ THI VẤN ĐÁP)

Thầy giáo đưa ra 3 ví dụ bài thực hành tại chỗ trực tiếp từ mã nguồn đồ án (trang 16 trong PDF). Dưới đây là hướng dẫn giải chi tiết để nhóm tự tin xử lý khi Thầy yêu cầu trực tiếp tại máy:

### Ca 1: Chèn một Middleware mới và chứng minh vị trí đặt là đúng
- **Đề bài ví dụ:** *"Hãy thêm một middleware đo thời gian xử lý request (Performance Logging) hoặc thêm Header bản quyền `X-Powered-By: LightBrary` vào mọi response."*
- **Cách làm:**
  1. Trong `Program.cs`, chèn đoạn middleware inline bằng `app.Use(...)`:
     ```csharp
     app.Use(async (context, next) =>
     {
         context.Response.Headers.Append("X-Powered-By", "LightBrary-Team16");
         var stopwatch = System.Diagnostics.Stopwatch.StartNew();
         await next();
         stopwatch.Stop();
         Console.WriteLine($"[PERF] {context.Request.Method} {context.Request.Path} executed in {stopwatch.ElapsedMilliseconds}ms");
     });
     ```
  2. **Giải thích vị trí đặt:** Đặt ngay **sau** `UseExceptionHandlingMiddleware` và **trước** `UseAuthentication`.  
     *Lý do:* Cần đặt sau middleware bắt lỗi để nếu có exception thì vẫn đo được thời gian và header vẫn được append; đồng thời đặt trước authentication để đo được toàn bộ thời gian của cả quá trình xác thực, phân quyền và chạy controller.

### Ca 2: Thêm một Policy Authorization cho endpoint có sẵn
- **Đề bài ví dụ:** *"Hãy tạo thêm một Policy tên `MemberOnly` chỉ cho phép người dùng có Role `User` truy cập và gắn vào endpoint xem lịch sử mượn cá nhân."*
- **Cách làm:**
  1. Trong `Program.cs`, thêm policy vào `AddAuthorization`:
     ```csharp
     builder.Services.AddAuthorization(options =>
     {
         options.AddPolicy("AdminOnly", p => p.RequireRole(RoleNames.ADMIN));
         options.AddPolicy("LibrarianOrAdmin", p => p.RequireRole(RoleNames.ADMIN, RoleNames.LIBRARIAN));
         options.AddPolicy("MemberOnly", p => p.RequireRole(RoleNames.USER)); // MỚI
     });
     ```
  2. Mở `UsersController.cs`, gắn attribute lên endpoint:
     ```csharp
     [HttpGet("api/me/borrow-records")]
     [Authorize(Policy = "MemberOnly")] // Thay cho [Authorize] chung chung
     public async Task<ActionResult<IEnumerable<BorrowRecordSummaryResponse>>> GetMyBorrowRecords()
     ```
  3. **Chứng minh:** Dùng token của tài khoản Thủ thư (`thuthu`) gọi vào endpoint này $\rightarrow$ Server trả về `403 Forbidden` vì thủ thư không có Role `User`. Dùng token của sinh viên (`3124560001`) gọi vào $\rightarrow$ Trả về `200 OK`.

### Ca 3: Thêm Migration và cập nhật API theo schema mới
- **Đề bài ví dụ:** *"Thêm thuộc tính `Publisher` (Nhà xuất bản, string, tối đa 100 ký tự, cho phép null) vào thực thể Book, tạo migration và cập nhật API."*
- **Cách làm theo đúng 4 bước:**
  1. **Bước 1 — Sửa Entity:** Mở `code/Entities/Book.cs`, thêm:
     ```csharp
     [MaxLength(100)]
     public string? Publisher { get; set; }
     ```
  2. **Bước 2 — Sửa DTOs:** Thêm `Publisher` vào `SaveBookRequest.cs` và `BookResponse.cs`.
  3. **Bước 3 — Tạo Migration:** Mở terminal chạy lệnh:
     ```bash
     dotnet ef migrations add AddPublisherToBook --project back_end/light_brary --startup-project back_end/light_brary
     dotnet ef database update --project back_end/light_brary --startup-project back_end/light_brary
     ```
  4. **Bước 4 — Cập nhật Service:** Trong `BookService.cs`, gán `book.Publisher = request.Publisher` trong `CreateAsync` và `UpdateAsync`.
  5. **Kiểm chứng:** Gọi `POST /api/books` với payload có `"publisher": "NXB Trẻ"`, sau đó gọi `GET /api/books/{id}` kiểm tra trường publisher trả về chính xác.

---

## 🎓 PHẦN 10: BỘ 4 CÂU HỎI TỰ KIỂM & LỜI GIẢI VẤN ĐÁP CỦA THẦY

Đây là 4 câu hỏi trọng tâm xuất hiện trực tiếp trong tài liệu hướng dẫn của giảng viên:

### Câu 1: Thứ tự các middleware trong pipeline của em có ý nghĩa gì? Đảo hai cái thì sao?
- **Trả lời:** Pipeline hoạt động theo cơ chế lồng nhau hai chiều (mô hình Russian Doll / củ hành).
  - `ExceptionHandlingMiddleware` bắt buộc phải đặt ở **đầu ngoài cùng** để khối `try/catch` có thể bao bọc toàn bộ các middleware phía sau, bất kỳ lỗi nào phát sinh từ Controller hay Middleware khác đều được chuyển thành JSON chuẩn.
  - `UseAuthentication()` phải đứng **trước** `ValidateUserMiddleware` và `UseAuthorization()`. Nếu đảo `UseAuthorization()` lên trước, `context.User` chưa được giải mã từ Bearer token $\rightarrow$ người dùng luôn bị xem là chưa đăng nhập và bị từ chối với mã `401 Unauthorized`.
  - `ValidateUserMiddleware` nằm giữa `UseAuthentication()` và `UseAuthorization()` để kiểm tra trạng thái khóa tài khoản và Security Stamp của người dùng trong CSDL ngay sau khi nhận diện danh tính và trước khi xét duyệt quyền.

### Câu 2: Service này nên Scoped hay Singleton? Vì sao?
- **Trả lời:** Tất cả các Service trong dự án đều phải đăng ký là **Scoped**.
  - **Lý do:** Các Service cần inject `AppDbContext` (hoặc các Repository sử dụng `AppDbContext`) để thao tác với cơ sở dữ liệu. Bản thân `AppDbContext` được thiết kế có vòng đời Scoped (tạo ra khi bắt đầu request và giải phóng khi kết thúc request để giải phóng kết nối CSDL).
  - Nếu đăng ký Service là **Singleton**: Sẽ vi phạm nguyên tắc và dẫn đến lỗi **Captive Dependency** (một đối tượng sống lâu Singleton giữ tham chiếu đến một đối tượng sống ngắn Scoped). Khi đó `AppDbContext` sẽ không bao giờ được giải phóng, gây rò rỉ bộ nhớ, và gây lỗi sập ứng dụng khi có 2 request đồng thời gọi vào cùng một Service do `DbContext` của EF Core không an toàn cho đa luồng (not thread-safe).

### Câu 3: Authorization của endpoint này được quyết định ở đâu?
- **Trả lời:** Quyết định phân quyền diễn ra tại **`AuthorizationMiddleware`** (được đăng ký bằng lệnh `app.UseAuthorization()`).
  - Middleware này chạy sau `AuthenticationMiddleware`. Nó đọc thông tin `ClaimsPrincipal` trong `context.User`, đối chiếu với thuộc tính `[Authorize(Policy = "...")]` hoặc `[Authorize(Roles = "...")]` đặt trên Controller/Action.
  - Nếu `context.User` không thỏa mãn yêu cầu của Policy, `AuthorizationMiddleware` sẽ lập tức ngắt pipeline và trả về mã HTTP `403 Forbidden` (hoặc `401 Unauthorized` nếu chưa đăng nhập), request hoàn toàn không chạm được tới tầng Controller.

### Câu 4: Điều gì xảy ra với DbContext sau mỗi request?
- **Trả lời:** Do `AppDbContext` được đăng ký với vòng đời **Scoped**, một thể hiện (instance) của `AppDbContext` được tạo ra thông qua DI Container khi request bắt đầu chạm tới tầng cần dữ liệu.
  - Trong suốt request đó, cùng một instance `DbContext` được chia sẻ giữa các Service và Repository.
  - Khi request kết thúc và response được gửi về client, ASP.NET Core sẽ tự động hủy (Dispose) phạm vi Scoped đó (`IServiceScope`), đồng thời gọi hàm `DisposeAsync()` trên `AppDbContext`. Kết nối CSDL được trả lại cho Connection Pool của Npgsql, và toàn bộ các đối tượng đang được theo dõi trong `ChangeTracker` của DbContext sẽ được giải phóng khỏi bộ nhớ RAM.

---
*(Bản tóm tắt hoàn tất và sẵn sàng làm tài liệu tham chiếu gốc cho báo cáo của T & YNhi).*
