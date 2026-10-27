# Luồng hoạt động của LightBrary

Tài liệu giải thích cách backend xử lý request và các nhóm API theo mã nguồn hiện tại. Xem [hướng dẫn Swagger](THIẾT%20KẾ%20API.md) để tra cứu endpoint và request/response; xem [database](database.md) để hiểu các bảng và quan hệ.

## 1. Khi ứng dụng khởi động

`Program.cs` tạo ứng dụng ASP.NET Core, đọc cấu hình và đăng ký các thành phần: controller, DbContext, Identity, Redis cache, service, repository, CORS và Swagger. Sau đó ứng dụng cấu hình middleware và ánh xạ controller.

Trước khi phục vụ request, ứng dụng dùng EF Core áp dụng migration, rồi chạy seeder để tạo role, admin và dữ liệu mẫu theo cấu hình. Seeder có transaction; dữ liệu PostgreSQL được giữ lại giữa các lần chạy. Việc khởi động cần kết nối database và cấu hình mật khẩu admin khi phải tạo admin mới.

## 2. Luồng chung của một request

```text
Frontend / Swagger / HTTP client
              │ HTTP request
              ▼
ASP.NET Core: routing và middleware
  → xử lý lỗi, HTTPS, CORS
  → xác thực → kiểm tra người dùng → phân quyền
              │
              ▼
Controller → Service → Repository hoặc Identity → EF Core → PostgreSQL
                  │
                  └── Redis (một số thao tác liên quan chi tiết sách)
              │
              ▼
         HTTP response (thường là JSON)
```

1. **Client gửi request:** frontend dùng `fetch` qua `front_end/src/api.ts`; trình duyệt hoặc HTTP client cũng có thể gọi API trực tiếp. Với endpoint cần đăng nhập, client gửi access token trong header `Authorization: Bearer ...`.
2. **Middleware xử lý:** `ExceptionHandlingMiddleware` bắt exception từ các bước phía sau; ASP.NET Core xử lý chuyển hướng HTTPS, CORS và authentication. `ValidateUserMiddleware` tự viết kiểm tra security stamp và trạng thái khóa của user đã xác thực tại endpoint được bảo vệ. Authorization kiểm tra `[Authorize]` và các policy. Endpoint công khai không yêu cầu đăng nhập. Swagger được cấu hình riêng trong môi trường Development.
3. **Controller nhận dữ liệu:** ASP.NET Core chọn route, đưa dữ liệu URL, query và JSON vào tham số/DTO. `[ApiController]` kết hợp Data Annotations kiểm tra các trường đầu vào và có thể trả `400` trước khi gọi action. Controller gọi service, sau đó chọn HTTP status và response.
4. **Service xử lý nghiệp vụ:** kiểm tra điều kiện, gọi repository/Identity, thay đổi dữ liệu, chuyển entity thành response DTO và xóa cache khi cần. Không phải service nào cũng chỉ dùng repository: một số thao tác dùng `DbContext` hoặc Identity trực tiếp.
5. **Truy cập dữ liệu:** repository dùng EF Core và LINQ để đọc hoặc cập nhật PostgreSQL. `DbContext` theo dõi thay đổi; `SaveChangesAsync` ghi dữ liệu. Identity cũng dùng EF Core store để làm việc với bảng người dùng và vai trò.
6. **Trả kết quả:** controller thường trả JSON với `200`, `201` hoặc `204`. Exception nghiệp vụ do middleware chuyển thành JSON gồm `status`, `code`, `message`; lỗi validation của framework và lỗi xác thực/phân quyền có thể có định dạng khác.

Middleware hoạt động theo chuỗi: khi component gọi bước kế tiếp, request đi vào sâu hơn; sau khi bước đó hoàn thành, response đi ngược qua các component đã gọi. Tùy request, một middleware có thể trả response sớm và controller không được gọi.

## 3. Phần tự viết và thư viện có sẵn

