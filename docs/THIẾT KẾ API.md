Dưới đây là thiết kế request/response dựa trên database **LightBrary** bạn gửi. Các API dùng chung DTO khi trả cùng loại dữ liệu để giảm trùng lặp và dễ triển khai.
# 1. Quy ước chung

|Nội dung|Quy ước|
|---|---|
|ID|Số nguyên dương, tương ứng `INTEGER` trong database|
|JSON|Dùng `camelCase`|
|Trạng thái|Trả chuỗi như `"Available"`, `"Borrowing"`, không trả số enum|
|Thời gian|ISO 8601 có múi giờ; response thống nhất UTC, ví dụ `"2026-10-03T14:00:00Z"`|
|GET, DELETE|Không có request body|
|POST tạo tài nguyên|Trả `201 Created`, body chứa tài nguyên vừa tạo và header `Location`|
|PUT cập nhật|Trả `200 OK` với dữ liệu sau cập nhật|
|DELETE thành công|Trả `204 No Content`, không có body|
|Trường do server quản lý|Không nhận `id`, `createdAt`, `borrowDate`, `returnDate` hoặc trạng thái nghiệp vụ trong body tạo/sửa thông thường|
|PUT|Gửi đầy đủ các trường được phép cập nhật; trường nullable gửi `null` khi muốn xóa giá trị|
|Response|Trả trực tiếp DTO, không cần bọc thêm `success`, `message`, `data`|
|Tên như `UserResponse`, `BookResponse` bên dưới là tên DTO, không phải thuộc tính bao ngoài JSON.|
|**Phân trang**|
|Những endpoint ghi `Page<T>` nhận:|
```
?page=1&pageSize=20
```
Quy định `page >= 1`, `pageSize` từ `1` đến `100`; mặc định lần lượt là `1` và `20`.  
Ví dụ `Page<UserResponse>`:
```
{
  "items": [
    {
      "id": 12,
      "fullName": "Nguyễn Văn An",
      "phoneNumber": "0901234567",
      "address": null,
      "roles": ["USER"],
      "createdAt": "2026-10-03T14:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```
Danh sách rỗng trả `200 OK`, `items: []`, `totalCount: 0`. Không trả `404` chỉ vì không có kết quả.  
**Response lỗi dùng chung**
```
{
  "type": "about:blank",
  "title": "Xung đột nghiệp vụ",
  "status": 409,
  "detail": "Bản sao sách hiện không sẵn sàng để cho mượn.",
  "code": "BOOK_COPY_NOT_AVAILABLE"
}
```
Lỗi validation có thêm `errors`:
```
{
  "type": "about:blank",
  "title": "Dữ liệu không hợp lệ",
  "status": 400,
  "code": "VALIDATION_ERROR",
  "errors": {
    "fullName": ["Họ tên không được để trống."],
    "phoneNumber": ["Số điện thoại không hợp lệ."]
  }
}
```

|HTTP status|Trường hợp|
|---|---|
|`400`|Body/query không hợp lệ, danh sách ID bị trùng, enum không hợp lệ|
|`401`|Chưa đăng nhập hoặc token không hợp lệ|
|`403`|Đã đăng nhập nhưng thiếu quyền|
|`404`|Không tìm thấy tài nguyên hoặc ID tham chiếu|
|`409`|Trùng dữ liệu duy nhất, xóa khi còn ràng buộc, trạng thái không cho phép thao tác|

---


# Auth

|Method|Endpoint|Chức năng|Quyền|
|---|---|---|---|
|POST|`/api/auth/register`|Độc giả tự đăng ký tài khoản|Công khai|
|POST|`/api/auth/login`|Đăng nhập và nhận access token|Công khai|
|POST|`/api/auth/change-password`|Đổi mật khẩu của chính mình|Đã đăng nhập|
**1. Đăng ký tài khoản**
```
POST /api/auth/register
Content-Type: application/json
```
Request — `RegisterRequest`:
```
{
  "fullName": "Nguyễn Văn An",
  "phoneNumber": "0901234567",
  "address": "Quận 11, TP.HCM",
  "password": "Example@12345"
}
```

