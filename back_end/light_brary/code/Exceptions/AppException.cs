using System.Net;

namespace light_brary.Exceptions
{
    public class AppException(HttpStatusCode statusCode, string code, string message) : Exception(message)
    {
        public HttpStatusCode StatusCode { get; } = statusCode;
        public string Code { get; } = code;

        public static AppException BadRequest(string message = "Dữ liệu đầu vào không hợp lệ")
            => new(HttpStatusCode.BadRequest, "BAD_REQUEST", message);

        public static AppException Unauthorized(string message = "Thông tin đăng nhập không chính xác")
            => new(HttpStatusCode.Unauthorized, "UNAUTHORIZED", message);

        public static AppException Forbidden(string message = "Không có quyền truy cập chức năng này")
            => new(HttpStatusCode.Forbidden, "FORBIDDEN", message);

        public static AppException NotFound(string message = "Không tìm thấy dữ liệu")
            => new(HttpStatusCode.NotFound, "NOT_FOUND", message);

        public static AppException Conflict(string message = "Dữ liệu bị xung đột")
            => new(HttpStatusCode.Conflict, "CONFLICT", message);
    }
}
