using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace light_brary.Middleware
{
    /// <summary>
    /// Custom Middleware tự bóc tách và xác thực JWT token từ Header Authorization,
    /// tương đương JwtAuthenticationFilter (OncePerRequestFilter) trong Spring Boot.
    /// </summary>
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenValidationParameters _validationParameters;
        private readonly ILogger<JwtAuthenticationMiddleware> _logger;

        public JwtAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<JwtAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            var jwtSection = configuration.GetSection("Jwt");
            var secretKey = jwtSection["SecretKey"]
                ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:SecretKey trong appsettings.json");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidIssuer = issuer,
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name,
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Trích xuất Authorization header: "Bearer <token>"
            string? authHeader = context.Request.Headers.Authorization;

            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                await AttachUserToContextAsync(context, token);
            }

            // 2. Chuyển quyền xử lý cho middleware tiếp theo trong pipeline
            await _next(context);
        }

        private async Task AttachUserToContextAsync(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JsonWebTokenHandler();
                var result = await tokenHandler.ValidateTokenAsync(token, _validationParameters);

                if (result.IsValid)
                {
                    // Tạo ClaimsIdentity có authenticationType để đánh dấu User.Identity.IsAuthenticated = true
                    var identity = new ClaimsIdentity(
                        result.ClaimsIdentity.Claims,
                        "CustomJwtAuth",
                        ClaimTypes.Name,
                        ClaimTypes.Role);

                    // Gán vào context.User (Tương đương SecurityContextHolder.getContext().setAuthentication(...) trong Spring)
                    context.User = new ClaimsPrincipal(identity);
                }
                else
                {
                    _logger.LogWarning("Token không hợp lệ hoặc đã hết hạn: {Reason}", result.Exception?.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình giải mã token");
            }
        }
    }

    public static class JwtAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuthenticationMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtAuthenticationMiddleware>();
        }
    }
}

