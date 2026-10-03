using Library.Application.Interfaces.Authentication;

namespace Library.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    // Hash giả dùng để cân bằng thời gian khi user không tồn tại (chống dò tài khoản qua thời gian phản hồi).
    private static readonly Lazy<string> DummyHash =
        new(() => BCrypt.Net.BCrypt.HashPassword("timing-equalizer-not-a-real-password", WorkFactor));

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool Verify(string password, string? hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            BCrypt.Net.BCrypt.Verify(password, DummyHash.Value);
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