|Trường|Quy định|
|---|---|
|`fullName`|Bắt buộc, không chỉ chứa khoảng trắng, tối đa 150 ký tự|
|`phoneNumber`|Bắt buộc, tối đa 50 ký tự, chuẩn hóa trước khi kiểm tra duy nhất|
|`address`|Không bắt buộc, cho phép `null`, tối đa 255 ký tự|
|`password`|Bắt buộc, đáp ứng chính sách mật khẩu của ứng dụng|
Không nhận `role`, `roleIds`, `passwordHash` hoặc `createdAt` từ client.
`confirmPassword` có thể kiểm tra ở giao diện, không cần đưa vào hợp đồng API.
Response — `201 Created`, sử dụng lại `UserResponse`:
```
{
  "id": 12,
  "fullName": "Nguyễn Văn An",
  "phoneNumber": "0901234567",
  "address": "Quận 11, TP.HCM",
  "roles": ["USER"],
  "createdAt": "2026-10-03T14:55:00Z"
}
```
Header:
```
Location: /api/me
```

Quy tắc xử lý:
- Băm mật khẩu.
- Tạo tài khoản và gán role `USER` trong cùng transaction.
- Chỉ trả thành công khi cả tài khoản và phép gán role đã được lưu.

|Trường hợp lỗi|HTTP status|Code|
|---|---|---|
|Dữ liệu hoặc mật khẩu không hợp lệ|`400`|`VALIDATION_ERROR`|
|Số điện thoại đã được sử dụng|`409`|`PHONE_NUMBER_EXISTS`|
**Khác biệt với `POST /api/users`:** `/api/auth/register` dành cho người dùng tự đăng ký và luôn được gán `USER`; `/api/users` dành cho quản trị viên tạo tài khoản.
**2. Đăng nhập**
```
POST /api/auth/login
Content-Type: application/json
```
Request — `LoginRequest`:
```
{
  "userName": "0901234567",
  "password": "Example@12345"
}
```

|Trường|Quy định|
|---|---|
|`userName`|Bắt buộc, tối đa 256 ký tự; bỏ khoảng trắng đầu/cuối. Tài khoản hiện tại dùng số điện thoại làm tên đăng nhập|
|`password`|Bắt buộc; giữ nguyên giá trị, không tự trim hoặc đổi chữ hoa/chữ thường|
Response — `200 OK`, `LoginResponse`:
```
{
  "accessToken": "<jwt-access-token>",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "user": {
    "id": 12,
    "fullName": "Nguyễn Văn An",
    "phoneNumber": "0901234567",
    "address": "Quận 11, TP.HCM",
    "roles": ["USER"],
    "createdAt": "2026-10-03T14:55:00Z"
  }
}
```

|Trường|Ý nghĩa|
|---|---|
|`accessToken`|JWT dùng để xác thực những request tiếp theo|
|`tokenType`|Loại token, giá trị `Bearer`|
|`expiresIn`|Thời gian sống của token, tính bằng giây; `900` là ví dụ 15 phút|
|`user`|Thông tin tài khoản, dùng lại `UserResponse`|
Client gửi token khi gọi API được bảo vệ:
```
GET /api/me
Authorization: Bearer <jwt-access-token>
```
JWT cần chứa định danh user và các role được server đọc từ database. Không nhận role do client gửi để phát hành token.
Số điện thoại không tồn tại hoặc mật khẩu không đúng đều trả cùng lỗi:
```
{
  "type": "about:blank",
  "title": "Đăng nhập thất bại",
  "status": 401,
  "detail": "Số điện thoại hoặc mật khẩu không đúng.",
  "code": "INVALID_CREDENTIALS"
}
```
Không trả lỗi riêng như “Số điện thoại chưa đăng ký” trong API đăng nhập.

# 2. Quản lý người dùng
## 2.1. Response dùng chung: `UserResponse`
```
{
  "id": 12,
  "fullName": "Nguyễn Văn An",
  "phoneNumber": "0901234567",
  "address": "Quận 11, TP.HCM",
  "roles": ["USER"],
  "createdAt": "2026-10-03T14:00:00Z"
}
```
`roles` lấy từ `AspNetUserRoles` và `AspNetRoles`. Không trả `password`, `passwordHash` hoặc các trường bảo mật nội bộ của Identity.
## 2.2. Request tạo user: `CreateUserRequest`
```
{
  "fullName": "Nguyễn Văn An",
  "phoneNumber": "0901234567",
  "address": "Quận 11, TP.HCM",
  "password": "Example@12345"
}
```

