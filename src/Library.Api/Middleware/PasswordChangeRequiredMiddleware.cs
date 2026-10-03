using System.Net;
using System.Text.Json;
using Library.Application.Common;

namespace Library.Api.Middleware;

public class PasswordChangeRequiredMiddleware
{
    private readonly RequestDelegate _next;

    public PasswordChangeRequiredMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true && user.HasClaim(AuthConstants.PasswordChangeRequiredClaim, "true"))
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Các endpoint được phép khi user phải đổi mật khẩu:
            var isAllowed = path.EndsWith("/auth/me")
                            || path.EndsWith("/auth/change-password")
                            || path.EndsWith("/auth/logout")
                            || path.StartsWith("/swagger")
                            || path.StartsWith("/openapi");

            if (!isAllowed)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    status = (int)HttpStatusCode.Forbidden,
                    code = "PASSWORD_CHANGE_REQUIRED",
                    message = "Tài khoản của bạn bắt buộc phải đổi mật khẩu lần đầu trước khi thực hiện các thao tác khác."
                };

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
                return;
            }
        }

        await _next(context);
    }
}
