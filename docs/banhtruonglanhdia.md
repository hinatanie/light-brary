
* Chú thuật sư cấp 3

Trọng tâm là middleware pipeline, dependency injection và cách endpoint được tổ chức trong ASP.NET Core.(tự tìm hiểu)

- middleware như 1 trạm kiểm tra trước khi request đi vào controller

vd trongdự án :

app.UseExceptionHandlingMiddleware();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<ValidateUserMiddleware>();
app.UseAuthorization();

các lớp middleware tự đọc cho hiểu ý nghĩa. Thứ tự tùy trường hợp: authentication phải trước authorization.
CORS và HTTPS redirection cũng có thể ảnh hưởng đến preflight OPTIONS và chuyển hướng; frontend nên gọi trực tiếp URL HTTPS của API.

lưu ý: lỗi ở service khi throw ra sẽ truyền ngược lên; nếu chưa được bắt và xử lý ở bên trong, ExceptionHandlingMiddleware sẽ bắt được. Có thể coi nó như một khung bao phủ bên ngoài các middleware phía sau.
        try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Đã xảy ra ngoại lệ: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
do thằng này thôi chứ bản chất vẫn là luồng xử lí request trước khi vào controller


- DI : đơn giản hóa là Class cần cái gì thì ASP.NET Core tự tạo và đưa cái đó vào, thay vì class tự new.

Scoped = một instance trong mỗi scope; thông thường mỗi HTTP request có một scope riêng.
Singleton = một instance dùng chung trong service provider của ứng dụng.
Transient = mỗi lần yêu cầu dependency thì tạo một instance mới.

các service và repository được đăng ký thủ công trong Program.cs dùng Scoped; AddDbContext cũng đăng ký DbContext là Scoped theo mặc định.


* Chú thuật sư cấp 2

 luồng thành công :
 service sẽ trả response lên controller, comtroller sẽ map sang http response
 các hàm : Ok() / Ok(data), Created(), CreatedAtAction(), NoContent() các thí chủ tự tìm hiểu

luồng thất bại :
service throw các AppException , rồi middleware bắt các lỗi đó rồi map. phần này tự coi code

* Chú thuật sư cấp 1

tự tìm hiểu ef, migration

ASP.NET Core Identity có các bảng chính như user, userrole và role; ngoài ra còn có bảng claim, login và token. User trong dự án kế thừa IdentityUser<Guid> và bổ sung các trường cần thiết.

Migration là các file mô tả việc thay đổi cấu trúc database

Ví dụ luồng làm việc khi cần thêm một cột: tự tìm hiểu lệnh nhé
1. Thêm thuộc tính vào entity hoặc thay đổi cấu hình model.
2. Tạo migration để EF Core ghi lại thay đổi cấu trúc.
3. Áp dụng migration vào database.

phần truy vấn nên đọc các file repo để hiểu LINQ. Include tải dữ liệu liên quan và có thể tránh N+1, nhưng không tự đảm bảo truy vấn tối ưu; cần xem SQL, số truy vấn và lượng dữ liệu thực tế.

* đặc cấp

tự tìm hiểu n+1 query

dự án dùng Redis distributed để cache

Distributed Redis cache nghĩa là dữ liệu cache được lưu trong Redis bên ngoài tiến trình API. Nếu deploy 2 server API cùng kết nối đến Redis đó thì chúng có thể dùng chung cache.

luồng cơ bản: GET chi tiết sách theo ID
Tìm dữ liệu trong Redis với key, ví dụ: books:{id}.
Nếu có thì trả dữ liệu từ cache.
Nếu không có thì đọc từ database, lưu kết quả vào Redis trong 10 phút rồi trả về.
(cache-aside)

tự xem các trường hợp sửa (cache invalidation)
