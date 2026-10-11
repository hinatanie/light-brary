using System.Net;
using System.Text.Json;
using light_brary.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra ngoại lệ: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, code, message) = exception switch
            {
                NotFoundException notFound     => (HttpStatusCode.NotFound, "NOT_FOUND", notFound.Message),
                ConflictException conflict     => (HttpStatusCode.Conflict, "CONFLICT", conflict.Message),
                BusinessException biz          => (HttpStatusCode.BadRequest, "BAD_REQUEST", biz.Message),
                UnauthorizedException unauth   => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", unauth.Message),
                ForbiddenException forbidden   => (HttpStatusCode.Forbidden, "FORBIDDEN", forbidden.Message),
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

            var json = JsonSerializer.Serialize(responseObj, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return context.Response.WriteAsync(json);
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

