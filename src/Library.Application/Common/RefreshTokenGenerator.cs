using System.Security.Cryptography;
using System.Text;

namespace Library.Application.Common;

public static class RefreshTokenGenerator
{
    /// <summary>Sinh refresh token ngẫu nhiên 512-bit (base64url).</summary>
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>Băm SHA-256 (hex thường, 64 ký tự) để lưu DB; không bao giờ lưu token thô.</summary>
    public static string Hash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
