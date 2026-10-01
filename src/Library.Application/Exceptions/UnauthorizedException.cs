namespace Library.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Bạn không có quyền thực hiện thao tác này.") : base(message)
    {
    }
}