|Trường|Quy định|
|---|---|
|`fullName`|Bắt buộc, không chỉ chứa khoảng trắng, tối đa 150 ký tự|
|`phoneNumber`|Bắt buộc, tối đa 50 ký tự, chuẩn hóa trước khi kiểm tra trùng|
|`address`|Cho phép bỏ qua hoặc `null`, tối đa 255 ký tự|
|`password`|Bắt buộc, theo chính sách mật khẩu của ứng dụng|
|**Đề xuất:** user mới mặc định được gán role `USER`. Tạo user và gán role trong cùng transaction. Muốn gán quyền khác thì sử dụng API phân quyền riêng.|
## 2.3. Request cập nhật: `UpdateUserRequest`
Dùng chung cho `PUT /api/users/{id}` và `PUT /api/me`:
```
{
  "fullName": "Nguyễn Văn An",
  "phoneNumber": "0901234567",
  "address": null
}
```
Không nhận `password`, `roles` hoặc `createdAt`. Đổi mật khẩu là nghiệp vụ riêng, không đặt trong API sửa thông tin cá nhân.
## 2.4. Hợp đồng từng API

|Method|Endpoint|Request|Response thành công|
|---|---|---|---|
|GET|`/api/users`|Query: `keyword?`, `role?`, `page`, `pageSize`|`200` — `Page<UserResponse>`|
|GET|`/api/users/{id}`|Path: `id`|`200` — `UserResponse`|
|POST|`/api/users`|Body: `CreateUserRequest`|`201` — `UserResponse`|
|PUT|`/api/users/{id}`|Path: `id`; body: `UpdateUserRequest`|`200` — `UserResponse`|
|DELETE|`/api/users/{id}`|Path: `id`|`204`|
|GET|`/api/me`|Không có body|`200` — `UserResponse`|
|PUT|`/api/me`|Body: `UpdateUserRequest`|`200` — `UserResponse`|
|Quy ước tìm kiếm:|
- `keyword`: tìm theo họ tên hoặc số điện thoại.
- `role`: tên role, ví dụ `USER`.
- Sắp xếp mặc định: `createdAt DESC, id DESC`.  
    Ví dụ:
```
GET /api/users?keyword=An&role=USER&page=1&pageSize=20
```
Quy tắc nghiệp vụ:
- Trùng số điện thoại: `409 PHONE_NUMBER_EXISTS`.
- User có bất kỳ lịch sử mượn nào: chặn xóa, trả `409 USER_HAS_BORROW_HISTORY`.
- Khi được phép xóa user, các bản ghi gán role được xóa theo quan hệ cascade.
- Chặn xóa user nếu đó là tài khoản `ADMIN` cuối cùng.
- `/api/me` lấy user ID từ danh tính đã xác thực, không nhận user ID từ client.  
    **Đề xuất cho `/api/me`:** cho phép mọi người dùng đã đăng nhập, gồm `ADMIN`, `LIBRARIAN`, `USER`. Nếu chỉ cho role `USER`, tài khoản chỉ mang role `LIBRARIAN` sẽ không xem được hồ sơ của chính mình.  
    Các API lịch sử mượn và sách đang mượn được định nghĩa chung tại phần 6.

---
# 3. Quản lý role và gán role
## 3.1. Response dùng chung: `RoleResponse`
```
{
  "id": 2,
  "name": "LIBRARIAN"
}
```
## 3.2. Request tạo/sửa role: `SaveRoleRequest`
```
{
  "name": "ASSISTANT"
}
```
`name` bắt buộc, tối đa 256 ký tự. Đề xuất chuẩn hóa về chữ hoa và kiểm tra duy nhất sau chuẩn hóa.
## 3.3. Hợp đồng từng API
Tất cả endpoint trong phần này dành cho `ADMIN`.

