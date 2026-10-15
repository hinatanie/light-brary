## 1. Bảng tổng hợp UNIQUE

| Bảng | Cột áp dụng | Loại ràng buộc | Ý nghĩa |
|---|---|---|---|
| `AspNetUsers` | `PhoneNumber` | `UNIQUE` | Không cho phép hai tài khoản sử dụng cùng số điện thoại. |
| `AspNetRoles` | `Name` | `UNIQUE` | Không cho phép trùng tên vai trò; tên phải được chuẩn hóa nhất quán. |
| `AspNetUserRoles` | `(UserId, RoleId)` | Composite PK, đã bảo đảm duy nhất | Không gán cùng một vai trò cho cùng một người dùng nhiều lần. Không cần thêm `UNIQUE` riêng. |
| `BookCopies` | `Barcode` | `UNIQUE` | Mỗi bản sao sách có một mã vạch duy nhất trên toàn hệ thống. |
| `BorrowDetails` | `(BorrowRecordId, BookCopyId)` | Composite `UNIQUE` | Một bản sao sách chỉ xuất hiện một lần trong cùng một phiếu mượn. |
| `BorrowDetails` | `BookCopyId`, với điều kiện `Status = 0` | Partial unique index | Một bản sao sách chỉ có tối đa một lượt mượn đang ở trạng thái `Borrowing`. |
| `Books` | `ISBN` | `UNIQUE`, nếu mỗi dòng là một ấn bản cụ thể | Không tạo hai đầu sách cho cùng ISBN đã chuẩn hóa. |

## 2. Bảng tổng hợp NOT NULL

| Bảng | Các cột bắt buộc `NOT NULL` | Các cột cho phép `NULL` |
|---|---|---|
| `AspNetUsers` | `Id`, `FullName`, `PasswordHash`, `CreatedAt`, `PhoneNumber` | `Address` |
| `AspNetRoles` | `Id`, `Name` | Không có |
| `AspNetUserRoles` | `UserId`, `RoleId` | Không có |
| `Categories` | `Id`, `Name` | `Description` |
| `Books` | `Id`, `Title`, `Author`, `CategoryId`, `CreatedAt`, `ISBN` | `PublishYear` |
| `BookCopies` | `Id`, `BookId`, `Barcode`, `Status` | `ConditionNote` |
| `BorrowRecords` | `Id`, `UserId`, `BorrowDate`, `DueDate`, `Status` | `Note` |
| `BorrowDetails` | `Id`, `BorrowRecordId`, `BookCopyId`, `Status` | `ReturnDate` |

## 3. Bảng tổng hợp CHECK

Bảng dưới đây sử dụng các giá trị enum hiện tại trong tài liệu.

| Bảng | Ràng buộc CHECK | Ý nghĩa |
|---|---|---|
| `BookCopies` | `Status IN (0, 1, 2)` | Chỉ nhận `Available`, `Borrowed`, `Lost`. |
| `BorrowRecords` | `Status IN (0, 1, 2)` | Chỉ nhận `Borrowing`, `Returned`, `Lost` |
| `BorrowDetails` | `Status IN (0, 1, 2)` | Chỉ nhận `Borrowing`, `Returned`, `Lost`. |
| `BorrowRecords` | `DueDate > BorrowDate` | Hạn trả phải sau thời điểm mượn. |
| `BorrowDetails` | `(Status = 1 AND ReturnDate IS NOT NULL) OR (Status IN (0, 2) AND ReturnDate IS NULL)` | Đã trả phải có ngày trả; đang mượn hoặc mất sách không có ngày trả. |
| `Books` | `PublishYear IS NULL OR PublishYear > 0` | Năm xuất bản có thể chưa biết; nếu có thì phải là số dương. |

`ReturnDate >= BorrowDate` và sự nhất quán giữa trạng thái bản sao với chi tiết mượn cần được kiểm soát bằng nghiệp vụ và transaction vì liên quan đến nhiều bảng.



## V. Bảng Tổng Hợp Quan Hệ Giữa Các Bảng (Relationship Matrix)

