using Library.Application.Exceptions;
using Library.Application.Validation;

namespace Library.UnitTests;

public class PhoneNumberNormalizerTests
{
    [Theory]
    [InlineData("0912345678", "0912345678")]
    [InlineData("+84912345678", "0912345678")]
    [InlineData("0084912345678", "0912345678")]
    [InlineData("0912.345.678", "0912345678")]
    [InlineData("0912-345-678", "0912345678")]
    [InlineData("(0912) 345 678", "0912345678")]
    [InlineData("+84 912 345 678", "0912345678")]
    public void Normalize_ValidVietnameseNumber_NormalizesToStandard(string input, string expected)
    {
        var result = PhoneNumberNormalizer.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("091234567890")] // 12 digits
    [InlineData("abcdefghij")]
    [InlineData("+12345678901")]
    public void Normalize_InvalidNumber_ThrowsBusinessException(string input)
    {
        Assert.Throws<BusinessException>(() => PhoneNumberNormalizer.Normalize(input));
    }
}
