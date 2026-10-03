namespace Library.Application.Exceptions;

/// <summary>Tài khoản đang bị khóa tạm thời do đăng nhập sai nhiều lần (HTTP 423).</summary>
public class LockedException : Exception
{
    public DateTime LockoutEndUtc { get; }

    public LockedException(string message, DateTime lockoutEndUtc) : base(message)
    {
        LockoutEndUtc = lockoutEndUtc;
    }
}
