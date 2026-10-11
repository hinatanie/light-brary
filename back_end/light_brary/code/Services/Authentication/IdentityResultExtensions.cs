using light_brary.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Services.Authentication
{
    internal static class IdentityResultExtensions
    {
        public static void EnsureSucceeded(this IdentityResult result)
        {
            if (result.Succeeded) return;

            if (result.Errors.Any(error => error.Code == "PasswordMismatch"))
                throw new UnauthorizedException("Mật khẩu hiện tại không chính xác.");

            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            if (result.Errors.Any(error => error.Code is "DuplicateUserName" or "DuplicateRoleName" or "DuplicateEmail" or "ConcurrencyFailure"))
                throw new ConflictException(message);

            throw new BusinessException(message);
        }
    }
}
