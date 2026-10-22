# LightBrary — Hệ Thống Quản Lý Thư Viện Trực Tuyến

Hệ thống quản lý thư viện sử dụng **ASP.NET Core (.NET 10)** ở Backend và **React 19 + TypeScript + Vite** ở Frontend. Backend được tổ chức theo các lớp Controllers, Services, Repositories và Entities trong thư mục `back_end/light_brary/code/` của một project; DTO được tách thành Requests và Responses. `Program.cs`, cấu hình và `Properties/` nằm ở thư mục project. Xem cấu trúc và quy tắc tại [CONTRIBUTING.md](docs/CONTRIBUTING.md).

---

## 🚀 Tính Năng Chính

- **Tài khoản được nhà trường cấp sẵn**: Sinh viên đăng nhập bằng mã số sinh viên lưu tại `UserName`; số điện thoại là thông tin liên hệ. Không có đăng ký công khai hoặc API tạo người dùng; cơ chế seed Admin ban đầu vẫn được giữ.
- **Quản lý đầu sách & bản sao vật lý**: Quản lý thông tin sách (tiêu đề, tác giả, ISBN, ảnh bìa, thể loại) và bản sao từng cuốn (mã barcode, tình trạng kệ).
- **Quy trình mượn - trả sách an toàn**: Lập phiếu mượn nhiều cuốn, gia hạn, trả sách tính phí phạt và báo mất sách (`Lost`).
- **Chống Race Condition tuyệt đối**: Cơ chế khóa dòng `SELECT ... FOR UPDATE` trong transaction kết hợp với Partial Unique Constraint `UX_BorrowDetails_ActiveCopy` ở mức CSDL ngăn chặn 100% tình trạng 2 người cùng mượn 1 cuốn sách đồng thời.
- **Phân quyền 3 cấp độ (Role-Based Access Control)**:
  - **Admin (Quản trị viên)**: Toàn quyền quản lý sách, thể loại, kho bản sao, độc giả và cấu hình phân quyền người dùng.
  - **Staff (Thủ thư)**: Xử lý quy trình mượn trả, hoàn trả sách vào kệ, báo mất sách, xem thống kê kho.
  - **Member (Bạn đọc)**: Tra cứu danh mục sách, theo dõi phiếu mượn cá nhân, tình trạng quá hạn và thông tin hồ sơ.
- **Bảo mật chuẩn Enterprise**:
  - Tự động xoay vòng Refresh Token (Rotation & Revocation) qua HttpOnly Cookie.
  - Ngăn chặn tấn công Brute-force: Tự động khóa tài khoản 15 phút sau 5 lần nhập sai mật khẩu (HTTP 423 Locked).
  - Chính sách mật khẩu nghiêm ngặt (tối thiểu 10 ký tự, có chữ hoa, thường, số, chống mật khẩu yếu).
  - Cơ chế bắt buộc đổi mật khẩu lần đầu (`MustChangePassword`).
  - Khởi tạo tài khoản quản trị ban đầu an toàn (AdminBootstrap) từ biến môi trường/cấu hình, không bao giờ log mật khẩu thô.
  - CSDL PostgreSQL chuẩn hóa: ID dạng `uuid` (Guid), unique index cho `PhoneNumber`, `Email`, `ISBN` và `citext` cho email/thể loại/role.

---

## Run using Docker Compose

Cần Docker Engine/Docker Desktop đang chạy và Docker Compose v2 trở lên. Chạy từ thư mục gốc repository:

```bash
docker compose up --build -d
```

Không cần tạo `.env` khi dev: Compose dùng mật khẩu PostgreSQL mặc định `change-me` và mật khẩu Admin/dữ liệu mẫu `change-me-admin`. Đây là giá trị công khai chỉ dành cho development cục bộ, không dùng cho production.

