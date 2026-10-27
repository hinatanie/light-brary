# PHÂN CHIA 6 PHÂN HỆ QUẢN LÝ VÀ 37 API CHÍNH THỨC (+ 1 HEALTHCHECK) (LIGHTBRARY)

Tài liệu này phản ánh chính xác 37 API nghiệp vụ và 1 endpoint giám sát hệ thống được hiện thực trong mã nguồn ASP.NET Core (`back_end/light_brary/code/Controllers/` và `Program.cs`).

---

# 1. Quản lý xác thực và tài khoản cá nhân (`AuthController`)

Phân hệ này xử lý đăng nhập, đổi mật khẩu, đổi số điện thoại và đăng xuất của người dùng.
- Bảng CSDL: `AspNetUsers`, `AspNetUserTokens`
- Tiền tố Route: `/api/auth`

### Danh sách API

| Method | Endpoint | Chức năng | Quyền hạn |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Đăng nhập bằng tên đăng nhập/mã sinh viên và mật khẩu. Trả về Bearer Token | Công khai |
| `PUT` | `/api/auth/password` | Người dùng tự đổi mật khẩu của mình | Đã đăng nhập (`[Authorize]`) |
| `PUT` | `/api/auth/phone-number` | Người dùng tự đổi số điện thoại liên hệ của mình | Đã đăng nhập (`[Authorize]`) |
| `PUT` | `/api/auth/logout` | Đăng xuất, làm mới Security Stamp để hủy hiệu lực token | Đã đăng nhập (`[Authorize]`) |

---

# 2. Quản lý người dùng (`UsersController`)

Phân hệ này quản lý thông tin bạn đọc và người dùng trong hệ thống. Hệ thống không mở API đăng ký công khai; tài khoản được trường cấp sẵn hoặc seed ban đầu.
- Bảng CSDL: `AspNetUsers`, `AspNetUserRoles`, `AspNetRoles`

### Danh sách API

| Method | Endpoint | Chức năng | Quyền hạn |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/me` | Người dùng xem hồ sơ cá nhân và danh sách vai trò của mình | Đã đăng nhập (`[Authorize]`) |
| `PUT` | `/api/me` | Người dùng cập nhật địa chỉ liên hệ của mình | Đã đăng nhập (`[Authorize]`) |
| `GET` | `/api/me/borrow-records` | Người dùng xem toàn bộ lịch sử các phiếu mượn của mình | Đã đăng nhập (`[Authorize]`) |
| `GET` | `/api/users` | Lấy danh sách người dùng (hỗ trợ lọc `keyword`, `role`) | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `GET` | `/api/users/{id:guid}` | Lấy thông tin chi tiết của một người dùng theo ID | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `GET` | `/api/users/{id:guid}/borrow-records` | Xem lịch sử các phiếu mượn của một người dùng cụ thể | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `DELETE` | `/api/users/{id:guid}` | Xóa tài khoản người dùng khỏi hệ thống | Chỉ Admin (`AdminOnly`) |

---

# 3. Quản lý vai trò & Phân quyền (`RolesController`)

Phân hệ này quản lý việc xem và gán vai trò (`Admin`, `Librarian`, `User`) cho người dùng.
- Bảng CSDL: `AspNetRoles`, `AspNetUserRoles`

### Danh sách API

| Method | Endpoint | Chức năng | Quyền hạn |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/roles` | Lấy danh sách các vai trò trong hệ thống | Chỉ Admin (`AdminOnly`) |
| `GET` | `/api/users/{userId:guid}/roles` | Xem danh sách các vai trò đã được gán cho một người dùng | Chỉ Admin (`AdminOnly`) |
| `POST` | `/api/users/{userId:guid}/roles/{roleId:guid}` | Gán một vai trò cho người dùng | Chỉ Admin (`AdminOnly`) |
| `DELETE` | `/api/users/{userId:guid}/roles/{roleId:guid}` | Gỡ một vai trò khỏi người dùng | Chỉ Admin (`AdminOnly`) |

---

# 4. Quản lý thể loại sách (`CategoriesController`)

Phân hệ này quản lý các danh mục phân loại sách.
- Bảng CSDL: `Categories`
- Tiền tố Route: `/api/categories`

### Danh sách API

| Method | Endpoint | Chức năng | Quyền hạn |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/categories` | Lấy toàn bộ danh sách thể loại sách | Công khai |
| `GET` | `/api/categories/{id:guid}` | Xem thông tin chi tiết một thể loại theo ID | Công khai |
| `POST` | `/api/categories` | Tạo thể loại sách mới | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `PUT` | `/api/categories/{id:guid}` | Cập nhật thông tin thể loại (tự động xóa cache sách liên quan) | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `DELETE` | `/api/categories/{id:guid}` | Xóa thể loại (bị chặn nếu thể loại còn sách thuộc về) | Chỉ Admin (`AdminOnly`) |

---

# 5. Quản lý đầu sách và bản sao (`BooksController` & `BookCopiesController`)

Phân hệ này quản lý thông tin đầu sách và các bản sao vật lý trên kệ thư viện.
- Bảng CSDL: `Books`, `BookCopies`

### 5.1. Đầu sách (`BooksController` — `/api/books`)

| Method | Endpoint | Chức năng | Quyền hạn |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/books` | Phân trang, tìm kiếm và lọc sách (`keyword`, `author`, `categoryId`, `onlyAvailable`, `sortBy`) | Công khai |
| `GET` | `/api/books/{id:guid}` | Xem chi tiết đầu sách (**áp dụng Redis Cache**, TTL 10 phút) | Công khai |
| `POST` | `/api/books` | Tạo đầu sách mới | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `PUT` | `/api/books/{id:guid}` | Cập nhật thông tin đầu sách (**xóa cache Redis**) | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `DELETE` | `/api/books/{id:guid}` | Xóa đầu sách nếu không còn bản sao nào (**xóa cache Redis**) | Chỉ Admin (`AdminOnly`) |

