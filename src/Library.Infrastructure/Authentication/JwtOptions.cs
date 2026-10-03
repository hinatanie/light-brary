using Microsoft.Extensions.Options;

namespace Library.Infrastructure.Authentication;

/// <summary>
/// Cấu hình JWT. <see cref="SecretKey"/> KHÔNG được nằm trong source: lấy từ biến môi trường
/// (Jwt__SecretKey) hoặc dotnet user-secrets. Thiếu/yếu thì ứng dụng không khởi động.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinSecretLengthBytes = 32;

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
}

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    private static readonly string[] PlaceholderFragments = { "change-this", "change_me", "changeme", "your-secret", "secret-key" };

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            errors.Add("Jwt:SecretKey chưa được cấu hình. Đặt biến môi trường Jwt__SecretKey hoặc dùng 'dotnet user-secrets set \"Jwt:SecretKey\" <giá trị>'.");
        }
        else
        {
            if (System.Text.Encoding.UTF8.GetByteCount(options.SecretKey) < JwtOptions.MinSecretLengthBytes)
                errors.Add($"Jwt:SecretKey quá ngắn (cần tối thiểu {JwtOptions.MinSecretLengthBytes} byte, khuyến nghị 'openssl rand -base64 48').");

            var lower = options.SecretKey.ToLowerInvariant();
            if (PlaceholderFragments.Any(lower.Contains))
                errors.Add("Jwt:SecretKey đang là giá trị mẫu. Hãy sinh một khóa ngẫu nhiên riêng.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer)) errors.Add("Jwt:Issuer chưa được cấu hình.");
        if (string.IsNullOrWhiteSpace(options.Audience)) errors.Add("Jwt:Audience chưa được cấu hình.");
        if (options.AccessTokenMinutes is < 1 or > 1440) errors.Add("Jwt:AccessTokenMinutes phải trong khoảng 1-1440.");

        return errors.Count > 0 ? ValidateOptionsResult.Fail(errors) : ValidateOptionsResult.Success;
    }
}
