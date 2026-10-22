using light_brary.Entities;
using light_brary.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Middleware
{
    public class ValidateUserMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context,
            SignInManager<User> signInManager, UserManager<User> userManager)
        {
            var endpoint = context.GetEndpoint();
            if (context.User.Identity?.IsAuthenticated != true ||
                endpoint?.Metadata.GetMetadata<IAuthorizeData>() is null ||
                endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                await next(context);
                return;
            }

            if (!Guid.TryParse(userManager.GetUserId(context.User), out _))
                throw AppException.Unauthorized("Không xác định được danh tính người dùng.");

            var user = await signInManager.ValidateSecurityStampAsync(context.User)
                ?? throw AppException.Unauthorized("Phiên đăng nhập không còn hợp lệ. Vui lòng đăng nhập lại.");

            if (await userManager.IsLockedOutAsync(user))
                throw AppException.Unauthorized("Tài khoản của bạn đã bị khóa, liên hệ quản thư để được hỗ trợ.");

            await next(context);
        }
    }
}
