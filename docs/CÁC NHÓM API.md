# PHÂN CHIA 6 PHÂN HỆ QUẢN LÝ VÀ API TƯƠNG ỨNG
# 1. Quản lý người dùng
Phân hệ này quản lý tài khoản người dùng trong hệ thống.
Bảng chính:
```
AspNetUsers
```
### API

| Method   | Endpoint                         | Chức năng                                        | Role             |
| -------- | -------------------------------- | ------------------------------------------------ | ---------------- |
| `GET`    | `/api/users`                     | Lấy danh sách người dùng                         | ADMIN, LIBRARIAN |
| `GET`    | `/api/users/{id}`                | Lấy thông tin chi tiết một người dùng            | ADMIN, LIBRARIAN |
| `POST`   | `/api/users`                     | Tạo tài khoản người dùng mới                     | ADMIN            |
| `PUT`    | `/api/users/{id}`                | Cập nhật thông tin người dùng                    | ADMIN            |
| `DELETE` | `/api/users/{id}`                | Xóa user nếu không bị ràng buộc bởi lịch sử mượn | ADMIN            |
| `GET`    | `/api/users/{id}/borrow-records` | Xem lịch sử mượn của một user                    | ADMIN, LIBRARIAN |
| `GET`    | `/api/users/{id}/borrowed-books` | Xem những cuốn user đang mượn                    | ADMIN, LIBRARIAN |
| `GET`    | `/api/me`                        | User xem thông tin tài khoản của chính mình      | USER             |
| `PUT`    | `/api/me`                        | User cập nhật thông tin cá nhân của mình         | USER             |
| `GET`    | `/api/me/borrow-records`         | User xem lịch sử mượn của mình                   | USER             |
| `GET`    | `/api/me/borrowed-books`         | User xem sách mình đang mượn                     | USER             |

---
# 2. Quản lý phân quyền
Phân hệ này quản lý role và việc gán role cho user.
Bảng chính:
```
AspNetRoles
AspNetUserRoles
```
Các role:
```
ADMIN
LIBRARIAN
USER
```
### API quản lý Role

|Method|Endpoint|Chức năng|Role|
|---|---|---|---|
|`GET`|`/api/roles`|Lấy danh sách role|ADMIN|
|`GET`|`/api/roles/{id}`|Xem chi tiết role|ADMIN|
|`POST`|`/api/roles`|Tạo role|ADMIN|
|`PUT`|`/api/roles/{id}`|Đổi tên role|ADMIN|
|`DELETE`|`/api/roles/{id}`|Xóa role nếu không vi phạm ràng buộc|ADMIN|

---
# 3. Quản lý danh mục sách
Phân hệ này quản lý `Categories`.
Bảng:
```
Categories
```
### API

|Method|Endpoint|Chức năng|Role|
|---|---|---|---|
|`GET`|`/api/categories`|Lấy danh sách category|Tất cả|
|`GET`|`/api/categories/{id}`|Xem chi tiết category|Tất cả|
|`POST`|`/api/categories`|Tạo category mới|ADMIN, LIBRARIAN|
|`PUT`|`/api/categories/{id}`|Cập nhật category|ADMIN, LIBRARIAN|
|`DELETE`|`/api/categories/{id}`|Xóa category|ADMIN|

---
# 4. Quản lý sách
## 4.1. API quản lý Books

|Method|Endpoint|Chức năng|Role|
|---|---|---|---|
|`GET`|`/api/books`|Lấy danh sách sách|Tất cả|
|`GET`|`/api/books/{id}`|Xem chi tiết một đầu sách|Tất cả|
|`POST`|`/api/books`|Thêm đầu sách|ADMIN, LIBRARIAN|
|`PUT`|`/api/books/{id}`|Sửa thông tin đầu sách|ADMIN, LIBRARIAN|
|`DELETE`|`/api/books/{id}`|Xóa đầu sách nếu không còn BookCopy|ADMIN|
|`GET`|`/api/books?keyword={keyword}`|Tìm kiếm sách|Tất cả|
|`GET`|`/api/books?categoryId={id}`|Lọc sách theo category|Tất cả|
|`GET`|`/api/books?author={author}`|Tìm theo tác giả|Tất cả|

---
## 4.2. API quản lý BookCopies

|Method|Endpoint|Chức năng|Role|
|---|---|---|---|
|`GET`|`/api/books/{bookId}/copies`|Xem các bản sao của đầu sách|ADMIN, LIBRARIAN|
|`POST`|`/api/books/{bookId}/copies`|Thêm một bản sao vật lý|ADMIN, LIBRARIAN|
|`GET`|`/api/book-copies/{id}`|Xem chi tiết một bản sao|ADMIN, LIBRARIAN|
|`GET`|`/api/book-copies/by-barcode/{barcode}`|Tìm sách bằng barcode|ADMIN, LIBRARIAN|
|`PUT`|`/api/book-copies/{id}`|Sửa barcode hoặc condition note|ADMIN, LIBRARIAN|
|`DELETE`|`/api/book-copies/{id}`|Xóa BookCopy nếu chưa có lịch sử mượn|ADMIN|
|`POST`|`/api/book-copies/{id}/mark-available`|Đánh dấu sách trở lại Available|ADMIN, LIBRARIAN|
|`POST`|`/api/book-copies/{id}/mark-lost`|Đánh dấu sách mất ngoài nghiệp vụ mượn|ADMIN, LIBRARIAN|

