namespace light_brary.Application.Interfaces
{
    public interface IPasswordHasher
    {
        /// <summary>Băm mật khẩu dạng plain text.</summary>
        string Hash(string password);

        /// <summary>Kiểm tra mật khẩu plain text có khớp với chuỗi đã băm hay không.</summary>
        bool Verify(string password, string passwordHash);
    }
}