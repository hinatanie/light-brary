using light_brary.Entities;
using Microsoft.AspNetCore.Identity;

namespace light_brary.Services.Authentication
{
    /// <summary>
    /// Bọc PasswordHasher có sẵn của ASP.NET Core Identity (PBKDF2 + salt ngẫu nhiên),
    /// không cần cài thêm package.
    /// </summary>
    public class PasswordHasher
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string Hash(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        public bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash)) return false;

            try
            {
                var result = _hasher.VerifyHashedPassword(null!, passwordHash, password);
                return result != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                // Chuỗi hash trong DB không đúng định dạng (vd: lưu plain text)
                return false;
            }
        }
    }
}