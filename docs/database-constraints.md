# ĐẶC TẢ RÀNG BUỘC CƠ SỞ DỮ LIỆU (DATABASE CONSTRAINTS)

Tài liệu này tổng hợp đầy đủ các ràng buộc toàn vẹn, chỉ mục, quan hệ và enum thực tế trong CSDL PostgreSQL của dự án **LightBrary** (đồng bộ trực tiếp với `AppDbContextModelSnapshot.cs` và các Enums trong mã nguồn).

---

## 1. Bảng Tổng Hợp Khóa Duy Nhất (`UNIQUE`)

| Bảng | Cột áp dụng | Loại ràng buộc | Ý nghĩa nghiệp vụ |
| :--- | :--- | :--- | :--- |
| `AspNetUsers` | `PhoneNumber` | `UNIQUE` Index | Mỗi tài khoản sinh viên/nhân viên gắn với một số điện thoại duy nhất. |
| `AspNetUsers` | `NormalizedUserName` | `UNIQUE` Index | Mã số sinh viên/tên đăng nhập không được trùng lặp. |
| `AspNetUsers` | `NormalizedEmail` | Non-Unique / Filtered Index | Tối ưu tìm kiếm theo email người dùng. |
| `AspNetRoles` | `NormalizedName` | `UNIQUE` Index | Tên vai trò (`ADMIN`, `LIBRARIAN`, `USER`) không trùng lặp. |
| `AspNetUserRoles` | `(UserId, RoleId)` | Composite PK | Một người dùng chỉ nhận một vai trò một lần duy nhất. |
| `Categories` | `Name` | `UNIQUE` Index | Tên thể loại sách không được trùng lặp. |
| `Books` | `ISBN` | `UNIQUE` Index | Mã tiêu chuẩn quốc tế ISBN là duy nhất cho mỗi đầu sách. |
| `BookCopies` | `Barcode` | `UNIQUE` Index | Mỗi bản sao vật lý trên kệ có một mã vạch Barcode riêng biệt. |

---

## 2. Bảng Tổng Hợp Ràng Buộc `NOT NULL` và `NULL`

| Bảng | Các cột bắt buộc (`NOT NULL`) | Các cột cho phép `NULL` |
| :--- | :--- | :--- |
| `AspNetUsers` | `Id`, `UserName`, `FullName`, `PhoneNumber`, `Address`, `PasswordHash`, `SecurityStamp`, `CreatedAt` | `Email`, `NormalizedEmail`, `LockoutEnd` |
| `AspNetRoles` | `Id`, `Name`, `NormalizedName` | `ConcurrencyStamp` |
| `AspNetUserRoles` | `UserId`, `RoleId` | Không có |
| `Categories` | `Id`, `Name` | `Description` |
| `Books` | `Id`, `Title`, `Author`, `ISBN`, `CategoryId`, `CreatedAt` | `ImageUrl`, `PublishYear` |
| `BookCopies` | `Id`, `BookId`, `Barcode`, `Status` | `ConditionNote` |
| `BorrowRecords` | `Id`, `UserId`, `BorrowDate`, `DueDate`, `Status` | `Note` |
| `BorrowDetails` | `Id`, `BorrowRecordId`, `Status` | `BookCopyId` *(SetNull khi xóa bản sao)*, `ReturnDate` |

---

## 3. Bảng Tổng Hợp Quan Hệ Giữa Các Bảng (Relationship Matrix)

| Bảng nguồn | Bảng đích | Quan hệ | CSDL / EF Core Delete Behavior | Kiểm soát tầng Nghiệp vụ (Service) |
| :--- | :--- | :---: | :--- | :--- |
| `AspNetUsers` | `AspNetUserRoles` | 1 - N | `Cascade Delete` | Xóa người dùng xóa luôn vai trò liên kết |
| `AspNetRoles` | `AspNetUserRoles` | 1 - N | `Cascade Delete` | Xóa vai trò xóa luôn liên kết |
| `Categories` | `Books` | 1 - N | `Restrict Delete` | `CategoryService.DeleteAsync`: Chặn xóa nếu danh mục còn chứa đầu sách |
| `Books` | `BookCopies` | 1 - N | `Cascade Delete` | `BookService.DeleteAsync`: Chặn xóa nếu đầu sách còn bản sao đang được mượn (`Borrowed`) |
| `AspNetUsers` | `BorrowRecords` | 1 - N | `Cascade Delete` | Xóa tài khoản xóa luôn toàn bộ phiếu mượn |
| `BorrowRecords` | `BorrowDetails` | 1 - N | `Cascade Delete` | Xóa phiếu mượn xóa toàn bộ các dòng chi tiết mượn |
| `BookCopies` | `BorrowDetails` | 1 - N | `SetNull` | `BookCopyService.DeleteAsync`: Chặn xóa nếu bản sao đã từng phát sinh lịch sử mượn |