---
# 5. Quản lý mượn sách
Đây là phân hệ nghiệp vụ chính.
Bảng tham gia:
```
BorrowRecords
BorrowDetails
BookCopies
AspNetUsers
```

---
## 5.2. Danh sách API Borrowing

|Method|Endpoint|Chức năng|Role|
|---|---|---|---|
|`POST`|`/api/borrow-records`|Tạo phiếu mượn|ADMIN, LIBRARIAN|
|`GET`|`/api/borrow-records`|Lấy danh sách phiếu mượn|ADMIN, LIBRARIAN|
|`GET`|`/api/borrow-records/{id}`|Xem chi tiết phiếu|ADMIN, LIBRARIAN|
|`GET`|`/api/borrow-records?userId={id}`|Tìm phiếu theo user|ADMIN, LIBRARIAN|
|`GET`|`/api/borrow-records?status=Borrowing`|Xem phiếu đang hoạt động|ADMIN, LIBRARIAN|
|`GET`|`/api/borrow-records?overdue=true`|Xem các phiếu có sách quá hạn|ADMIN, LIBRARIAN|
|`GET`|`/api/users/{id}/borrow-records`|Xem lịch sử mượn của user|ADMIN, LIBRARIAN|
|`GET`|`/api/users/{id}/borrowed-books`|Xem sách user đang giữ|ADMIN, LIBRARIAN|
|`GET`|`/api/me/borrow-records`|User xem lịch sử của mình|USER|
|`GET`|`/api/me/borrowed-books`|User xem sách đang mượn|USER|

---
# 6. Quản lý trả sách và mất sách

|Method|Endpoint|Chức năng|Role|
|---|---|---|---|
|`POST`|`/api/borrow-details/{id}/return`|Trả một cuốn sách|ADMIN, LIBRARIAN|
|`POST`|`/api/returns`|Trả nhiều cuốn sách|ADMIN, LIBRARIAN|
|`POST`|`/api/borrow-details/{id}/mark-lost`|Báo mất sách đang mượn|ADMIN, LIBRARIAN|
|`POST`|`/api/book-copies/{id}/mark-available`|Khôi phục sách Lost thành Available|ADMIN, LIBRARIAN|
|`GET`|`/api/borrow-records?overdue=true`|Danh sách mượn quá hạn|ADMIN, LIBRARIAN|

---
# 7. Tổng hợp 6 phân hệ

|STT|Phân hệ|Bảng chính|API quan trọng|
|---|---|---|---|
|1|Quản lý người dùng|`AspNetUsers`|`/api/users`, `/api/me`|
|2|Quản lý phân quyền|`AspNetRoles`, `AspNetUserRoles`|`/api/roles`, `/api/users/{id}/roles`|
|3|Quản lý danh mục|`Categories`|`/api/categories`|
|4|Quản lý sách|`Books`, `BookCopies`|`/api/books`, `/api/book-copies`|
|5|Quản lý mượn sách|`BorrowRecords`, `BorrowDetails`, `BookCopies`|`/api/borrow-records`|
|6|Quản lý trả/mất sách|`BorrowRecords`, `BorrowDetails`, `BookCopies`|`/api/borrow-details/{id}/return`, `/mark-lost`|

---
# 8. Bộ API cốt lõi hoàn chỉnh

```
# User
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
GET    /api/me
PUT    /api/me
GET    /api/me/borrow-records
GET    /api/me/borrowed-books
# Role
GET    /api/roles
GET    /api/roles/{id}
POST   /api/roles
PUT    /api/roles/{id}
DELETE /api/roles/{id}
GET    /api/users/{userId}/roles
POST   /api/users/{userId}/roles/{roleId}
DELETE /api/users/{userId}/roles/{roleId}
# Category
GET    /api/categories
GET    /api/categories/{id}
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}
# Book
GET    /api/books
GET    /api/books/{id}
POST   /api/books
PUT    /api/books/{id}
DELETE /api/books/{id}
# BookCopy
GET    /api/books/{bookId}/copies
POST   /api/books/{bookId}/copies
GET    /api/book-copies/{id}
GET    /api/book-copies/by-barcode/{barcode}
PUT    /api/book-copies/{id}
DELETE /api/book-copies/{id}
POST   /api/book-copies/{id}/mark-available
POST   /api/book-copies/{id}/mark-lost
# Borrow
POST   /api/borrow-records
GET    /api/borrow-records
GET    /api/borrow-records/{id}
GET    /api/borrow-records?userId={userId}
GET    /api/borrow-records?status=Borrowing
GET    /api/borrow-records?overdue=true
GET    /api/users/{userId}/borrow-records
GET    /api/users/{userId}/borrowed-books
# Return / Lost
POST   /api/borrow-details/{id}/return
POST   /api/borrow-details/{id}/mark-lost
POST   /api/returns
```