### 5.2. Bản sao vật lý (`BookCopiesController` — `/api/...`)

| Method | Endpoint | Chức năng | Quyền hạn |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/book-copies/all` | Xem toàn bộ danh sách bản sao sách trong thư viện | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `GET` | `/api/books/{bookId:guid}/copies` | Xem các bản sao vật lý của một đầu sách | Công khai |
| `POST` | `/api/books/{bookId:guid}/copies` | Tạo bản sao vật lý mới cho đầu sách (**xóa cache Redis**) | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `GET` | `/api/book-copies/{id:guid}` | Xem chi tiết một bản sao sách | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `GET` | `/api/book-copies/by-barcode/{barcode}` | Tra cứu nhanh bản sao qua mã vạch Barcode | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `PUT` | `/api/book-copies/{id:guid}` | Cập nhật thông tin bản sao (barcode, ghi chú, trạng thái) | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `DELETE` | `/api/book-copies/{id:guid}` | Xóa bản sao sách (**xóa cache Redis**) | Chỉ Admin (`AdminOnly`) |

---

# 6. Quản lý mượn, trả và báo mất sách (`BorrowRecordsController`)

Phân hệ cốt lõi quản lý quy trình lưu thông sách trong thư viện.
- Bảng CSDL: `BorrowRecords`, `BorrowDetails`, `BookCopies`

### Danh sách API

| Method | Endpoint | Chức năng | Quyền hạn |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/borrow-records` | Tạo phiếu mượn nhiều bản sao sách cho độc giả; chuyển trạng thái sách sang `Borrowed` (**xóa cache Redis**) | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `GET` | `/api/borrow-records` | Lấy danh sách phiếu mượn (lọc theo `userId`, `status`, `overdue`) | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `GET` | `/api/borrow-records/{id:guid}` | Xem chi tiết phiếu mượn (độc giả chỉ xem được phiếu của mình; thủ thư/admin xem được tất cả) | Đã đăng nhập (`[Authorize]`) |
| `POST` | `/api/borrow-records/{borrowRecordId:guid}/returns` | Trả một hoặc nhiều cuốn sách trong phiếu; chuyển sách về `Available`, tính lại trạng thái phiếu (**xóa cache Redis**) | Thủ thư / Admin (`LibrarianOrAdmin`) |
| `POST` | `/api/borrow-details/{id:guid}/mark-lost` | Báo mất một cuốn sách đang mượn; chuyển sách sang `Lost`, tính lại trạng thái phiếu (**xóa cache Redis**) | Thủ thư / Admin (`LibrarianOrAdmin`) |

---

# 7. Endpoint Vận hành & Giám sát (`HealthCheck`)

| Method | Endpoint | Chức năng | Quyền hạn |
| :--- | :--- | :--- | :--- |
| `GET` | `/health` | Kiểm tra trạng thái sống của dịch vụ API (trả về chuỗi `Healthy`) | Công khai (`AllowAnonymous`) |

---

# 8. Bảng tổng hợp 37 Endpoints nghiệp vụ (+ 1 HealthCheck = 38 Endpoints) của dự án

```text
# Auth (4 endpoints)
POST   /api/auth/login
PUT    /api/auth/password
PUT    /api/auth/phone-number
PUT    /api/auth/logout

# Users & Profile (7 endpoints)
GET    /api/me
PUT    /api/me
GET    /api/me/borrow-records
GET    /api/users
GET    /api/users/{id:guid}
GET    /api/users/{id:guid}/borrow-records
DELETE /api/users/{id:guid}

# Roles (4 endpoints)
GET    /api/roles
GET    /api/users/{userId:guid}/roles
POST   /api/users/{userId:guid}/roles/{roleId:guid}
DELETE /api/users/{userId:guid}/roles/{roleId:guid}

# Categories (5 endpoints)
GET    /api/categories
GET    /api/categories/{id:guid}
POST   /api/categories
PUT    /api/categories/{id:guid}
DELETE /api/categories/{id:guid}

# Books & BookCopies (12 endpoints)
GET    /api/books
GET    /api/books/{id:guid}
POST   /api/books
PUT    /api/books/{id:guid}
DELETE /api/books/{id:guid}
GET    /api/book-copies/all
GET    /api/books/{bookId:guid}/copies
POST   /api/books/{bookId:guid}/copies
GET    /api/book-copies/{id:guid}
GET    /api/book-copies/by-barcode/{barcode}
PUT    /api/book-copies/{id:guid}
DELETE /api/book-copies/{id:guid}

# Borrow & Return (5 endpoints)
POST   /api/borrow-records
GET    /api/borrow-records
GET    /api/borrow-records/{id:guid}
POST   /api/borrow-records/{borrowRecordId:guid}/returns
POST   /api/borrow-details/{id:guid}/mark-lost

# Health (1 endpoint)
GET    /health
```
