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

## ⚙️ Cấu hình môi trường

Dự án dùng **hai nơi** để chứa cấu hình cục bộ. Cả hai đều **không được đưa lên Git**:

| Nơi lưu      | Dùng cho                                    | Ai đọc         |
| ------------ | ------------------------------------------- | -------------- |
| `.env`       | Thông tin khởi tạo PostgreSQL               | Docker Compose |
| User Secrets | Bí mật của ứng dụng (ví dụ `Jwt:SecretKey`) | ASP.NET Core   |

### 1. Tạo file `.env`

```powershell
Copy-Item .env.example .env      # macOS/Linux: cp .env.example .env
```

Mở `.env` và chỉnh nếu cần:

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=Asdf1234!
POSTGRES_DB=lightbrary_db
POSTGRES_PORT=5432
```

Khởi động và kiểm tra database:

```powershell
docker compose up -d
docker compose ps                # cột STATUS phải là "healthy"
```

> ⚠️ ASP.NET Core **không đọc** file `.env`. Nếu bạn đổi user, mật khẩu, tên database hoặc cổng, hãy sửa cả chuỗi kết nối `ConnectionStrings:DefaultConnection` trong `user-secrets` cho khớp.
>
> Các biến `POSTGRES_*` chỉ có tác dụng ở lần khởi tạo đầu tiên. Muốn áp dụng giá trị mới, chạy `docker compose down -v` (**xóa toàn bộ dữ liệu**) rồi `docker compose up -d`.

Nếu backend báo `28P01: password authentication failed` dù cấu hình mật khẩu khớp, kiểm tra xem cổng `5432` có đang thuộc PostgreSQL của dự án khác không:

```bash
docker ps --format 'table {{.Names}}\t{{.Ports}}'
docker compose ps
```

Nếu cổng bị trùng, đặt `POSTGRES_PORT=5433` trong `.env` (chọn cổng còn trống), rồi chạy `docker compose up -d postgres`. Cập nhật `ConnectionStrings:DefaultConnection` trong User Secrets sang cùng cổng, giữ các thông tin còn lại khớp với PostgreSQL của LightBrary:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=lightbrary_db;Username=postgres;Password=<mật khẩu PostgreSQL>" --project back_end/light_brary
```

Thay `<mật khẩu PostgreSQL>` bằng mật khẩu thực tế, sau đó chạy lại backend. Việc đổi cổng không cần xóa database hoặc volume.

### 2. Tạo User Secrets

Chạy các lệnh từ thư mục gốc của solution. `--project` trỏ tới project API (project chứa `Program.cs`).

**Bước 1: khởi tạo (chỉ làm nếu `.csproj` của project API chưa có `<UserSecretsId>`)**

```powershell
dotnet user-secrets init --project back_end/light_brary
```

Nếu `.csproj` đã có dòng `<UserSecretsId>`, bỏ qua bước này. Dòng đó được commit và không phải bí mật.

**Bước 2: sinh một khóa ngẫu nhiên (tối thiểu 32 ký tự) và lưu lại**

```powershell
$bytes = New-Object byte[] 48
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)

dotnet user-secrets set "Jwt:SecretKey" $key --project back_end/light_brary
```

### 3. Kiểm tra

```powershell
dotnet user-secrets list --project back_end/light_brary
```

Kết quả mong đợi có dòng:

```
Jwt:SecretKey = <chuỗi ngẫu nhiên>
```

> `list` in giá trị ra dạng văn bản thường, hãy cẩn thận khi chia sẻ màn hình hoặc chụp ảnh gửi người khác.

Các lệnh hữu ích khác:

```powershell
dotnet user-secrets remove "Jwt:SecretKey" --project back_end/light_brary   # xóa một khóa
dotnet user-secrets clear --project back_end/light_brary                    # xóa tất cả
```

---

## 🛠 Yêu Cầu Hệ Thống

- **.NET SDK**: 10.0 trở lên
- **Node.js**: 18.0 trở lên
- **PostgreSQL**: 16 hoặc 17 (hoặc Docker Compose)

---

## ⚡ Hướng Dẫn Khởi Chạy

### 1. Khởi động Cơ sở dữ liệu PostgreSQL

Sử dụng Docker Compose đi kèm:

```bash
docker compose up -d
```

Cơ sở dữ liệu mặc định sẽ lắng nghe tại `localhost:5432` với database `lightbrary_db`.

### 2. Khởi chạy Backend (.NET 10 Web API)

```bash
dotnet run --project back_end/light_brary
```

- API tự động áp dụng migration, tạo Admin nếu chưa có và bổ sung dữ liệu thư viện mẫu trong môi trường Development.
- Swagger UI: `http://localhost:5048/swagger`
- OpenAPI Schema: `http://localhost:5048/openapi/v1.json`

### 3. Khởi chạy Frontend (React + Vite)

Mở một terminal mới:

```bash
cd front_end
npm install
npm run dev
```

Giao diện người dùng sẽ chạy tại: `http://localhost:3000` (hoặc `http://localhost:5173`).

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

Seed chạy tự động cùng backend, không cần chạy `seed_data.sql` riêng. Chạy lại sẽ bổ sung tài khoản, sách và bản sao còn thiếu, giữ nguyên dữ liệu, mật khẩu và tình trạng mượn đã có. Nếu database đã có Admin hoặc thủ thư thì tái sử dụng các tài khoản đó; tổng dữ liệu có thể nhiều hơn số lượng trên nếu trước đó đã có dữ liệu khác. Ảnh bìa cần kết nối Internet.

Đặt `Seed__SampleData=false` để tắt dữ liệu mẫu. Ngoài Development, dữ liệu mẫu mặc định tắt; có thể bật bằng `Seed__SampleData=true`. Cơ chế khởi tạo Admin vẫn hoạt động độc lập.

---

## 🧪 Kiểm Thử Tự Động (Automated Tests)

Solution hiện chứa một project backend, chưa có project Unit Tests hoặc Integration Tests. Kiểm tra compile từ thư mục gốc:

```bash
dotnet build Library.slnx
```

Các endpoint cần được kiểm tra trên môi trường development với PostgreSQL đã cấu hình.
