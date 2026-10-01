using Library.Domain.Entities;

namespace Library.Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    DateTime GetExpiration();
}
