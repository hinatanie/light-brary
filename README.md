# LightBrary

LightBrary là hệ thống quản lý thư viện, hỗ trợ tra cứu sách, quản lý bản sao, người dùng và nghiệp vụ mượn trả. Backend ASP.NET Core là trọng tâm dự án; frontend React được giữ để demo.

## Công nghệ

- **Backend:** ASP.NET Core 10, ASP.NET Core Identity, Entity Framework Core, PostgreSQL, Redis và Swagger.
- **Frontend:** React 19, TypeScript và Vite.
- **Công cụ khác:** Docker Compose, xUnit và GitHub Actions.

- **Tài khoản được nhà trường cấp sẵn**: Sinh viên đăng nhập bằng mã số sinh viên lưu tại `UserName`; số điện thoại là thông tin liên hệ. Không có đăng ký công khai; tài khoản được seed ban đầu (Admin, Thủ thư, 20 Sinh viên).
- **Quản lý đầu sách & bản sao vật lý**: Quản lý thông tin sách (tiêu đề, tác giả, ISBN, ảnh bìa, thể loại) và bản sao từng cuốn trên kệ (mã barcode duy nhất, tình trạng sách).
- **Bộ nhớ đệm phân tán Redis (Cache-Aside & Invalidation)**: Tối ưu đọc chi tiết đầu sách với Redis Distributed Cache (TTL 10 phút, phản hồi <1ms), tự động xóa cache liên tầng khi mượn, trả, báo mất sách, thêm/xóa bản sao hoặc sửa thể loại.
- **Quy trình mượn - trả sách chuẩn hóa**: Lập phiếu mượn nhiều cuốn (`POST /api/borrow-records`), hoàn trả sách vào kệ theo từng phiếu (`POST /api/borrow-records/{id}/returns`), và xử lý báo mất sách (`POST /api/borrow-details/{id}/mark-lost`).
- **Phân quyền 3 cấp độ (Role-Based Access Control & Policies)**:
  - **Admin (Quản trị viên)**: Toàn quyền quản lý sách, thể loại, kho bản sao, độc giả và gán/gỡ vai trò người dùng (`RolesController`).
  - **Staff (Thủ thư)**: Lập phiếu mượn, trả sách, báo mất sách, quản lý thể loại, tạo sách và bản sao.
  - **Member (Bạn đọc)**: Tra cứu danh mục sách có phân trang/lọc, xem hồ sơ cá nhân và lịch sử mượn sách của chính mình.
- **Bảo mật và Kiểm soát phiên với ASP.NET Core Identity**:
  - Xác thực hiện đại bằng **ASP.NET Core Identity API Bearer Token** với Data Protection.
  - Hủy phiên tức thì thông qua `ValidateUserMiddleware`: Kiểm tra `SecurityStamp` và `IsLockedOutAsync` trong DB ở mỗi request; khi người dùng đổi mật khẩu hoặc đăng xuất (`PUT /api/auth/logout`), token cũ bị vô hiệu hóa ngay lập tức.
  - Chống tấn công dò mật khẩu (Brute-force): Tự động khóa tài khoản 15 phút sau 5 lần nhập sai liên tiếp (`LockoutEnabled = true`, `MaxFailedAccessAttempts = 5`).
  - CSDL PostgreSQL 17 chuẩn hóa: Khóa chính và ngoại dạng `uuid` (Guid), ràng buộc toàn vẹn và Unique Index cho `PhoneNumber`, `NormalizedUserName`, `ISBN`, `Barcode`, `Category.Name`.

- **Docker Compose:** Docker Engine hoặc Docker Desktop và Docker Compose v2. Cách này chạy API, PostgreSQL 17 và Redis 7 trong container, không cần .NET SDK trên máy.
- **Chạy backend trực tiếp:** .NET SDK 10, PostgreSQL và Redis. Có thể dùng Compose chỉ để chạy PostgreSQL và Redis.
- **Chạy frontend:** Node.js `^20.19.0` hoặc `>=22.12.0` và npm.

Các lệnh dưới đây chạy từ thư mục gốc repository, trừ khi có ghi khác.

## Cách 1: Chạy backend bằng Docker Compose

Mở terminal tại thư mục gốc của repository `light-brary` (thư mục chứa file `compose.yml`), rồi chạy:

```bash
docker compose up --build -d
```

Compose khởi động API tại `http://localhost:5218`, PostgreSQL tại `localhost:5434` và Redis tại `localhost:6381`. API tự áp dụng migration và tạo dữ liệu mẫu trong môi trường Development. Swagger: `http://localhost:5218/swagger`.

Mặc định development dùng mật khẩu PostgreSQL `change-me` và mật khẩu tài khoản mẫu `change-me-admin`. Để đổi mật khẩu hoặc cổng, sao chép `.env.example` thành `.env` rồi chỉnh sửa; không dùng mật khẩu mặc định cho production.

```bash
cp .env.example .env
docker compose ps
docker compose down
```

`docker compose down` giữ lại dữ liệu PostgreSQL. Thay đổi thông tin PostgreSQL trong `.env` không tự cập nhật tài khoản của database đã được khởi tạo.

## Cách 2: Chạy backend trực tiếp (dev)

Mở terminal tại thư mục gốc.dùng Docker cho hai dịch vụ này, chạy:

```bash
docker compose up -d postgres redis
```

dùng dotnet user-secrets : chạy tại light_brary

```bash
dotnet user-secrets set "Seed:AdminPassword" "<mật-khẩu-admin>"
```

đã set xong biến thì chạy :

```bash
dotnet run
```

Backend tự áp dụng migration và seed khi khởi động; Swagger ở `http://localhost:5218/swagger` trong Development.

## Chạy frontend

Sau khi chạy backend theo một trong hai cách trên, mở terminal khác tại thư mục gốc `light-brary`, rồi chuyển vào `front_end` và chạy:

```bash
cd front_end
npm ci
npm run dev
```

Mở `http://localhost:3000`. Vite chuyển các yêu cầu `/api` đến backend tại `http://localhost:5218`; nếu đổi cổng API, sửa proxy trong `front_end/vite.config.ts`.

Trên database mới ở Development, tài khoản mẫu gồm admin `0999999999`, thủ thư `thuthu` và sinh viên từ `3124560001` đến `3124560020`. Mật khẩu là mật khẩu admin được cấu hình ở trên, trừ khi có cấu hình riêng cho dữ liệu mẫu.

## Tài liệu thêm

- [Hướng dẫn Swagger](docs/THIẾT%20KẾ%20API.md): xem và thử API.
- [Cơ sở dữ liệu](docs/database.md): các bảng và ràng buộc quan hệ.
- [Luồng hoạt động](docs/LUONG-HOAT-DONG.md): cách xử lý request, thư viện và các nhóm API.