Khi cần đổi cổng hoặc mật khẩu, có thể tạo `.env` bằng `cp .env.example .env` (PowerShell: `Copy-Item .env.example .env`) rồi sửa các biến tương ứng. Nếu đã có `.env`, giữ file hiện tại; giá trị trong file hoặc environment variables sẽ ghi đè mặc định. Mật khẩu Admin phải có ít nhất 8 ký tự. File `.env` được Git và Docker build context bỏ qua; không đưa mật khẩu thực tế lên Git. Với mật khẩu PostgreSQL, nên dùng chuỗi chữ/số ngẫu nhiên (ví dụ sinh bằng `openssl rand -hex 24`) để tránh ký tự phân cách connection string.

Compose khởi động ba service độc lập với những container PostgreSQL/Redis chạy thủ công trên máy:

| Service | Địa chỉ từ máy host (mặc định) | Địa chỉ trong Docker network |
| --- | --- | --- |
| API | `http://localhost:5218` | `api:8080` |
| PostgreSQL 17 | `localhost:5434` | `postgres:5432` |
| Redis 7 | `localhost:6381` | `redis:6379` |

Các cổng host chỉ bind vào `127.0.0.1`. Đổi `API_PORT`, `POSTGRES_PORT`, `REDIS_PORT` trong `.env` nếu bị trùng. Cổng API mặc định khớp proxy Vite hiện tại; đổi cổng API thì cần cập nhật target proxy khi chạy frontend. Cổng PostgreSQL/Redis được chọn để tránh các container local đang dùng `5433` và `6380`.

- API: `http://localhost:5218/api/books`
- Swagger UI: `http://localhost:5218/swagger`
- Swagger JSON: `http://localhost:5218/swagger/v1/swagger.json`
- Health: `http://localhost:5218/health` trả `Healthy` khi API chạy; đây là liveness check, không kiểm tra PostgreSQL/Redis liên tục.

Compose mặc định dùng `Development` để có Swagger và dữ liệu mẫu. Đây là cấu hình phát triển cục bộ. Ngoài Development, Swagger không được bật. Nếu chạy với `ASPNETCORE_ENVIRONMENT=Production`, cấu hình mật khẩu riêng cho `POSTGRES_PASSWORD`, `SEED_ADMIN_PASSWORD` và đặt thêm `SEED_SAMPLE_DATA=false` để tắt dữ liệu mẫu.

API đợi PostgreSQL và Redis healthy trước khi khởi động. Cơ chế `AppDbContext.Database.MigrateAsync()` hiện có vẫn tự áp dụng migration rồi seed Admin/dữ liệu mẫu; không cần SDK hoặc lệnh EF trong runtime container. PostgreSQL lưu dữ liệu trong named volume `postgres_data`; Redis chỉ là cache, không cần lưu bền.

Configuration được nạp qua `appsettings.json`, `appsettings.Development.json` (khi Development) rồi environment variables. Compose truyền:

```text
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=<POSTGRES_DB>;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>
ConnectionStrings__Redis=redis:6379
Seed__AdminPassword=<SEED_ADMIN_PASSWORD>
Seed__SampleData=<SEED_SAMPLE_DATA>
```

ASP.NET Core không tự đọc `.env`; Docker Compose lấy giá trị mặc định hoặc giá trị ghi đè từ `.env`/environment variables rồi truyền các biến trên vào API. User Secrets của máy host không được copy vào image. Code xác thực hiện tại dùng ASP.NET Core Identity API endpoints, không đọc `Jwt:SecretKey`, nên Compose không cần JWT secret riêng. Dockerfile dùng SDK .NET 10 để restore/publish và ASP.NET Runtime 10 để chạy `light_brary.dll` bằng user không phải root; runtime chỉ nhận output publish và công cụ `curl` cho health check.

Kiểm tra hoặc chạy nền:

```bash
docker compose config --quiet
docker compose up --build -d --wait
docker compose ps
curl --fail http://localhost:5218/health
curl --fail http://localhost:5218/api/books
docker compose logs api
```

Dừng hệ thống, giữ dữ liệu database:

```bash
docker compose down
```

Chỉ khi chủ động muốn xóa database và khởi tạo lại:

