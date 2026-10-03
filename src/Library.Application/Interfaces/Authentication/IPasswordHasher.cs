namespace Library.Application.Interfaces.Authentication;

public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>
    /// Kiểm tra mật khẩu. Nếu <paramref name="hash"/> là null (user không tồn tại / chưa có mật khẩu),
    /// vẫn thực hiện một phép băm giả để thời gian phản hồi không lộ việc user có tồn tại hay không.
    /// </summary>
    bool Verify(string password, string? hash);
}