| Bảng nguồn | Bảng đích | Loại quan hệ | Ý nghĩa nghiệp vụ | Hành vi khi xóa (On Delete) |
| :--- | :--- | :---: | :--- | :--- |
| `AspNetUsers` | `AspNetUserRoles` | 1 - N | Một người dùng có nhiều vai trò | `Cascade Delete` |
| `AspNetRoles` | `AspNetUserRoles` | 1 - N | Một vai trò được gán cho nhiều người | `Restrict Delete` |
| `Categories` | `Books` | 1 - N | Một thể loại chứa nhiều đầu sách | `Restrict` (Chặn xóa nếu còn sách) |
| `Books` | `BookCopies` | 1 - N | Một đầu sách có nhiều bản sao vật lý trên kệ | `Restrict Delete` |
| `AspNetUsers` | `BorrowRecords` | 1 - N | Một người dùng tạo nhiều phiếu mượn | `Cascade Delete` |
| `BorrowRecords` | `BorrowDetails` | 1 - N | Một phiếu mượn chứa nhiều cuốn sách | `Cascade Delete` |
| `BorrowRecords` | `Fines` | 1 - 1 | Một phiếu mượn có thể có một phiếu phạt | `Cascade Delete` |
| `Fines` | `FineDetails` | 1 - N | Một phiếu phạt có nhiều chi tiết phạt | `Cascade Delete` |
| `BorrowDetails` | `FineDetails` | 1 - 1 | Một lượt mượn sách có thể có một chi tiết phạt | `Cascade Delete` |
| `BookCopies` | `BorrowDetails` | 1 - N | Một bản sao sách xuất hiện trong nhiều lượt mượn | `Restrict` (Bảo toàn lịch sử) |

---

## VI. Danh Sách Chỉ Mục Tối Ưu Truy Vấn (Indexes)

| Tên Chỉ Mục (Index) | Bảng áp dụng | Cột đánh chỉ mục | Loại Index | Mục đích tối ưu |
| :--- | :--- | :--- | :--- | :--- |
| `IX_BookCopies_Barcode` | `BookCopies` | `Barcode` | **UNIQUE** | Đảm bảo mã vạch không trùng lặp; tăng tốc tra cứu khi thủ thư quét mã vạch. |
| `IX_BookCopies_BookId` | `BookCopies` | `BookId` | Non-Unique | Tối ưu truy vấn tìm tất cả các bản sao vật lý của một đầu sách. |
| `IX_Books_CategoryId` | `Books` | `CategoryId` | Non-Unique | Tối ưu tìm kiếm và lọc danh sách sách theo thể loại. |
| `IX_BorrowRecords_UserId` | `BorrowRecords` | `UserId` | Non-Unique | Tối ưu truy vấn lịch sử mượn trả của một độc giả. |
| `IX_BorrowDetails_BorrowRecordId` | `BorrowDetails` | `BorrowRecordId` | Non-Unique | Tối ưu lấy toàn bộ danh sách các cuốn sách nằm trong một phiếu mượn. |
| `IX_BorrowDetails_BookCopyId` | `BorrowDetails` | `BookCopyId` | Non-Unique | Tối ưu tra cứu lịch sử mượn của một cuốn sách vật lý cụ thể. |
| `IX_AspNetUserRoles_RoleId` | `AspNetUserRoles` | `RoleId` | Non-Unique | Tối ưu tìm kiếm danh sách người dùng thuộc một vai trò nhất định. |


## Enum của các Property

### `BookCopyStatus`
```csharp
public enum BookCopyStatus : short
{
    Available = 0,
    Borrowed = 1,
    Lost = 2
}
```
Ý nghĩa:
```text
0 = Available
1 = Borrowed
2 = Lost
```

---
### `BorrowRecordStatus`
Enum:
```csharp
public enum BorrowRecordStatus : short
{
    Borrowing = 0,
    Returned = 1,
    Lost = 2
}
```
Ý nghĩa:
```text
0 = Borrowing
1 = Returned
2 = Lost
```

---
### `BorrowDetailStatus`
Enum:
```csharp
public enum BorrowDetailStatus : short
{
    Borrowing = 0,
    Returned = 1,
    Lost = 2
}
```
Ý nghĩa:
```text
0 = Borrowing
1 = Returned
2 = Lost
```
