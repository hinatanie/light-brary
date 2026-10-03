using Library.Application.Exceptions;
using Library.Application.Validation;

namespace Library.UnitTests;

public class IsbnNormalizerTests
{
    [Theory]
    [InlineData("0-306-40615-2", "0306406152")]
    [InlineData("978-0-306-40615-7", "9780306406157")]
    [InlineData("030640615X", "030640615X")]
    [InlineData("0-306-40615-x", "030640615X")]
    public void Normalize_ValidIsbn_StripsDashesAndUppercases(string input, string expected)
    {
        var result = IsbnNormalizer.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_NullOrWhitespace_ReturnsNull(string? input)
    {
        var result = IsbnNormalizer.Normalize(input);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("ABCDEFGHIJ")]
    [InlineData("978030640615700")]
    public void Normalize_InvalidIsbn_ThrowsBusinessException(string input)
    {
        Assert.Throws<BusinessException>(() => IsbnNormalizer.Normalize(input));
    }
}
