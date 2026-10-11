using System.Security.Claims;
using System.Text;
using light_brary.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace light_brary.Services.Authentication
{
    public class JwtTokenService(IConfiguration configuration)
    {
        private readonly string _issuer = configuration.GetSection("Jwt")["Issuer"]
            ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Issuer");
        private readonly string _audience = configuration.GetSection("Jwt")["Audience"]
            ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Audience");
        private readonly string _secretKey = configuration.GetSection("Jwt")["SecretKey"]
            ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:SecretKey");
        private readonly int _expirationMinutes = ParseExpirationMinutes(configuration.GetSection("Jwt")["ExpirationMinutes"]);

        private static int ParseExpirationMinutes(string? value)
            => int.TryParse(value, out var minutes) ? minutes : 60;

        public int ExpiresInSeconds => _expirationMinutes * 60;

        public string GenerateToken(User user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.MobilePhone, user.PhoneNumber!),
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _issuer,
                Audience = _audience,
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            };

            return new JsonWebTokenHandler().CreateToken(descriptor);
        }
    }
}