```bash
docker compose down -v
```

**Cảnh báo: `down -v` xóa database volume và toàn bộ dữ liệu trong đó.** Không cần xóa volume khi chỉ đổi host port. `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` chỉ khởi tạo database ở lần đầu; sửa `.env` không đổi credential của database đã có. Với dữ liệu cần giữ, cập nhật credential trong PostgreSQL rồi đồng bộ cấu hình thay vì xóa volume.

File Compose cũ chưa mount named volume và dùng container cố định `lightbrary-postgres`. Cấu hình mới không tự nhập dữ liệu từ container đó: nếu cần giữ dữ liệu cũ, dùng `pg_dump`/restore trước khi chuyển, và không xóa container hoặc volume cũ khi chưa có bản sao lưu.

## Local development without Docker

Backend vẫn chạy trực tiếp với .NET SDK 10; cần PostgreSQL 17 (hoặc bản 16 hiện đang dùng) và Redis 7 sẵn sàng trên máy. Nếu dùng database/container riêng hiện có, không cần bật cả bộ Compose. Khi chỉ muốn dùng dependencies từ Compose:

```bash
docker compose up -d postgres redis
```

Không cần `.env` nếu dùng cấu hình dev mặc định; PostgreSQL dùng mật khẩu `change-me`. Nếu đã có `.env`, giữ file hiện tại để tiếp tục dùng cấu hình riêng. Cấu hình này expose PostgreSQL tại `localhost:5434`, Redis tại `localhost:6381`. Khi dùng Redis local hiện tại ở `localhost:6380`, giữ nguyên `appsettings.Development.json`; chỉ override Redis khi dùng địa chỉ khác.

Đặt secrets cho project API (thay placeholder bằng giá trị thực tế, khớp database của bạn):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5434;Database=lightbrary_db;Username=postgres;Password=<mật-khẩu-PostgreSQL>" --project back_end/light_brary
dotnet user-secrets set "Seed:AdminPassword" "<mật-khẩu-Admin-ít-nhất-8-ký-tự>" --project back_end/light_brary
# Chỉ cần dòng này nếu Redis không chạy ở localhost:6380:
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6381" --project back_end/light_brary

dotnet restore Library.slnx
dotnet run --project back_end/light_brary
```

Project đã có `UserSecretsId`, không cần khởi tạo lại. Có thể dùng environment variables `ConnectionStrings__DefaultConnection`, `ConnectionStrings__Redis`, `Seed__AdminPassword` thay User Secrets. Không có mật khẩu mặc định trong appsettings; lần đầu seed Admin cần cung cấp mật khẩu. Profile HTTP trong `Properties/launchSettings.json` dùng `http://localhost:5218`, Swagger tại `/swagger`, schema tại `/swagger/v1/swagger.json` và health tại `/health`.

Backend tự chạy migration và seed theo cơ chế hiện có. Chi tiết sách theo ID được cache 10 phút với tiền tố `LightBrary:books:`; danh sách phân trang vẫn đọc từ database. Để kiểm tra cache qua HTTP, lấy một ID từ `GET /api/books`, gọi `GET /api/books/{id}` hai lần rồi kiểm tra Redis:

```bash
docker compose exec redis redis-cli --scan --pattern 'LightBrary:books:*'
```

Khởi chạy frontend trong terminal khác (cần Node.js tương thích Vite hiện tại):

```bash
cd front_end
npm ci
npm run dev
```

Frontend mặc định chạy tại `http://localhost:3000`, proxy `/api` đến `http://localhost:5218`.

---

## 🔑 Tài khoản và dữ liệu khởi tạo

Khi chạy trên database trống trong môi trường **Development**, API tự seed:

