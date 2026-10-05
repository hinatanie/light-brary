# LightBrary — Hệ Thống Quản Lý Thư Viện Trực Tuyến

Hệ thống quản lý thư viện hiện đại được xây dựng theo kiến trúc Clean Architecture với **ASP.NET Core (.NET 10)** ở Backend và **React 19 + TypeScript + Vite** ở Frontend.

---

## 🚀 Tính Năng Chính

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

### 2. Tạo User Secrets

Chạy các lệnh từ thư mục gốc của solution. `--project` trỏ tới project API (project chứa `Program.cs`).

**Bước 1: khởi tạo (chỉ làm nếu `.csproj` của project API chưa có `<UserSecretsId>`)**

```powershell
dotnet user-secrets init
```

Nếu `.csproj` đã có dòng `<UserSecretsId>`, bỏ qua bước này. Dòng đó được commit và không phải bí mật.

**Bước 2: sinh một khóa ngẫu nhiên (tối thiểu 32 ký tự) và lưu lại**

```powershell
$bytes = New-Object byte[] 48
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)

dotnet user-secrets set "Jwt:SecretKey" $key
```

### 3. Kiểm tra

```powershell
dotnet user-secrets list
```

Kết quả mong đợi có dòng:

```
Jwt:SecretKey = <chuỗi ngẫu nhiên>
```

> `list` in giá trị ra dạng văn bản thường, hãy cẩn thận khi chia sẻ màn hình hoặc chụp ảnh gửi người khác.

Các lệnh hữu ích khác:

```powershell
dotnet user-secrets remove "Jwt:SecretKey" --project src/Library.Api   # xóa một khóa
dotnet user-secrets clear --project src/Library.Api                    # xóa tất cả
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
dotnet run --project src/Library.Api
```

- API sẽ tự động áp dụng migration và bootstrap tài khoản Admin nếu CSDL chưa có dữ liệu.
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

## 🔑 Tài Khoản Quản Trị Khởi Tạo (Môi Trường Phát Triển)

Khi hệ thống khởi chạy lần đầu tiên trên môi trường Development:

- **Số điện thoại đăng nhập**: `0916114536`
- **Email**: `admin@lightbrary.vn`
- **Mật khẩu khởi tạo**: `ThuVienSach2026!`
- **Lưu ý**: Hệ thống đã bật cờ `MustChangePassword`. Sau khi đăng nhập, hệ thống sẽ tự động yêu cầu bạn đổi sang mật khẩu mới trước khi tiếp tục.

> Trong môi trường Production, thông tin Admin ban đầu và JWT Secret được nạp thông qua Environment Variables hoặc Secret Manager (`AdminBootstrap__Password`, `Jwt__SecretKey`), tuyệt đối không dùng giá trị mặc định.

---

## 🧪 Kiểm Thử Tự Động (Automated Tests)

Chạy toàn bộ Unit Tests & Integration Tests:

```bash
dotnet test Library.slnx
```

Hệ thống bao gồm:

- **43 Unit Tests**: Kiểm tra chi tiết `PasswordPolicy`, `PhoneNumberNormalizer`, `IsbnNormalizer`, `BorrowStatusCalculator`, `AuthService`, `BorrowRecordService`, v.v.
- **Integration Tests**: Kiểm tra trực tiếp trên CSDL PostgreSQL thực tế, bao gồm:
  - Test kiểm tra tính duy nhất của Email và Số điện thoại (HTTP 409 Conflict).
  - Test kiểm tra Race Condition mượn sách: Chạy **10 requests đồng thời** cùng tranh chấp mượn 1 cuốn sách -> Kết quả chỉ duy nhất 1 request thành công, 9 requests còn lại bị chặn an toàn.
