using System.Text;
using Library.Application.Exceptions;

namespace Library.Application.Validation;

/// <summary>Chính sách mật khẩu dùng chung cho đăng ký, đổi mật khẩu và khởi tạo admin.</summary>
public static class PasswordPolicy
{
    public const int MinLength = 10;
    public const int MaxLength = 64;
    public const int MaxBytes = 72; // BCrypt chỉ dùng 72 byte đầu

    private static readonly HashSet<string> CommonWeakPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password123!", "Password@123", "Admin12345!", "Admin@12345", "Matkhau123!",
        "1234567890Aa!", "Welcome123!", "IloveYou123!"
    };

    private static readonly string[] WeakPrefixes =
    {
        "password", "matkhau", "123456", "abcdef", "letmein", "iloveyou", "welcome", "admin123"
    };

    public static IReadOnlyList<string> Validate(string? password, string? phoneNumber = null, string? email = null)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Mật khẩu không được để trống.");
            return errors;
        }

        if (password.Length < MinLength) errors.Add($"Mật khẩu phải có ít nhất {MinLength} ký tự.");
        if (password.Length > MaxLength) errors.Add($"Mật khẩu không được vượt quá {MaxLength} ký tự.");
        if (Encoding.UTF8.GetByteCount(password) > MaxBytes) errors.Add($"Mật khẩu quá dài (tối đa {MaxBytes} byte).");
        if (!password.Any(char.IsUpper)) errors.Add("Mật khẩu phải có ít nhất một chữ hoa.");
        if (!password.Any(char.IsLower)) errors.Add("Mật khẩu phải có ít nhất một chữ thường.");
        if (!password.Any(char.IsDigit)) errors.Add("Mật khẩu phải có ít nhất một chữ số.");

        var lower = password.ToLowerInvariant();
        if (CommonWeakPasswords.Contains(password) || WeakPrefixes.Any(p => lower.StartsWith(p)))
            errors.Add("Mật khẩu quá phổ biến hoặc dễ đoán.");

        if (!string.IsNullOrWhiteSpace(phoneNumber) && lower.Contains(phoneNumber.Trim()))
            errors.Add("Mật khẩu không được chứa số điện thoại.");

        var localPart = email?.Split('@')[0].Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(localPart) && localPart.Length >= 4 && lower.Contains(localPart))
            errors.Add("Mật khẩu không được chứa tên email.");

        return errors;
    }

    public static void EnsureValid(string? password, string? phoneNumber = null, string? email = null)
    {
        var errors = Validate(password, phoneNumber, email);
        if (errors.Count > 0)
            throw new BusinessException(string.Join(" ", errors));
    }
}