|Method|Endpoint|Request|Response thành công|
|---|---|---|---|
|GET|`/api/roles`|Không có body|`200` — `RoleResponse[]`|
|GET|`/api/roles/{id}`|Path: `id`|`200` — `RoleResponse`|
|POST|`/api/roles`|Body: `SaveRoleRequest`|`201` — `RoleResponse`|
|PUT|`/api/roles/{id}`|Path: `id`; body: `SaveRoleRequest`|`200` — `RoleResponse`|
|DELETE|`/api/roles/{id}`|Path: `id`|`204`|
|GET|`/api/users/{userId}/roles`|Path: `userId`|`200` — `RoleResponse[]`|
|POST|`/api/users/{userId}/roles/{roleId}`|Path: `userId`, `roleId`; không body|`204`|
|DELETE|`/api/users/{userId}/roles/{roleId}`|Path: `userId`, `roleId`; không body|`204`|
|Ví dụ response danh sách role của user:|
```
[
  {
    "id": 2,
    "name": "LIBRARIAN"
  },
  {
    "id": 3,
    "name": "USER"
  }
]
```
Quy tắc:
- Gán role đã được gán: trả `204`, không tạo bản ghi trùng.
- Gỡ role chưa được gán: trả `204`.
- User hoặc role không tồn tại: trả `404`.
- Role đang được gán cho user: chặn xóa với `409 ROLE_IN_USE`.
- Tên role trùng: `409 ROLE_NAME_EXISTS`.
- Chặn gỡ role `ADMIN` khỏi quản trị viên cuối cùng.  
    **Đề xuất bảo vệ ba role hệ thống:** `ADMIN`, `LIBRARIAN`, `USER` không được đổi tên hoặc xóa vì các API đang dựa vào đúng những tên này để phân quyền.  
    Role tự tạo như `ASSISTANT` không tự động có quyền gọi API. Với mô hình hiện tại, quyền truy cập vẫn phải được khai báo trong ứng dụng. Nếu hệ thống chỉ cần đúng ba role cố định, có thể bỏ API tạo/sửa/xóa role và chỉ giữ API xem/gán/gỡ.

---
# 4. Quản lý danh mục
## 4.1. Response: `CategoryResponse`
```
{
  "id": 1,
  "name": "Công nghệ thông tin",
  "description": "Sách về lập trình và hệ thống phần mềm."
}
```
## 4.2. Request tạo/sửa: `SaveCategoryRequest`
```
{
  "name": "Công nghệ thông tin",
  "description": "Sách về lập trình và hệ thống phần mềm."
}
```

|Trường|Quy định|
|---|---|
|`name`|Bắt buộc, không rỗng, tối đa 100 ký tự, duy nhất|
|`description`|Nullable, tối đa 255 ký tự|
## 4.3. Hợp đồng từng API

|Method|Endpoint|Request|Response thành công|
|---|---|---|---|
|GET|`/api/categories`|Query: `keyword?`, `page`, `pageSize`|`200` — `Page<CategoryResponse>`|
|GET|`/api/categories/{id}`|Path: `id`|`200` — `CategoryResponse`|
|POST|`/api/categories`|Body: `SaveCategoryRequest`|`201` — `CategoryResponse`|
|PUT|`/api/categories/{id}`|Path: `id`; body: `SaveCategoryRequest`|`200` — `CategoryResponse`|
|DELETE|`/api/categories/{id}`|Path: `id`|`204`|
|`keyword` tìm theo tên; sắp xếp mặc định `name ASC, id ASC`.|
|Tên bị trùng trả `409 CATEGORY_NAME_EXISTS`. Danh mục còn sách trả `409 CATEGORY_HAS_BOOKS` khi xóa.|

---
# 5. Quản lý sách và bản sao
## 5.1. Response đầu sách: `BookResponse`
Dùng chung cho danh sách, chi tiết, tạo và cập nhật:
```
{
  "id": 10,
  "title": "Lập trình C# căn bản",
  "author": "Nguyễn Văn Bình",
  "isbn": "9786041234567",
  "publishYear": 2025,
  "category": {
    "id": 1,
    "name": "Công nghệ thông tin"
  },
  "totalCopies": 5,
  "availableCopies": 3,
  "createdAt": "2026-10-03T14:00:00Z"
}
```
`totalCopies` và `availableCopies` là dữ liệu tính từ `BookCopies`, không phải cột mới cần thêm vào `Books`.  
Không đưa toàn bộ danh sách bản sao vào response đầu sách. Dùng endpoint `/api/books/{bookId}/copies` khi cần.
## 5.2. Request tạo/sửa đầu sách: `SaveBookRequest`
```
{
  "title": "Lập trình C# căn bản",
  "author": "Nguyễn Văn Bình",
  "isbn": "9786041234567",
  "publishYear": 2025,
  "categoryId": 1
}
```

|Trường|Quy định|
|---|---|
|`title`|Bắt buộc, tối đa 250 ký tự|
|`author`|Bắt buộc, tối đa 150 ký tự|
|`isbn`|Bắt buộc, tối đa 50 ký tự, duy nhất sau chuẩn hóa|
|`publishYear`|Nullable; nếu có phải lớn hơn 0|
|`categoryId`|Bắt buộc, danh mục phải tồn tại|
|Các chuỗi bắt buộc không được chỉ chứa khoảng trắng.|
## 5.3. Hợp đồng API đầu sách

