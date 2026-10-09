# Cẩm Nang Lý Thuyết Cơ Bản Về C# (.NET)

Tài liệu này tổng hợp các kiến thức cốt lõi, cú pháp cơ bản và các quy ước quan trọng trong ngôn ngữ lập trình C# (đặc biệt áp dụng cho các dự án .NET Backend và Entity Framework Core).

---

## Mục lục
1. [Cấu trúc chương trình & Không gian tên](#1-cấu-trúc-chương-trình--không-gian-tên)
2. [Kiểu dữ liệu & Biến](#2-kiểu-dữ-liệu--biến)
3. [Cơ chế xử lý Null (Nullable Types)](#3-cơ-chế-xử-lý-null-nullable-types)
4. [Phạm vi truy cập (Access Modifiers)](#4-phạm-vi-truy-cập-access-modifiers)
5. [Thuộc tính (Properties)](#5-thuộc-tính-properties)
6. [Lập trình hướng đối tượng (OOP)](#6-lập-trình-hướng-đối-tượng-oop)
7. [Tập hợp & Generics (Collections)](#7-tập-hợp--generics-collections)
8. [LINQ (Language Integrated Query)](#8-linq-language-integrated-query)
9. [Lập trình bất đồng bộ (Async / Await)](#9-lập-trình-bất-đồng-bộ-async--await)
10. [Các cú pháp hiện đại thường gặp](#10-các-cú-pháp-hiện-đại-thường-gặp)
11. [Data Annotations trong Entity Framework Core](#11-data-annotations-trong-entity-framework-core)

---

## 1. Cấu trúc chương trình & Không gian tên

### 1.1 `using` Directives
- **Khái niệm**: Dùng để nhập (import) các thư viện hoặc không gian tên (`namespace`) vào file mã nguồn.
- **Mục đích**: Tránh việc phải gõ đầy đủ tên không gian tên mỗi khi gọi một lớp.
```csharp
// Thay vì viết System.Console.WriteLine("Hello");
using System;

Console.WriteLine("Hello");
```

### 1.2 `namespace` (Không gian tên)
- **Khái niệm**: Dùng để gom nhóm các lớp (`class`), giao diện (`interface`), `struct`, `enum` có liên quan lại với nhau.
- **Mục đích**: Tổ chức mã nguồn khoa học, tránh xung đột tên gọi giữa các thư viện/module khác nhau.
```csharp
namespace light_brary.Domain.Entities
{
    public class Book 
    {
        // Code ở đây
    }
}
```
*Lưu ý (C# 10+)*: Hỗ trợ cú pháp File-scoped namespace giúp code gọn hơn:
```csharp
namespace light_brary.Domain.Entities;

public class Book { }
```

---

## 2. Kiểu dữ liệu & Biến

Trong C#, kiểu dữ liệu được chia làm 2 nhóm chính:

| Tiêu chí | Value Type (Kiểu giá trị) | Reference Type (Kiểu tham chiếu) |
| :--- | :--- | :--- |
| **Vùng nhớ** | Lưu trực tiếp trên **Stack** | Lưu địa chỉ con trỏ trên Stack, dữ liệu thực nằm trên **Heap** |
| **Ví dụ** | `int`, `float`, `double`, `bool`, `char`, `struct`, `enum` | `string`, `class`, `interface`, mảng (`array`), `delegate` |
| **Gán trị** | Sao chép giá trị độc lập | Sao chép tham chiếu (cùng trỏ vào một vùng nhớ) |
| **Giá trị mặc định** | `0`, `false`, giá trị rỗng | `null` |

### Các kiểu dữ liệu phổ biến:
- Số nguyên: `int` (32-bit), `long` (64-bit), `short` (16-bit), `byte` (8-bit).
- Số thực: `double` (thông dụng), `decimal` (độ chính xác cao, dùng trong tài chính/tiền tệ).
- Logic: `bool` (`true` / `false`).
- Chuỗi & Ký tự: `char` ('A'), `string` ("Hello").
- Định danh duy nhất: `Guid` (Globally Unique Identifier - 128 bit UUID).
- Thời gian: `DateTime` (chứa ngày/giờ), `DateTimeOffset` (ngày/giờ có kèm múi giờ).

### Từ khóa `var` (Type Inference)
- Trình biên dịch sẽ tự suy luận kiểu dữ liệu dựa trên giá trị gán ban đầu.
- Kiểu dữ liệu vẫn là tĩnh (Static typing), không phải kiểu động (dynamic).
```csharp
var count = 10;           // Trình biên dịch tự hiểu count là int
var name = "light-brary"; // Trình biên dịch tự hiểu name là string
```

---

## 3. Cơ chế xử lý Null (Nullable Types)

Từ C# 8.0, tính năng **Nullable Reference Types** được giới thiệu để hạn chế lỗi `NullReferenceException` (tỷ phú Tony Hoare gọi lỗi null là "The Billion Dollar Mistake").

### 3.1 Nullable Value Types (`T?`)
Các kiểu giá trị mặc định không thể nhận `null`. Để cho phép `null`, thêm dấu `?`:
```csharp
int? publishYear = null; // Hợp lệ
int age = null;          // Lỗi biên dịch
```

### 3.2 Nullable Reference Types (`string?`)
- `string title`: Bắt buộc phải có giá trị, không được mang giá trị `null`.
- `string? imageUrl`: Cho phép giá trị là `null`.

### 3.3 Các toán tử liên quan đến Null:
- **Null-forgiving operator (`!`)**: Báo cho compiler bỏ qua cảnh báo null:
  ```csharp
  public string Title { get; set; } = null!; // EF Core hoặc code sẽ gán sau
  ```
- **Null-conditional operator (`?.`)**: Chỉ truy cập thuộc tính nếu đối tượng khác null:
  ```csharp
  var length = book?.Title?.Length; // Nếu book null, trả về null thay vì crash
  ```
- **Null-coalescing operator (`??`)**: Cung cấp giá trị mặc định nếu biểu thức là null:
  ```csharp
  string displayImage = book.ImageUrl ?? "default-cover.png";
  ```
- **Null-coalescing assignment (`??=`)**: Gán nếu giá trị hiện tại đang null:
  ```csharp
  categoryName ??= "Chưa phân loại";
  ```

---

## 4. Phạm vi truy cập (Access Modifiers)

Quy định phạm vi các thành phần (class, method, property) có thể được nhìn thấy và sử dụng:

| Từ khóa | Phạm vi truy cập |
| :--- | :--- |
| `public` | Truy cập tự do từ bất cứ đâu. |
| `private` | Chỉ có thể truy cập bên trong chính class định nghĩa nó. |
| `protected` | Chỉ truy cập được bên trong class đó và các class kế thừa (con) từ nó. |
| `internal` | Chỉ truy cập được trong cùng một Project / Assembly (`.dll`). |
| `protected internal` | Truy cập được trong cùng assembly HOẶC từ class kế thừa ở assembly khác. |

---

## 5. Thuộc tính (Properties)

Trong C#, thuộc tính (`Property`) kết hợp giữa trường dữ liệu (field) và phương thức (getter/setter) để đảm bảo tính đóng gói (encapsulation).

### 5.1 Auto-implemented Property
```csharp
public class Book
{
    // Cung cấp cả đọc và ghi công khai
    public string Title { get; set; }

    // Chỉ đọc từ bên ngoài, chỉ ghi từ bên trong class
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // init: Chỉ gán được giá trị lúc khởi tạo đối tượng (C# 9+)
    public Guid Id { get; init; } = Guid.NewGuid();
}
```

### 5.2 Property với logic xử lý (Backing Field)
```csharp
private int _age;
public int Age
{
    get => _age;
    set
    {
        if (value < 0) throw new ArgumentException("Tuổi không được âm");
        _age = value;
    }
}
```

---

## 6. Lập trình hướng đối tượng (OOP)

C# là ngôn ngữ thuần OOP, xoay quanh 4 tính chất cơ bản:

### 6.1 Tính đóng gói (Encapsulation)
- Che giấu dữ liệu nội bộ thông qua các `private field` và chỉ mở ra giao tiếp thông qua `public properties/methods`.

### 6.2 Tính kế thừa (Inheritance)
- Sử dụng dấu hai chấm `:` để kế thừa. C# chỉ hỗ trợ **đơn kế thừa** cho class, nhưng cho phép kế thừa nhiều **interface**.
```csharp
public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Book : BaseEntity
{
    public string Title { get; set; } = null!;
}
```

### 6.3 Tính đa hình (Polymorphism)
- Cho phép các lớp con định nghĩa lại hành vi của lớp cha thông qua từ khóa `virtual` (lớp cha) và `override` (lớp con).
```csharp
public class Notification
{
    public virtual void Send() => Console.WriteLine("Gửi thông báo chung");
}

public class EmailNotification : Notification
{
    public override void Send() => Console.WriteLine("Gửi qua Email");
}
```

### 6.4 Tính trừu tượng (Abstraction)
- Thể hiện qua `abstract class` và `interface`:
  - **`interface`**: Bản hợp đồng định nghĩa các method/property mà class thực thi bắt buộc phải có.
  - **`abstract class`**: Lớp trừu tượng, không thể tạo đối tượng trực tiếp qua `new`, có thể chứa cả phương thức có thân hàm và phương thức trừu tượng (`abstract method`).
```csharp
public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id);
    Task AddAsync(Book book);
}
```

---

## 7. Tập hợp & Generics (Collections)

### 7.1 Generics (`<T>`)
Cho phép viết code linh hoạt với nhiều kiểu dữ liệu mà vẫn đảm bảo tính an toàn kiểu (type-safety) tại thời điểm biên dịch, không tốn chi phí boxing/unboxing.

### 7.2 Các Collection phổ biến trong `System.Collections.Generic`:
- `List<T>`: Danh sách mảng động, truy cập phần tử theo chỉ mục `[index]`, tự động tăng kích thước.
- `Dictionary<TKey, TValue>`: Bảng băm lưu trữ theo cặp Khóa - Giá trị (`Key-Value`), tra cứu cực nhanh với độ phức tạp $O(1)$.
- `HashSet<T>`: Tập hợp các phần tử không trùng lặp.

### 7.3 Các Interface tập hợp thường gặp:
- `IEnumerable<T>`: Tập hợp chỉ đọc, hỗ trợ duyệt qua bằng vòng lặp `foreach`. Phù hợp cho việc query dữ liệu chưa thực thi.
- `ICollection<T>`: Kế thừa `IEnumerable<T>`, bổ sung thêm các phương thức chỉnh sửa danh sách (`Add`, `Remove`, `Count`).
- `IList<T>`: Kế thừa `ICollection<T>`, hỗ trợ truy cập theo chỉ mục (`Index`).
- `IQueryable<T>`: Dùng trong EF Core, cho phép dịch các câu lệnh LINQ thành câu lệnh SQL để truy vấn trực tiếp trên Database server.

---

## 8. LINQ (Language Integrated Query)

LINQ cung cấp cú pháp đồng nhất để truy vấn dữ liệu từ nhiều nguồn (List, Database, XML,...).

Ví dụ sử dụng Method Syntax:
```csharp
var books = new List<Book>();

// 1. Where: Lọc dữ liệu
var modernBooks = books.Where(b => b.PublishYear > 2000);

// 2. Select: Chiếu/chuyển đổi dữ liệu (Projection)
var bookTitles = books.Select(b => b.Title);

// 3. FirstOrDefault: Lấy phần tử đầu tiên thỏa điều kiện hoặc giá trị mặc định
var firstBook = books.FirstOrDefault(b => b.ISBN == "123456");

// 4. Any / All: Kiểm tra tồn tại
bool hasBooks = books.Any();
bool allValid = books.All(b => b.PublishYear > 0);

// 5. OrderBy: Sắp xếp
var sortedBooks = books.OrderBy(b => b.Title).ThenByDescending(b => b.PublishYear);

// 6. ToList(): Thực thi truy vấn và chuyển thành List
var resultList = books.Where(b => b.PublishYear == 2024).ToList();
```

---

## 9. Lập trình bất đồng bộ (Async / Await)

Giải phóng luồng (thread) trong lúc chờ các tác vụ I/O nặng (đọc ghi file, gọi mạng, truy vấn CSDL), giúp ứng dụng backend xử lý được nhiều request đồng thời hơn.

```csharp
public async Task<Book?> GetBookAsync(Guid id)
{
    // Luồng không bị block khi chờ CSDL phản hồi
    var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
    return book;
}
```
* **`async`**: Đánh dấu phương thức có chứa xử lý bất đồng bộ.
* **`await`**: Tạm dừng thực thi phương thức hiện tại cho đến khi tác vụ hoàn thành, trả lại thread cho hệ thống.
* **`Task` / `Task<T>`**: Đại diện cho một tác vụ đang chạy (không có giá trị trả về hoặc trả về giá trị kiểu `T`).

---

## 10. Các cú pháp hiện đại thường gặp

### 10.1 Toán tử `nameof`
Lấy tên của biến, kiểu hoặc thành viên dưới dạng chuỗi `string`, an toàn khi refactor mã nguồn:
```csharp
nameof(Book.Title) // Trả về "Title"
```

### 10.2 Chuỗi nội suy (String Interpolation)
Sử dụng ký tự `$` để chèn biến/biểu thức trực tiếp vào chuỗi:
```csharp
string author = "Nam Cao";
string message = $"Tác giả: {author}, thời gian: {DateTime.Now:yyyy-MM-dd}";
```

### 10.3 Biểu thức Lambda (Lambda Expressions)
Cú pháp viết hàm ẩn danh ngắn gọn theo dạng `input => expression`:
```csharp
Func<int, int> square = x => x * x;
```

### 10.4 Pattern Matching
Cú pháp kiểm tra kiểu và giá trị linh hoạt:
```csharp
if (obj is Book b && b.PublishYear > 2020)
{
    Console.WriteLine(b.Title);
}
```

---

## 11. Data Annotations trong Entity Framework Core

Được sử dụng trực tiếp trên các Entity để định hình bảng và các cột trong Database:

| Annotation | Ý nghĩa & Tác dụng |
| :--- | :--- |
| `[Table("TenBang")]` | Chỉ định tên bảng trong cơ sở dữ liệu. |
| `[Key]` | Đánh dấu thuộc tính là Khóa chính (Primary Key). |
| `[Required]` | Bắt buộc trường này không được `NULL` (`NOT NULL`). |
| `[StringLength(max)]` | Giới hạn số ký tự tối đa của cột chuỗi (`nvarchar(max)`). |
| `[Column("TenCot")]` | Đổi tên cột trong CSDL khác với tên property trong C#. |
| `[ForeignKey("TenId")]` | Chỉ định thuộc tính là Khóa ngoại (Foreign Key). |
| `[InverseProperty("...")]` | Xác định thuộc tính quan hệ đối ứng trong mối quan hệ 2 chiều. |
| `[Index(nameof(...))]` | Tạo chỉ mục (Index) trên một hoặc nhiều cột để tối ưu tìm kiếm. |
| `[NotMapped]` | Không tạo cột này trong CSDL (chỉ dùng nội bộ trong code C#). |

