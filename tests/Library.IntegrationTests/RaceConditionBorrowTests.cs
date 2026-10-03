using Library.Application.DTOs.BorrowRecords;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Domain.Constants;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Library.IntegrationTests;

public class RaceConditionBorrowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RaceConditionBorrowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAsync_ConcurrentBorrowSameCopy_AllowsExactlyOneBorrow()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        // 1. Chuẩn bị dữ liệu mẫu
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"reader_{Guid.NewGuid():N}@lightbrary.vn",
            PhoneNumber = $"09{Random.Shared.Next(10000000, 99999999)}",
            FullName = "Độc giả kiểm thử Concurrency",
            CreatedAt = DateTime.UtcNow
        };
        var memberRole = await context.Roles.FirstAsync(r => r.Name == RoleNames.Member);
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = memberRole.Id });
        await context.Users.AddAsync(user);

        var book = new Book
        {
            Title = "Concurrency in C#",
            Author = "Stephen Cleary",
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow
        };
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();

        var copy = new BookCopy
        {
            BookId = book.Id,
            Barcode = $"BC_{Guid.NewGuid():N}"[..15],
            Status = BookCopyStatus.Available
        };
        await context.BookCopies.AddAsync(copy);
        await context.SaveChangesAsync();

        // 2. Chạy 10 request mượn cùng 1 bản sao sách song song cùng một lúc
        const int concurrentRequests = 10;
        var successes = 0;
        var failures = 0;

        var tasks = Enumerable.Range(0, concurrentRequests).Select(async _ =>
        {
            using var reqScope = _factory.Services.CreateScope();
            var service = reqScope.ServiceProvider.GetRequiredService<IBorrowRecordService>();

            try
            {
                var req = new CreateBorrowRequest(
                    user.Id,
                    DateTime.UtcNow.AddDays(7),
                    new List<int> { copy.Id },
                    "Mượn thử nghiệm đồng thời");

                await service.CreateAsync(req);
                Interlocked.Increment(ref successes);
            }
            catch (Exception ex) when (ex is BusinessException or ConflictException)
            {
                Interlocked.Increment(ref failures);
            }
        });

        await Task.WhenAll(tasks);

        // 3. Khẳng định: CHÍNH XÁC 1 request thành công, 9 request còn lại bị từ chối
        Assert.Equal(1, successes);
        Assert.Equal(concurrentRequests - 1, failures);

        // 4. Kiểm tra bất biến cơ sở dữ liệu
        using var verifyScope = _factory.Services.CreateScope();
        var verifyCtx = verifyScope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        var updatedCopy = await verifyCtx.BookCopies.AsNoTracking().FirstAsync(c => c.Id == copy.Id);
        Assert.Equal(BookCopyStatus.Borrowed, updatedCopy.Status);

        var activeBorrowCount = await verifyCtx.BorrowDetails
            .CountAsync(bd => bd.BookCopyId == copy.Id && bd.Status == BorrowDetailStatus.Borrowing);
        Assert.Equal(1, activeBorrowCount);
    }
}