|Method|Endpoint|Request|Response thành công|
|---|---|---|---|
|GET|`/api/books`|Query: `keyword?`, `categoryId?`, `author?`, `page`, `pageSize`|`200` — `Page<BookResponse>`|
|GET|`/api/books/{id}`|Path: `id`|`200` — `BookResponse`|
|POST|`/api/books`|Body: `SaveBookRequest`|`201` — `BookResponse`|
|PUT|`/api/books/{id}`|Path: `id`; body: `SaveBookRequest`|`200` — `BookResponse`|
|DELETE|`/api/books/{id}`|Path: `id`|`204`|
|Ví dụ kết hợp tìm kiếm và lọc:|
```
GET /api/books?keyword=C%23&categoryId=1&author=Bình&page=1&pageSize=20
```
Quy ước:
- `keyword` tìm theo tiêu đề hoặc ISBN.
- `author` tìm theo tên tác giả.
- Các bộ lọc kết hợp bằng điều kiện `AND`.
- Sắp xếp mặc định `createdAt DESC, id DESC`.  
    Những URL tìm kiếm trong danh sách ban đầu là **cùng một API `GET /api/books`**, chỉ khác query parameters.  
    Trùng ISBN trả `409 ISBN_EXISTS`. Còn bất kỳ `BookCopy` nào thì chặn xóa đầu sách, trả `409 BOOK_HAS_COPIES`.
## 5.4. Response bản sao: `BookCopyResponse`
```
{
  "id": 101,
  "bookId": 10,
  "bookTitle": "Lập trình C# căn bản",
  "barcode": "LB-000101",
  "status": "Available",
  "conditionNote": "Sách mới."
}
```
## 5.5. Request tạo/sửa bản sao: `SaveBookCopyRequest`
```
{
  "barcode": "LB-000101",
  "conditionNote": "Sách mới."
}
```
- `barcode`: bắt buộc, tối đa 50 ký tự, duy nhất.
- `conditionNote`: nullable, tối đa 255 ký tự.
- Khi tạo, `bookId` lấy từ URL và trạng thái mặc định là `Available`.
- Khi sửa, không cho đổi `bookId` hoặc tự đặt `status`.
## 5.6. Hợp đồng API bản sao

|Method|Endpoint|Request|Response thành công|
|---|---|---|---|
|GET|`/api/books/{bookId}/copies`|Path: `bookId`; query: `status?`, `page`, `pageSize`|`200` — `Page<BookCopyResponse>`|
|POST|`/api/books/{bookId}/copies`|Path: `bookId`; body: `SaveBookCopyRequest`|`201` — `BookCopyResponse`|
|GET|`/api/book-copies/{id}`|Path: `id`|`200` — `BookCopyResponse`|
|GET|`/api/book-copies/by-barcode/{barcode}`|Path: `barcode`|`200` — `BookCopyResponse`|
|PUT|`/api/book-copies/{id}`|Path: `id`; body: `SaveBookCopyRequest`|`200` — `BookCopyResponse`|
|DELETE|`/api/book-copies/{id}`|Path: `id`|`204`|
|POST|`/api/book-copies/{id}/mark-lost`|Path: `id`; không body|`200` — `BookCopyResponse`|
|POST|`/api/book-copies/{id}/mark-available`|Path: `id`; không body|`200` — `BookCopyResponse`|
|Danh sách bản sao sắp xếp `id ASC`. `status` nhận `Available`, `Borrowed`, `Lost`.|
|Ví dụ:|
```
GET /api/books/10/copies?status=Available&page=1&pageSize=20
```
Quy tắc:

|Thao tác|Điều kiện|
|---|---|
|Xóa bản sao|Chưa có bất kỳ `BorrowDetail` nào tham chiếu|
|Báo mất ngoài nghiệp vụ mượn|Bản sao đang `Available` và không có lượt mượn đang hoạt động|
|Khôi phục bản sao|Bản sao đang `Lost` và không có lượt mượn đang hoạt động|
|Bản sao đang `Borrowed`|Phải xử lý bằng API trả sách hoặc báo mất theo `BorrowDetail`|
|Barcode trùng trả `409 BARCODE_EXISTS`. Có lịch sử mượn trả `409 BOOK_COPY_HAS_BORROW_HISTORY`. Trạng thái không phù hợp trả `409 INVALID_BOOK_COPY_STATE`.|
|Để hợp đồng rõ ràng, các API chuyển trạng thái ở đây trả `409` nếu tài nguyên đã ở trạng thái đích.|

