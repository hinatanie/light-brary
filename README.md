# LightBrary

LightBrary là hệ thống quản lý thư viện, hỗ trợ tra cứu sách, quản lý bản sao, người dùng và nghiệp vụ mượn trả. Backend ASP.NET Core là trọng tâm dự án; frontend React được giữ để demo.

## Công nghệ

- **Backend:** ASP.NET Core 10, ASP.NET Core Identity, Entity Framework Core, PostgreSQL, Redis và Swagger.
- **Frontend:** React 19, TypeScript và Vite.
- **Công cụ khác:** Docker Compose, xUnit và GitHub Actions.

## Chuẩn bị

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

## Cách 2: Chạy backend trực tiếp

Mở terminal tại thư mục gốc của repository `light-brary` (thư mục chứa `Library.slnx`). Chuẩn bị PostgreSQL và Redis trên máy; nếu dùng Docker cho hai dịch vụ này, chạy:

```bash
docker compose up -d postgres redis
```

Cấu hình bên dưới tương ứng với cổng và tài khoản mặc định của Compose. Thay mật khẩu PostgreSQL bằng giá trị đang dùng và chọn mật khẩu admin từ 8 ký tự; nếu dùng database hoặc Redis khác, chỉnh địa chỉ và thông tin kết nối cho phù hợp.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5434;Database=lightbrary_db;Username=postgres;Password=<mật-khẩu-PostgreSQL>" --project back_end/light_brary
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6381" --project back_end/light_brary
dotnet user-secrets set "Seed:AdminPassword" "<mật-khẩu-admin>" --project back_end/light_brary

dotnet restore Library.slnx
dotnet run --project back_end/light_brary
```

Cũng có thể cấu hình bằng biến môi trường `ConnectionStrings__DefaultConnection`, `ConnectionStrings__Redis` và `Seed__AdminPassword`. Backend tự áp dụng migration và seed khi khởi động; Swagger ở `http://localhost:5218/swagger` trong Development.

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
