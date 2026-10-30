# Xem và thử API bằng Swagger

Swagger hiển thị các endpoint, tham số và mô hình request/response của phiên bản backend đang chạy.

1. Khởi động backend ở môi trường **Development** theo một trong các cách trong [README](../README.md).
2. Mở [http://localhost:5218/swagger](http://localhost:5218/swagger). Nếu đổi cổng API, thay `5218` bằng cổng tương ứng.
3. Chọn một endpoint để xem chi tiết. Với endpoint công khai, chọn **Try it out**, nhập tham số hoặc request body nếu cần, rồi chọn **Execute** để xem response.

Swagger chỉ được bật trong Development. Schema OpenAPI dạng JSON có tại `http://localhost:5218/swagger/v1/swagger.json`.