| Thành phần | Dùng ở đâu | Vai trò |
|---|---|---|
| ASP.NET Core | `Program.cs`, controller, DTO | Framework cung cấp HTTP pipeline, routing, DI, model binding, validation, authentication, authorization và JSON. Dự án tự cấu hình và viết controller, DTO. |
| Middleware | `code/Middleware/` | Cơ chế pipeline có sẵn của ASP.NET Core; dự án tự viết xử lý exception và kiểm tra user/security stamp. |
| ASP.NET Core Identity | `AuthService`, `UserService`, `RoleService` | Có sẵn quản lý user/role, băm và kiểm tra mật khẩu, sign-in, security stamp và bearer token. Dự án viết nghiệp vụ và policy sử dụng các API này. |
| EF Core và Npgsql | `Data/`, `Repositories/`, `Migrations/` | EF Core ánh xạ entity, LINQ, theo dõi thay đổi và migration; Npgsql kết nối PostgreSQL. Dự án tự định nghĩa entity, truy vấn, ràng buộc và seed. |
| Redis cache | `BookService` và các service liên quan | Package Redis cung cấp implementation của `IDistributedCache`; dự án tự chọn key, thời gian sống, dữ liệu cache và thời điểm xóa cache. PostgreSQL vẫn là nơi lưu dữ liệu chính. |
| Swashbuckle | `Program.cs` | Sinh tài liệu OpenAPI và giao diện Swagger từ endpoint/DTO, bật UI trong Development. |
| React, React Router, Vite | `front_end/` | Thư viện giao diện, điều hướng và server phát triển; nhóm tự viết trang, gọi API và xử lý trạng thái giao diện. |
| xUnit, EF Core SQLite | `back_end/light_brary.Tests/` | Framework test và database SQLite trong bộ nhớ cho test hiện có; test tập trung vào cache sách. |

Dependency injection là chức năng có sẵn của ASP.NET Core: `Program.cs` đăng ký service và repository theo phạm vi request (`scoped`), framework tạo và đưa dependency vào constructor. Repository, service và logic nghiệp vụ là code dự án.

## 4. Các luồng API

### Đăng nhập và tài khoản

- **Đăng nhập — `POST /api/auth/login`:** `AuthController` nhận username/password → `AuthService` gọi `SignInManager.PasswordSignInAsync` với bearer scheme → Identity kiểm tra thông tin và phát bearer token khi thành công. Response có access token và refresh token. Token này do Identity/Data Protection bảo vệ, không phải JWT. Project hiện không map endpoint refresh token. Frontend lưu access token trong `localStorage` và dùng khi gọi API.
- **Đổi mật khẩu — `PUT /api/auth/password`:** yêu cầu xác thực → lấy user ID từ claims → `AuthService` kiểm tra mật khẩu mới khác mật khẩu cũ → `UserManager.ChangePasswordAsync` kiểm tra mật khẩu hiện tại và cập nhật. Chính sách hiện tại yêu cầu tối thiểu 8 ký tự.
- **Đổi số điện thoại — `PUT /api/auth/phone-number`:** DTO kiểm tra định dạng → service kiểm tra trùng và số mới khác số hiện tại → Identity cập nhật số điện thoại.
- **Đăng xuất — `PUT /api/auth/logout`:** service cập nhật security stamp; middleware kiểm tra stamp khi người dùng gọi các endpoint được bảo vệ tiếp theo.

### Hồ sơ và người dùng

- **Hồ sơ — `GET/PUT /api/me`:** controller lấy ID từ người dùng đã xác thực → `UserService` dùng Identity lấy user; PUT cập nhật địa chỉ → trả `UserResponse`.
- **Lịch sử — `GET /api/me/borrow-records` hoặc `GET /api/users/{id}/borrow-records`:** service kiểm tra user tồn tại → repository tải phiếu và chi tiết → chuyển sang response. Đường dẫn `/me` dành cho chính user; đường dẫn theo ID yêu cầu Admin hoặc Librarian.
- **Tra cứu — `GET /api/users`, `GET /api/users/{id}`:** Admin/Librarian truy vấn danh sách theo từ khóa/role hoặc lấy một user; service kết hợp thông tin vai trò và trả DTO.
- **Xóa — `DELETE /api/users/{id}`:** Admin gọi service → Identity xóa user. Cần lưu ý database có quan hệ cascade tới phiếu mượn; xem [database](database.md).

Hệ thống không cung cấp API đăng ký hay tạo người dùng; có hàm tạo user nội bộ trong service và dữ liệu seed.

### Vai trò

`RolesController` yêu cầu Admin. `GET /api/roles` và `GET /api/users/{userId}/roles` trả các vai trò hệ thống hoặc vai trò của user. Với `POST/DELETE /api/users/{userId}/roles/{roleId}`, service kiểm tra role/user → Identity gán hoặc gỡ role → cập nhật security stamp trong transaction → trả `204`. Các role được hỗ trợ là Admin, Librarian và User; không có API CRUD role.

### Thể loại

