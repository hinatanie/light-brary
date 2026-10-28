# Ghi chú backend LightBrary

## Chú thuật sư cấp 3

Trọng tâm là middleware pipeline, dependency injection và cách endpoint được tổ chức trong ASP.NET Core (tự tìm hiểu).

### Middleware pipeline

Middleware giống như một trạm kiểm tra trong quá trình xử lý request, trước khi request đi vào controller. Ví dụ trong dự án:

```csharp
app.UseExceptionHandlingMiddleware();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<ValidateUserMiddleware>();
app.UseAuthorization();
```

Tự đọc các lớp middleware để hiểu ý nghĩa. Thứ tự tùy trường hợp: authentication phải trước authorization. CORS và HTTPS redirection cũng có thể ảnh hưởng đến preflight `OPTIONS` và chuyển hướng; frontend nên gọi trực tiếp URL HTTPS của API.

**Lưu ý về xử lý lỗi:** Lỗi ở service khi throw ra sẽ truyền ngược lên; nếu chưa được bắt và xử lý ở bên trong, `ExceptionHandlingMiddleware` sẽ bắt được. Có thể coi nó như một khung bao phủ bên ngoài các middleware phía sau, nhờ đoạn `try/catch` quanh `await next(context)`:

```csharp
try
{
    await next(context);
}
catch (Exception ex)
{
    logger.LogError(ex, "Đã xảy ra ngoại lệ: {Message}", ex.Message);
    await HandleExceptionAsync(context, ex);
}
```

### Dependency injection (DI)

Có thể hiểu đơn giản: class cần gì thì ASP.NET Core tạo và đưa dependency đó vào, thay vì class tự `new`.

- **Scoped:** Một instance trong mỗi scope; thông thường mỗi HTTP request có một scope riêng.
- **Singleton:** Một instance dùng chung trong service provider của ứng dụng.
- **Transient:** Mỗi lần yêu cầu dependency thì tạo một instance mới.

Các service và repository được đăng ký thủ công trong `Program.cs` dùng Scoped; `AddDbContext` cũng đăng ký DbContext là Scoped theo mặc định.

## Chú thuật sư cấp 2

### Luồng thành công

Service trả kết quả lên controller; controller chuyển kết quả thành HTTP response. Các hàm `Ok()` / `Ok(data)`, `Created()`, `CreatedAtAction()` và `NoContent()` các thí chủ tự tìm hiểu.

### Luồng thất bại

Service throw `AppException`, middleware bắt lỗi và chuyển thành HTTP response. Phần này tự xem code.

## Chú thuật sư cấp 1

### EF Core và Identity

Tự tìm hiểu EF Core và migration.

ASP.NET Core Identity có các bảng chính như user, userrole và role; ngoài ra còn có bảng claim, login và token. User trong dự án kế thừa `IdentityUser<Guid>` và bổ sung các trường cần thiết.

### Migration

Migration là các file mô tả việc thay đổi cấu trúc database. Ví dụ luồng làm việc khi cần thêm một cột (tự tìm hiểu lệnh nhé):

1. Thêm thuộc tính vào entity hoặc thay đổi cấu hình model.
2. Tạo migration để EF Core ghi lại thay đổi cấu trúc.
3. Áp dụng migration vào database.

### Truy vấn

Nên đọc các file repository để hiểu LINQ. `Include` tải dữ liệu liên quan và có thể tránh N+1, nhưng không tự đảm bảo truy vấn tối ưu; cần xem SQL, số truy vấn và lượng dữ liệu thực tế.

## Đặc cấp

### N+1 query

Tự tìm hiểu N+1 query.

### Redis cache

Dự án dùng Redis distributed cache: dữ liệu cache được lưu trong Redis bên ngoài tiến trình API. Nếu deploy hai server API cùng kết nối đến Redis đó thì chúng có thể dùng chung cache.

**Luồng GET chi tiết sách theo ID** (cache-aside):

1. Tìm dữ liệu trong Redis với key, ví dụ `books:{id}`.
2. Nếu có, trả dữ liệu từ cache.
3. Nếu không có, đọc từ database, lưu kết quả vào Redis trong 10 phút rồi trả về.

Tự xem các trường hợp sửa dữ liệu và xóa cache cũ (cache invalidation).
