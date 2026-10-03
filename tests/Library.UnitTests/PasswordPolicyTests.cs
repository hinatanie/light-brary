using Library.Application.Exceptions;
using Library.Application.Validation;

namespace Library.UnitTests;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Short1!")] // < 10 chars
    [InlineData("alllowercase123!")] // no uppercase
    [InlineData("ALLUPPERCASE123!")] // no lowercase
    [InlineData("NoDigitsHere!")] // no digit
    [InlineData("1234567890")] // no letter
    public void Validate_InvalidComposition_ReturnsErrors(string password)
    {
        var errors = PasswordPolicy.Validate(password);
        Assert.NotEmpty(errors);
        Assert.Throws<BusinessException>(() => PasswordPolicy.EnsureValid(password));
    }

    [Theory]
    [InlineData("Password123!")] // common weak
    [InlineData("admin123456!A")] // starts with admin123
    [InlineData("matkhau1234A!")] // starts with matkhau
    public void Validate_WeakCommonPasswords_ReturnsErrors(string password)
    {
        var errors = PasswordPolicy.Validate(password);
        Assert.Contains(errors, e => e.Contains("phổ biến"));
    }

    [Fact]
    public void Validate_ContainsPhoneNumber_ReturnsError()
    {
        var errors = PasswordPolicy.Validate("Abc0916114536!", phoneNumber: "0916114536");
        Assert.Contains(errors, e => e.Contains("số điện thoại"));
    }

    [Fact]
    public void Validate_ContainsEmailLocalPart_ReturnsError()
    {
        var errors = PasswordPolicy.Validate("AbcNguyenVanA123!", email: "nguyenvana@gmail.com");
        Assert.Contains(errors, e => e.Contains("email"));
    }

    [Theory]
    [InlineData("ThuVienSach2026!")]
    [InlineData("SecurePassphrase#2026")]
    [InlineData("KhongDuocQuen123!")]
    public void Validate_ValidPassword_ReturnsNoErrors(string password)
    {
        var errors = PasswordPolicy.Validate(password);
        Assert.Empty(errors);
        PasswordPolicy.EnsureValid(password);
    }
}
