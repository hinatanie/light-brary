using Microsoft.AspNetCore.Identity;

namespace light_brary.Exceptions
{
    internal static class IdentityResultExtensions
    {
        public static void ThrowIfFailed(this IdentityResult result)
        {
            if (result.Succeeded) return;

            if (result.Errors.Any(error => error.Code == "PasswordMismatch"))
                throw AppException.Unauthorized("Mật khẩu hiện tại không chính xác.");

            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            if (result.Errors.Any(error => error.Code is "DuplicateUserName" or "DuplicateRoleName" or "ConcurrencyFailure"))
                throw AppException.Conflict(message);

            throw AppException.BadRequest(message);
        }
    }
}


// ConcurrencyFailure lỗi xung đột khi nhiều thao tác cùng cập nhật một bản ghi.
// Ví dụ:
// 1. Hai admin cùng mở thông tin của một user.
// 2. Admin A lưu thay đổi trước.
// 3. Admin B lưu dựa trên dữ liệu cũ.
// 4. Identity phát hiện bản ghi đã thay đổi và trả ConcurrencyFailure, tránh ghi đè mất thay đổi của A.