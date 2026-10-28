using System.Net;
using light_brary.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Middleware
{
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Đã xảy ra ngoại lệ: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, code, message) = exception switch
            {
                AppException appEx            => (appEx.StatusCode, appEx.Code, appEx.Message),
                ArgumentException argEx        => (HttpStatusCode.BadRequest, "BAD_REQUEST", argEx.Message),
                DbUpdateException              => (HttpStatusCode.Conflict, "DATABASE_CONFLICT", "Xung đột dữ liệu cơ sở dữ liệu."),
                _                              => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "Đã xảy ra lỗi hệ thống không mong muốn.")
            };

            context.Response.StatusCode = (int)statusCode;

            var responseObj = new
            {
                status = (int)statusCode,
                code,
                message
            };

            return context.Response.WriteAsJsonAsync(responseObj, options: null, contentType: "application/json");
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}

