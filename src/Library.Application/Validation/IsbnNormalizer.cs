using System.Text.RegularExpressions;
using Library.Application.Exceptions;

namespace Library.Application.Validation;

public static partial class IsbnNormalizer
{
    [GeneratedRegex(@"^(\d{9}[\dX]|\d{13})$")]
    private static partial Regex IsbnRegex();

    /// <summary>Bỏ dấu gạch/khoảng trắng, viết hoa X. Trả về null nếu rỗng; ném BusinessException nếu sai định dạng.</summary>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var cleaned = new string(input.Where(c => c != '-' && !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (!IsbnRegex().IsMatch(cleaned))
            throw new BusinessException("ISBN không hợp lệ. ISBN phải gồm 10 hoặc 13 ký tự (có thể có dấu gạch ngang).");
        return cleaned;
    }
}
