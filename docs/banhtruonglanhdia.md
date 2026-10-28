
* Chú thuật sư cấp 4

Trọng tâm là middleware pipeline, dependency injection và cách endpoint được tổ chức trong ASP.NET Core.(tự tìm hiểu)

- middleware như 1 trạm kiểm tra trước khi request đi vào controller

vd trongdự án :

app.UseExceptionHandlingMiddleware();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<ValidateUserMiddleware>();
app.UseAuthorization();

các lớp middleware tự đọc cho hiểu ý nghia, còn đảo thứ tự thì sao ? tùy trường hợp, nếu đảo authorize lên trước authen thì nát,
còn cors và redirection thì vô tư.

lưu ý : lỗi ở service khi throw ra sẽ truyền ngược lên và ExceptionHandlingMiddleware cũng sẽ bắt được, có thể coi nó như 1 khung bao phủ bên ngoài.
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

Scoped = mỗi HTTP request có một instance riêng.
Singleton = cả ứng dụng dùng chung đúng một instance.
Transient = lần nào xin → tạo lần đó

mình đang dùng full Scoped vì đa số phải dính với dbcontext, mà èf mặc định cho dbcontext là scope rồi.


* Chú thuật sư cấp 3 (1A)
