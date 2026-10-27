# Cơ sở dữ liệu hiện tại

Nguồn xác thực: `code/Entities/`, `code/Data/AppDbContext.cs` và migration `Migrations/20261022030516_chantroimoi.cs`. PostgreSQL thông qua Npgsql/EF Core; ID của các bảng nghiệp vụ, người dùng và vai trò dùng `Guid`/`uuid`; một số bảng phụ của Identity dùng ID số nguyên. ASP.NET Core Identity tạo thêm các bảng user, role, claims, logins và tokens.

| Bảng | Dữ liệu và quan hệ chính |
|---|---|
| `AspNetUsers` | Identity user với `FullName`, `Address`, `PhoneNumber`, `CreatedAt`; username dùng để đăng nhập. PhoneNumber có unique index. |
| `AspNetRoles`, `AspNetUserRoles` | Role và quan hệ nhiều-nhiều giữa user và role. |
| `Categories` | `Name` tối đa 20 ký tự, `Description` tối đa 255; tên có unique index. |
| `Books` | `Title` tối đa 50, `Author` tối đa 100, `ISBN` tối đa 50 và unique, `ImageUrl` tối đa 500, `PublishYear`, `CreatedAt`, `CategoryId`. |
| `BookCopies` | Mã vạch unique, trạng thái, ghi chú và `BookId`. Một đầu sách có nhiều bản sao. |
| `BorrowRecords` | `UserId`, ngày mượn, hạn trả, ghi chú, trạng thái `Borrowing`/`Closed`. |
| `BorrowDetails` | Thuộc phiếu; tham chiếu bản sao nullable, có ngày trả và trạng thái. Enum có `Borrowing`, `Returned`, `Lost`, `Damaged`, `Over`; luồng hiện tại dùng ba giá trị đầu. |


## Ràng buộc quan hệ

| Quan hệ | Hành vi trong database | Tác động |
|---|---|---|
| `Categories` → `Books` | `Restrict` | Không xóa được thể loại còn sách tham chiếu. |
| `Books` → `BookCopies` | `Cascade` | Xóa đầu sách sẽ xóa các bản sao liên quan. |
| `AspNetUsers` → `BorrowRecords` | `Cascade` | Xóa người dùng sẽ xóa các phiếu mượn liên quan. |
| `BorrowRecords` → `BorrowDetails` | `Cascade` | Xóa phiếu sẽ xóa các dòng chi tiết. |
| `BookCopies` → `BorrowDetails` | `SetNull` | Xóa bản sao giữ lại chi tiết nhưng đặt `BookCopyId` thành null. |
| `AspNetUsers` / `AspNetRoles` → `AspNetUserRoles` | `Cascade` | Xóa user hoặc role sẽ xóa các dòng gán role liên quan. |

Service còn kiểm tra trước một số thao tác: ví dụ chặn xóa thể loại còn sách và xóa bản sao có lịch sử mượn. Tuy vậy, xóa gián tiếp qua cascade có thể ảnh hưởng lịch sử; không nên hiểu các kiểm tra này là bảo đảm giữ toàn bộ dữ liệu lịch sử.
