namespace Library.Application.Interfaces;

/// <summary>Chạy một nhóm thao tác trong cùng một transaction cơ sở dữ liệu.</summary>
public interface IUnitOfWork
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work);
}