`CategoriesController` cho phép xem công khai; Admin/Librarian tạo hoặc cập nhật, Admin xóa. Service kiểm tra tên, tìm dữ liệu qua repository và lưu bằng EF Core. Khi cập nhật thể loại, service xóa cache chi tiết của các sách thuộc thể loại; khi xóa, service chặn nếu thể loại còn sách.

### Đầu sách

- **Danh sách — `GET /api/books`:** controller nhận `page`, `pageSize`, từ khóa, thể loại, tác giả, trạng thái còn sách và cách sắp xếp → `BookService` → `BookRepository` tạo LINQ, đếm và phân trang → EF Core truy vấn PostgreSQL → chuyển entity sang DTO, gồm số bản sao và số còn sẵn. Danh sách không dùng Redis.
- **Chi tiết — `GET /api/books/{id}`:** `BookService` đọc key `books:{id}` qua `IDistributedCache`. Nếu có thì giải mã JSON và trả; nếu chưa có thì repository lấy sách cùng thể loại và bản sao từ PostgreSQL, service chuyển sang DTO, lưu JSON vào cache trong 10 phút rồi trả. Redis dùng tiền tố `LightBrary:` do cấu hình.
- **Tạo/sửa/xóa:** service kiểm tra thể loại, ISBN và điều kiện nghiệp vụ → repository ghi database. Sửa/xóa xóa cache chi tiết tương ứng. Admin/Librarian được tạo, sửa; chỉ Admin xóa. Service chặn xóa nếu có bản sao đang mượn.

Đây là cache-aside ở phạm vi chi tiết sách, không phải cache toàn bộ hệ thống. Redis lỗi có thể khiến request lỗi; ghi database và xóa cache không phải một transaction chung.

### Bản sao sách

Một đầu sách có thể có nhiều cuốn vật lý, mỗi cuốn có barcode và trạng thái riêng.

- `GET /api/books/{bookId}/copies` công khai; các API xem toàn bộ, xem theo ID hoặc barcode yêu cầu Admin/Librarian.
- Tạo/sửa bản sao: service kiểm tra đầu sách hoặc bản sao và barcode → repository ghi database → trả DTO. Tạo bản sao xóa cache đầu sách liên quan.
- Xóa bản sao: chỉ Admin; service chặn nếu bản sao có lịch sử mượn, sau đó xóa và xóa cache đầu sách. Sửa barcode/ghi chú không xóa cache đầu sách vì DTO chi tiết sách chỉ dùng số lượng bản sao.

### Mượn, trả và báo mất

**Tạo phiếu — `POST /api/borrow-records`:** Admin/Librarian gửi user ID, các bản sao, hạn trả và ghi chú → service kiểm tra user, hạn trả, loại ID trùng và kiểm tra các bản sao tồn tại, đang `Available` → chuyển bản sao sang `Borrowed`, tạo phiếu và chi tiết `Borrowing` → repository lưu thay đổi bằng EF Core → xóa cache các đầu sách liên quan → trả phiếu.

**Xem phiếu — `GET /api/borrow-records` và `GET /api/borrow-records/{id}`:** danh sách dành cho Admin/Librarian và có thể lọc theo user, trạng thái, quá hạn; repository tải dữ liệu và service tạo DTO. Chi tiết yêu cầu đăng nhập: Admin/Librarian được xem, user khác chỉ xem phiếu của mình. Danh sách phiếu hiện không phân trang; lọc quá hạn thực hiện sau khi tải danh sách.

**Trả sách — `POST /api/borrow-records/{borrowRecordId}/returns`:** service tải phiếu và chi tiết → lọc các ID được gửi thuộc phiếu → kiểm tra các dòng tìm được đang mượn → chuyển chi tiết sang `Returned`, ghi thời gian trả và chuyển bản sao sang `Available` → tính lại trạng thái phiếu (`Borrowing` nếu còn sách đang mượn, nếu không thì `Closed`) → lưu và xóa cache đầu sách liên quan. ID không thuộc phiếu hiện bị bỏ qua, không có kiểm tra riêng để báo lỗi cho từng ID.

**Báo mất — `POST /api/borrow-details/{id}/mark-lost`:** service tìm chi tiết đang mượn → chuyển chi tiết và bản sao sang `Lost` → tính lại trạng thái phiếu → lưu và xóa cache đầu sách.

Lưu ý: luồng mượn hiện chưa dùng khóa dòng hay partial unique index để ngăn hai request đồng thời mượn cùng bản sao. Không có chức năng gia hạn hay tính phí phạt trong các API hiện tại.
