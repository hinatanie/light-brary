namespace Library.Application.Common;

/// <summary>Các hằng số chính sách xác thực dùng chung.</summary>
public static class AuthConstants
{
    public const int MaxFailedLoginAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    /// <summary>
    /// Khoảng thời gian cho phép dùng lại refresh token vừa bị xoay (ví dụ 2 tab gọi refresh gần như đồng thời)
    /// mà không bị coi là tấn công tái sử dụng token.
    /// </summary>
    public static readonly TimeSpan RefreshReuseGrace = TimeSpan.FromSeconds(10);

    /// <summary>Claim đánh dấu token thuộc tài khoản bắt buộc đổi mật khẩu.</summary>
    public const string PasswordChangeRequiredClaim = "pwd_change_required";

    /// <summary>Số bản sao tối đa trong một phiếu mượn.</summary>
    public const int MaxCopiesPerBorrow = 10;

    /// <summary>Tiền phạt tối đa cho một cuốn sách.</summary>
    public const double MaxFineAmount = 10_000_000;
}