---
# 6. Quản lý mượn sách
## 6.1. Quy tắc trạng thái phiếu
Database có ba trạng thái phiếu nhưng chưa xác định rõ trường hợp trả một phần và mất một phần. Đề xuất thống nhất:

|Tình trạng các chi tiết|`BorrowRecord.status`|
|---|---|
|Còn ít nhất một chi tiết `Borrowing`|`Borrowing`|
|Tất cả chi tiết đều `Returned`|`Returned`|
|Không còn chi tiết `Borrowing`, có ít nhất một chi tiết `Lost`|`Lost`|
|Ví dụ: phiếu có một cuốn đã mất và một cuốn vẫn đang mượn thì phiếu vẫn là `Borrowing`. Các trường đếm trong response thể hiện rõ cuốn bị mất.|
|`isOverdue` là giá trị tính toán: **phiếu còn chi tiết `Borrowing` và thời điểm hiện tại đã vượt `dueDate`**. Không thêm trạng thái `Overdue` vào database.|
## 6.2. Request tạo phiếu: `CreateBorrowRecordRequest`
```
{
  "userId": 12,
  "bookCopyIds": [101, 102],
  "dueDate": "2026-10-17T14:00:00Z",
  "note": "Độc giả mượn tại quầy."
}
```

|Trường|Quy định|
|---|---|
|`userId`|Bắt buộc, người mượn phải tồn tại|
|`bookCopyIds`|Bắt buộc, có ít nhất một ID, không trùng ID|
|`dueDate`|Bắt buộc, có múi giờ, sau thời điểm tạo phiếu|
|`note`|Nullable, tối đa 255 ký tự|
|Client gửi **ID bản sao vật lý**, không gửi `bookId`. Hai bản sao cùng một đầu sách là hai tài nguyên khác nhau.|
|Server tự đặt:|
- `borrowDate`: thời điểm hiện tại.
- Phiếu và các chi tiết: `Borrowing`.
- Các bản sao được mượn: `Borrowed`.
- `returnDate`: `null`.  
    Việc tạo phiếu, tạo chi tiết và cập nhật bản sao phải cùng transaction. Nếu một bản sao không thể mượn, toàn bộ yêu cầu thất bại.
## 6.3. Response danh sách: `BorrowRecordSummaryResponse`
```
{
  "id": 500,
  "user": {
    "id": 12,
    "fullName": "Nguyễn Văn An"
  },
  "borrowDate": "2026-10-03T14:00:00Z",
  "dueDate": "2026-10-17T14:00:00Z",
  "status": "Borrowing",
  "totalBooks": 2,
  "borrowingCount": 2,
  "returnedCount": 0,
  "lostCount": 0,
  "isOverdue": false
}
```
Các trường đếm được tính từ `BorrowDetails`.
## 6.4. Response chi tiết: `BorrowRecordResponse`
```
{
  "id": 500,
  "user": {
    "id": 12,
    "fullName": "Nguyễn Văn An"
  },
  "borrowDate": "2026-10-03T14:00:00Z",
  "dueDate": "2026-10-17T14:00:00Z",
  "status": "Borrowing",
  "note": "Độc giả mượn tại quầy.",
  "isOverdue": false,
  "details": [
    {
      "id": 1001,
      "bookCopyId": 101,
      "barcode": "LB-000101",
      "bookId": 10,
      "bookTitle": "Lập trình C# căn bản",
      "status": "Borrowing",
      "returnDate": null
    },
    {
      "id": 1002,
      "bookCopyId": 102,
      "barcode": "LB-000102",
      "bookId": 11,
      "bookTitle": "Cơ sở dữ liệu",
      "status": "Borrowing",
      "returnDate": null
    }
  ]
}
```
`details[].id` là **ID của `BorrowDetail`**, dùng khi trả hoặc báo mất sách đang mượn.
## 6.5. Response sách đang mượn: `BorrowedBookResponse`
```
{
  "borrowDetailId": 1001,
  "borrowRecordId": 500,
  "bookCopyId": 101,
  "barcode": "LB-000101",
  "bookId": 10,
  "bookTitle": "Lập trình C# căn bản",
  "borrowDate": "2026-10-03T14:00:00Z",
  "dueDate": "2026-10-17T14:00:00Z",
  "isOverdue": false
}
```
Danh sách này chỉ lấy những chi tiết có `status = Borrowing`. Không lấy cuốn đã trả hoặc đã ghi nhận mất.
## 6.6. Hợp đồng từng API

