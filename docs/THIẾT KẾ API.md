# Xem và thử API bằng Swagger

Swagger hiển thị các endpoint, tham số và mô hình request/response của phiên bản backend đang chạy.

1. Khởi động backend ở môi trường **Development** theo một trong các cách trong [README](../README.md).
2. Mở [http://localhost:5218/swagger](http://localhost:5218/swagger). Nếu đổi cổng API, thay `5218` bằng cổng tương ứng.
3. Chọn một endpoint để xem chi tiết. Với endpoint công khai, chọn **Try it out**, nhập tham số hoặc request body nếu cần, rồi chọn **Execute** để xem response.

Swagger chỉ được bật trong Development. Schema OpenAPI dạng JSON có tại `http://localhost:5218/swagger/v1/swagger.json`.

Để thử các endpoint yêu cầu đăng nhập:

1. Gọi `POST /api/auth/login` với tên đăng nhập và mật khẩu, rồi sao chép `accessToken` trong response.
2. Bấm **Authorize** ở đầu trang Swagger hoặc biểu tượng khóa tại endpoint.
3. Dán `accessToken` vào ô **Value**, không thêm tiền tố `Bearer`, rồi bấm **Authorize** và **Close**.
4. Chọn **Try it out** và **Execute** tại endpoint cần thử. Swagger tự gửi header `Authorization: Bearer <accessToken>`.

Swagger dùng cấu hình Bearer chung nên mọi endpoint đều hiện biểu tượng khóa; các endpoint công khai vẫn gọi được khi chưa nhập token. Tài khoản vẫn cần quyền phù hợp với endpoint được bảo vệ. Khi token hết hạn, đăng nhập lại và nhập token mới vào **Authorize**.
