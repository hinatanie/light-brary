using System.Text.RegularExpressions;
using Library.Application.Exceptions;

namespace Library.Application.Validation;

/// <summary>Chuẩn hóa số điện thoại Việt Nam về dạng 0xxxxxxxxx (10 số).</summary>
public static partial class PhoneNumberNormalizer
{
    [GeneratedRegex(@"^0\d{9}$")]
    private static partial Regex CanonicalRegex();

    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = new string(input.Where(c => !char.IsWhiteSpace(c) && c != '.' && c != '-' && c != '(' && c != ')').ToArray());

        if (cleaned.StartsWith("+84")) cleaned = "0" + cleaned[3..];
        else if (cleaned.StartsWith("0084")) cleaned = "0" + cleaned[4..];

        if (!CanonicalRegex().IsMatch(cleaned)) return false;

        normalized = cleaned;
        return true;
    }

    public static string Normalize(string? input)
    {
        if (!TryNormalize(input, out var normalized))
            throw new BusinessException("Số điện thoại không hợp lệ. Vui lòng nhập 10 chữ số (ví dụ 0912345678) hoặc +84912345678.");
        return normalized;
    }
}