- **20 sinh viên**, mã đăng nhập từ `3124560001` đến `3124560020`, có họ tên, số điện thoại và địa chỉ mẫu.
- **1 Admin**, dùng `Seed:AdminUserName`; nếu chưa cấu hình thì dùng `Seed:AdminPhone` (mặc định `0999999999`).
- **1 thủ thư**, tên đăng nhập `thuthu`.
- **5 thể loại**, mỗi thể loại có 6 sách: Công nghệ thông tin, Kỹ năng sống, Văn học, Kinh tế, Lịch sử & triết học.
- **30 đầu sách có thật**, lưu tên tiếng Anh, tác giả, ISBN, năm của ấn bản và đường dẫn ảnh bìa Open Library.
- **75 bản sao**: 15 sách có 2 bản, 15 sách có 3 bản; barcode riêng và trạng thái sẵn sàng cho mượn.

Admin dùng mật khẩu cấu hình tại `Seed:AdminPassword`. Sinh viên và thủ thư dùng `Seed:SamplePassword`; nếu chưa đặt thì dùng `Seed:AdminPassword`. Đặt mật khẩu bằng .NET User Secrets hoặc các biến môi trường `Seed__AdminPassword`, `Seed__SamplePassword`, không lưu mật khẩu thực tế vào Git.

Seed chạy tự động cùng backend, không cần chạy script SQL riêng. Chạy lại sẽ bổ sung tài khoản, sách và bản sao còn thiếu, giữ nguyên dữ liệu, mật khẩu và tình trạng mượn đã có. Nếu database đã có Admin hoặc thủ thư thì tái sử dụng các tài khoản đó; tổng dữ liệu có thể nhiều hơn số lượng trên nếu trước đó đã có dữ liệu khác. Ảnh bìa cần kết nối Internet.

Đặt `Seed__SampleData=false` để tắt dữ liệu mẫu. Ngoài Development, dữ liệu mẫu mặc định tắt; có thể bật bằng `Seed__SampleData=true`. Cơ chế khởi tạo Admin vẫn hoạt động độc lập.

---

## 🧪 Kiểm Thử Tự Động (Automated Tests)

Kiểm tra compile và chạy các test cache từ thư mục gốc:

```bash
dotnet build Library.slnx
dotnet test Library.slnx
```

Các test dùng SQLite trong bộ nhớ và `IDistributedCache` trong bộ nhớ, không cần PostgreSQL. Để chạy cùng bộ test với Redis thật trên Bash:

```bash
LIGHTBRARY_TEST_REDIS=localhost:6380 dotnet test Library.slnx
# Hoặc dùng Redis của Compose:
LIGHTBRARY_TEST_REDIS=localhost:6381 dotnet test Library.slnx
```

Mỗi test dùng tiền tố Redis riêng và xóa các key do test tạo. Bộ test kiểm tra cache miss/hit, dữ liệu không tồn tại, cập nhật/xóa sách và xóa cache khi dữ liệu liên quan thay đổi. Các endpoint vẫn cần được kiểm tra trên môi trường development với PostgreSQL đã cấu hình.

## GitHub Actions CI

Workflow `.github/workflows/ci.yml` chạy khi push lên mọi branch (bao gồm `main`) và khi mở/cập nhật pull request. Job dùng Ubuntu, checkout source, cài .NET SDK `10.0.x`, rồi chạy toàn bộ solution:

```bash
dotnet restore Library.slnx
dotnet build Library.slnx --configuration Release --no-restore
dotnet test Library.slnx --configuration Release --no-build --no-restore
```

CI dùng SQLite/cache bộ nhớ theo thiết kế test hiện tại, không cần secret, PostgreSQL hoặc Redis service container. Workflow chỉ build/test, không deploy.

Cấu trúc đóng gói và CI:

```text
light-brary/
├── Dockerfile
├── compose.yml
├── .dockerignore
├── .env.example
├── .gitignore
├── .github/
│   └── workflows/
│       └── ci.yml
├── Library.slnx
├── README.md
├── back_end/
│   ├── light_brary/
│   │   ├── light_brary.csproj
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Migrations/
│   │   └── code/
│   └── light_brary.Tests/
│       ├── light_brary.Tests.csproj
│       └── BookCacheTests.cs
├── front_end/
└── docs/
```
