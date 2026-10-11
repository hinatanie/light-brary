using System.Security.Claims;
using System.Text;
using light_brary.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace light_brary.Services.Authentication
{
    public class JwtTokenService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secretKey;
        private readonly int _expirationMinutes;

        public JwtTokenService(IConfiguration configuration)
        {
            var section = configuration.GetSection("Jwt");
            _issuer = section["Issuer"] ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Issuer");
            _audience = section["Audience"] ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Audience");
            _secretKey = section["SecretKey"] ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:SecretKey");
            _expirationMinutes = int.TryParse(section["ExpirationMinutes"], out var minutes) ? minutes : 60;
        }

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