---

## 4. Danh Sách Chỉ Mục Tối Ưu Truy Vấn (Indexes)

| Tên Chỉ Mục (Index) | Bảng áp dụng | Cột đánh chỉ mục | Loại Index | Mục đích tối ưu |
| :--- | :--- | :--- | :--- | :--- |
| `UserNameIndex` | `AspNetUsers` | `NormalizedUserName` | `UNIQUE` | Tra cứu nhanh người dùng khi đăng nhập. |
| `IX_AspNetUsers_PhoneNumber` | `AspNetUsers` | `PhoneNumber` | `UNIQUE` | Kiểm tra tính duy nhất và tra cứu số điện thoại. |
| `RoleNameIndex` | `AspNetRoles` | `NormalizedName` | `UNIQUE` | Định danh vai trò hệ thống. |
| `IX_Categories_Name` | `Categories` | `Name` | `UNIQUE` | Chặn trùng tên thể loại và tra cứu nhanh. |
| `IX_Books_ISBN` | `Books` | `ISBN` | `UNIQUE` | Tra cứu nhanh đầu sách qua mã ISBN. |
| `IX_Books_CategoryId` | `Books` | `CategoryId` | Non-Unique | Tối ưu lọc danh sách sách theo thể loại. |
| `IX_BookCopies_Barcode` | `BookCopies` | `Barcode` | `UNIQUE` | Tối ưu quét mã vạch Barcode mượn trả tại quầy. |
| `IX_BookCopies_BookId` | `BookCopies` | `BookId` | Non-Unique | Nạp danh sách các bản sao của một cuốn sách. |
| `IX_BorrowRecords_UserId` | `BorrowRecords` | `UserId` | Non-Unique | Tối ưu tra cứu lịch sử mượn của một độc giả. |
| `IX_BorrowDetails_BorrowRecordId` | `BorrowDetails` | `BorrowRecordId` | Non-Unique | Nạp toàn bộ sách thuộc về một phiếu mượn. |
| `IX_BorrowDetails_BookCopyId` | `BorrowDetails` | `BookCopyId` | Non-Unique | Tra cứu lịch sử mượn của một bản sao vật lý. |

---

## 5. Danh Sách Enum và Giá Trị Số Nguyên Lưu Trong CSDL

### 5.1. `BookCopyStatus` (Trạng thái bản sao vật lý)
File: `back_end/light_brary/code/Enums/BookCopyStatus.cs`

```csharp
public enum BookCopyStatus
{
    Available = 0, // Sẵn sàng trên kệ để mượn
    Borrowed = 1,  // Đang được mượn bởi độc giả
    Lost = 2       // Bị mất hoặc thất lạc
}
```

### 5.2. `BorrowRecordStatus` (Trạng thái phiếu mượn tổng)
File: `back_end/light_brary/code/Enums/BorrowRecordStatus.cs`

```csharp
public enum BorrowRecordStatus
{
    Borrowing = 0, // Phiếu đang hoạt động, còn ít nhất một cuốn chưa hoàn tất
    Closed = 1     // Phiếu đã kết thúc (tất cả các cuốn đều đã Trả hoặc Báo mất)
}
```

### 5.3. `BorrowDetailStatus` (Trạng thái từng cuốn sách trong phiếu)
File: `back_end/light_brary/code/Enums/BorrowDetailStatus.cs`

```csharp
public enum BorrowDetailStatus
{
    Borrowing = 0, // Đang được mượn
    Returned = 1,  // Đã trả sách về thư viện
    Lost = 2,      // Độc giả làm mất sách
    Damaged = 3,   // Sách bị hư hỏng khi trả
    Over = 4       // Quá hạn mượn
}
```
