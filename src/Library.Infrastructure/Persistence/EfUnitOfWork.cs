using System.Data;
using Library.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _context;

    public EfUnitOfWork(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work)
    {
        // Đã ở trong transaction (gọi lồng nhau) thì dùng lại.
        if (_context.Database.CurrentTransaction != null)
            return await work();

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        var result = await work();
        await transaction.CommitAsync();
        return result; // Nếu work() ném lỗi, Dispose của transaction sẽ tự rollback.
    }
}