|Method|Endpoint|Request|Response thành công|
|---|---|---|---|
|POST|`/api/borrow-records`|Body: `CreateBorrowRecordRequest`|`201` — `BorrowRecordResponse`|
|GET|`/api/borrow-records`|Query: `userId?`, `status?`, `overdue?`, `page`, `pageSize`|`200` — `Page<BorrowRecordSummaryResponse>`|
|GET|`/api/borrow-records/{id}`|Path: `id`|`200` — `BorrowRecordResponse`|
|GET|`/api/users/{userId}/borrow-records`|Path: `userId`; query: `status?`, `overdue?`, `page`, `pageSize`|`200` — `Page<BorrowRecordSummaryResponse>`|
|GET|`/api/users/{userId}/borrowed-books`|Path: `userId`; query: `overdue?`, `page`, `pageSize`|`200` — `Page<BorrowedBookResponse>`|
|GET|`/api/me/borrow-records`|Query: `status?`, `overdue?`, `page`, `pageSize`|`200` — `Page<BorrowRecordSummaryResponse>`|
|GET|`/api/me/borrowed-books`|Query: `overdue?`, `page`, `pageSize`|`200` — `Page<BorrowedBookResponse>`|
|Ví dụ:|
```
GET /api/borrow-records?userId=12&status=Borrowing&overdue=true&page=1&pageSize=20
```
Quy ước:
- `status`: `Borrowing`, `Returned`, `Lost`.
- Không gửi `overdue`: không lọc quá hạn.
- `overdue=true`: chỉ lấy bản ghi có `isOverdue = true`.
- `overdue=false`: chỉ lấy bản ghi có `isOverdue = false`.
- Bộ lọc kết hợp bằng `AND`.
- Lịch sử phiếu sắp xếp `borrowDate DESC, id DESC`.
- Sách đang mượn sắp xếp `dueDate ASC, borrowDetailId ASC`.
- Endpoint `/api/users/{userId}/...` trả `404` nếu user không tồn tại.
- Endpoint `/api/me/...` lấy user từ danh tính đã xác thực.  
    Các API thuộc `/api/me` không nhận thêm `userId` để chọn người khác.  
    Danh sách hiện tại chưa có API cho độc giả xem chi tiết một phiếu của mình. Nếu giao diện cần, bổ sung:
```
GET /api/me/borrow-records/{id}
```
Trả `200 BorrowRecordResponse`; chỉ tìm trong phiếu thuộc user hiện tại. Phiếu không tồn tại hoặc thuộc người khác đều trả `404`.

---
# 7. Quản lý trả sách và mất sách
## 7.1. Trả một cuốn sách
```
POST /api/borrow-details/1001/return
```
Không có request body. Server tự ghi nhận thời điểm trả.  
Response `200 OK` — `BorrowDetailActionResponse`:
```
{
  "id": 1001,
  "borrowRecordId": 500,
  "bookCopyId": 101,
  "status": "Returned",
  "returnDate": "2026-10-10T09:00:00Z",
  "bookCopyStatus": "Available",
  "borrowRecordStatus": "Borrowing"
}
```
Response trả cả ba trạng thái để client cập nhật giao diện mà không phải gọi lại nhiều API.  
Trong một transaction, server cập nhật chi tiết thành `Returned`, ghi `returnDate`, cập nhật bản sao thành `Available` và tính lại trạng thái phiếu.  
Với hợp đồng này, chỉ chi tiết đang `Borrowing` được trả. Chi tiết đã `Returned` hoặc `Lost` trả `409 INVALID_BORROW_DETAIL_STATE`.
## 7.2. Trả nhiều cuốn sách
```
POST /api/returns
```
Request — `ReturnBooksRequest`:
```
{
  "borrowDetailIds": [1001, 1002]
}
```
Quy tắc:
- Danh sách không rỗng, không trùng ID.
- Có thể chứa chi tiết thuộc nhiều phiếu.
- Tất cả chi tiết phải đang `Borrowing`.
- Xử lý toàn bộ trong một transaction.
- Chỉ cần một chi tiết không hợp lệ thì không cập nhật chi tiết nào.  
    Response `200 OK`:
```
{
  "items": [
    {
      "id": 1001,
      "borrowRecordId": 500,
      "bookCopyId": 101,
      "status": "Returned",
      "returnDate": "2026-10-10T09:00:00Z",
      "bookCopyStatus": "Available",
      "borrowRecordStatus": "Returned"
    },
    {
      "id": 1002,
      "borrowRecordId": 500,
      "bookCopyId": 102,
      "status": "Returned",
      "returnDate": "2026-10-10T09:00:00Z",
      "bookCopyStatus": "Available",
      "borrowRecordStatus": "Returned"
    }
  ]
}
```
`borrowRecordStatus` phản ánh trạng thái **sau khi hoàn tất toàn bộ lần trả**, không phải trạng thái trung gian khi xử lý từng dòng.  
API này cập nhật các bảng hiện có, không cần thêm bảng `Returns`.
## 7.3. Báo mất sách đang mượn
```
POST /api/borrow-details/1001/mark-lost
```
Không có request body.  
Response `200 OK` — `BorrowDetailActionResponse`:
```
{
  "id": 1001,
  "borrowRecordId": 500,
  "bookCopyId": 101,
  "status": "Lost",
  "returnDate": null,
  "bookCopyStatus": "Lost",
  "borrowRecordStatus": "Borrowing"
}
```
Điều kiện: chi tiết đang `Borrowing`.  
Trong cùng transaction, server chuyển chi tiết và bản sao thành `Lost`, giữ `returnDate = null`, rồi tính lại trạng thái phiếu. Phiếu trong ví dụ vẫn `Borrowing` vì còn cuốn khác chưa trả.  
Database hiện chưa có cột lưu lý do hoặc thời điểm báo mất, nên request không thêm `lostReason` hoặc `lostAt`.
## 7.4. Khôi phục sách đã mất
Dùng lại API đã định nghĩa:
```
POST /api/book-copies/101/mark-available
```
Response `200 OK` — `BookCopyResponse`:
```
{
  "id": 101,
  "bookId": 10,
  "bookTitle": "Lập trình C# căn bản",
  "barcode": "LB-000101",
  "status": "Available",
  "conditionNote": "Sách mới."
}
```
**Quy ước đề xuất cho schema hiện tại:** thao tác này xác nhận bản sao đã trở lại kho và có thể cho mượn. Lượt mượn từng báo mất vẫn giữ `Lost` để bảo toàn kết quả xử lý trước đó.  
Nếu muốn ghi nhận “độc giả trả lại cuốn đã báo mất” và sửa kết quả lượt mượn, cần định nghĩa một nghiệp vụ khôi phục riêng. API khôi phục bản sao hiện tại không tự thay đổi lịch sử mượn.
## 7.5. Tổng hợp API trả/mất

|Method|Endpoint|Body|Response thành công|
|---|---|---|---|
|POST|`/api/borrow-details/{id}/return`|Không có|`200` — `BorrowDetailActionResponse`|
|POST|`/api/returns`|`ReturnBooksRequest`|`200` — `{ "items": BorrowDetailActionResponse[] }`|
|POST|`/api/borrow-details/{id}/mark-lost`|Không có|`200` — `BorrowDetailActionResponse`|
|POST|`/api/book-copies/{id}/mark-lost`|Không có|`200` — `BookCopyResponse`|
|POST|`/api/book-copies/{id}/mark-available`|Không có|`200` — `BookCopyResponse`|
|GET|`/api/borrow-records?overdue=true`|Không có|`200` — `Page<BorrowRecordSummaryResponse>`|
# 8. Các điểm cần giữ khi triển khai
DTO request chỉ chứa trường được phép ghi. Đặc biệt, không cho client tự gửi trạng thái để bỏ qua nghiệp vụ mượn, trả hoặc mất sách.  
Transaction cần kết hợp với khóa dòng hoặc cập nhật có điều kiện khi mượn bản sao; chỉ kiểm tra `Available` rồi cập nhật sau đó chưa đủ để ngăn hai request cùng mượn một cuốn. Partial unique index cho `BorrowDetails.BookCopyId` khi `Status = Borrowing` trong schema là lớp bảo vệ bổ sung.  
Các thao tác trả/mất nhiều cuốn cùng phiếu cũng cần đồng bộ việc tính lại trạng thái phiếu, để `BorrowRecords.Status` luôn khớp với các chi tiết sau khi transaction hoàn tất.  
Phân quyền giữ theo danh sách bạn đưa, với đề xuất mở `/api/me` cho mọi tài khoản đã xác thực. Các endpoint ghi “Tất cả” được hiểu là cho phép cả khách chưa đăng nhập.