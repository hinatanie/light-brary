using System.Net;
using System.Text.Json;
using light_brary.Data;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using light_brary.Entities;
using light_brary.Enums;
using light_brary.Exceptions;
using light_brary.Repositories;
using light_brary.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace light_brary.Tests;

public class BookCacheTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly List<Guid> bookIds = [];
    private ServiceProvider provider = null!;
    private AsyncServiceScope scope;
    private AppDbContext Context => scope.ServiceProvider.GetRequiredService<AppDbContext>();
    private IDistributedCache Cache => scope.ServiceProvider.GetRequiredService<IDistributedCache>();
    private BookService Books => scope.ServiceProvider.GetRequiredService<BookService>();
    private BookCopyService Copies => scope.ServiceProvider.GetRequiredService<BookCopyService>();
    private BorrowRecordService Borrows => scope.ServiceProvider.GetRequiredService<BorrowRecordService>();

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        services.AddIdentityCore<User>().AddEntityFrameworkStores<AppDbContext>();

        // Có thể chạy cùng bộ test với Redis thật bằng biến môi trường này.
        if (Environment.GetEnvironmentVariable("LIGHTBRARY_TEST_REDIS") is { Length: > 0 } redis)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redis;
                options.InstanceName = $"LightBrary:Tests:{Guid.NewGuid()}:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<BookRepository>();
        services.AddScoped<BookCopyRepository>();
        services.AddScoped<CategoryRepository>();
        services.AddScoped<BorrowRecordRepository>();
        services.AddScoped<BorrowDetailRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<BookService>();
        services.AddScoped<BookCopyService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<BorrowRecordService>();
        services.AddScoped<UserService>();
        provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        scope = provider.CreateAsyncScope();
        await Context.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task GetById_CachesResponse_AndReadsCacheWithoutDatabase()
    {
        var book = await SeedBookAsync();
        Context.BookCopies.Add(new BookCopy
        {
            BookId = book.Id,
            Barcode = Guid.NewGuid().ToString(),
            Status = BookCopyStatus.Borrowed
        });
        await Context.SaveChangesAsync();

        var response = await ReadAsync(book.Id);
        Assert.Equal(2, response.TotalCopies);
        Assert.Equal(1, response.AvailableCopies);
        Assert.Equal(book.Category.Name, response.CategoryName);
        Assert.Equal(JsonSerializer.Serialize(response), await Cache.GetStringAsync($"books:{book.Id}"));

        // SQLite mất dữ liệu khi đóng kết nối; cache hit phải không truy vấn DB.
        await connection.CloseAsync();
        var cachedResponse = await ReadAsync(book.Id);
        Assert.Equal(JsonSerializer.Serialize(response), JsonSerializer.Serialize(cachedResponse));
    }

    [Fact]
    public async Task GetById_MissingBook_ReturnsNotFoundWithoutCaching()
    {
        var id = Guid.NewGuid();
        var error = await Assert.ThrowsAsync<AppException>(() => Books.GetByIdAsync(id));
        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
        Assert.Null(await Cache.GetStringAsync($"books:{id}"));
    }

    [Fact]
    public async Task GetById_CachedJsonNull_ReloadsBook()
    {
        var book = await SeedBookAsync();
        await Cache.SetStringAsync($"books:{book.Id}", "null");
        Assert.Equal(book.Title, (await ReadAsync(book.Id)).Title);
        Assert.NotEqual("null", await Cache.GetStringAsync($"books:{book.Id}"));
    }

    [Fact]
    public async Task UpdateBook_InvalidatesCache_AndReloadsUpdatedDetails()
    {
        var book = await SeedBookAsync();
        await ReadAsync(book.Id);
        await Books.UpdateAsync(book.Id, new SaveBookRequest
        {
            Title = "Tên sách mới",
            Author = book.Author,
            ISBN = book.ISBN,
            CategoryId = book.CategoryId
        });
        Assert.Null(await Cache.GetStringAsync($"books:{book.Id}"));
        Assert.Equal("Tên sách mới", (await ReadAsync(book.Id)).Title);
    }

    [Fact]
    public async Task DeleteBook_InvalidatesCache_AndDoesNotReturnDeletedBook()
    {
        var book = await SeedBookAsync();
        await ReadAsync(book.Id);
        await Books.DeleteAsync(book.Id);
        Assert.Null(await Cache.GetStringAsync($"books:{book.Id}"));
        var error = await Assert.ThrowsAsync<AppException>(() => ReadAsync(book.Id));
        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
    }

    [Fact]
    public async Task CreateAndDeleteCopy_RefreshesCopyCounts()
    {
        var book = await SeedBookAsync();
        Assert.Equal(1, (await ReadAsync(book.Id)).TotalCopies);
        var copy = await Copies.CreateAsync(book.Id, new SaveBookCopyRequest
        {
            Barcode = Guid.NewGuid().ToString()
        });
        Assert.Null(await Cache.GetStringAsync($"books:{book.Id}"));
        var response = await ReadAsync(book.Id);
        Assert.Equal(2, response.TotalCopies);
        Assert.Equal(2, response.AvailableCopies);

        await Copies.DeleteAsync(copy.Id);
        Assert.Null(await Cache.GetStringAsync($"books:{book.Id}"));
        response = await ReadAsync(book.Id);
        Assert.Equal(1, response.TotalCopies);
        Assert.Equal(1, response.AvailableCopies);
    }

    [Fact]
    public async Task BorrowAndReturnBooks_RefreshesAffectedBooksOnly()
    {
        var first = await SeedBookAsync();
        var second = await SeedBookAsync();
        await ReadAsync(first.Id);
        await ReadAsync(second.Id);
        var record = await BorrowAsync(first, second);
        foreach (var book in new[] { first, second })
        {
            Assert.Null(await Cache.GetStringAsync($"books:{book.Id}"));
            Assert.Equal(0, (await ReadAsync(book.Id)).AvailableCopies);
        }

        await Borrows.ReturnMultipleBooksAsync(record.Id, new ReturnBooksRequest
        {
            BorrowDetailIds = [record.Details.Single(detail => detail.BookId == first.Id).Id]
        });
        Assert.Null(await Cache.GetStringAsync($"books:{first.Id}"));
        Assert.NotNull(await Cache.GetStringAsync($"books:{second.Id}"));
        Assert.Equal(1, (await ReadAsync(first.Id)).AvailableCopies);
        Assert.Equal(0, (await ReadAsync(second.Id)).AvailableCopies);
    }

    [Fact]
    public async Task MarkLostBook_InvalidatesCache()
    {
        var book = await SeedBookAsync();
        var record = await BorrowAsync(book);
        await ReadAsync(book.Id);
        await Borrows.MarkLostBookAsync(record.Details.Single().Id);
        Assert.Null(await Cache.GetStringAsync($"books:{book.Id}"));
        Assert.Equal(0, (await ReadAsync(book.Id)).AvailableCopies);
    }

    [Fact]
    public async Task UpdateCategory_InvalidatesAllBooksInCategory()
    {
        var first = await SeedBookAsync();
        var second = await SeedBookAsync(first.CategoryId);
        var unrelated = await SeedBookAsync();
        await ReadAsync(first.Id);
        await ReadAsync(second.Id);
        await ReadAsync(unrelated.Id);

        await scope.ServiceProvider.GetRequiredService<CategoryService>()
            .UpdateAsync(first.CategoryId, new SaveCategoryRequest
            {
                Name = "Thể loại mới",
                Description = "Mô tả mới"
            });
        foreach (var book in new[] { first, second })
        {
            Assert.Null(await Cache.GetStringAsync($"books:{book.Id}"));
            var response = await ReadAsync(book.Id);
            Assert.Equal("Thể loại mới", response.CategoryName);
            Assert.Equal("Mô tả mới", response.Category.Description);
        }
        Assert.NotNull(await Cache.GetStringAsync($"books:{unrelated.Id}"));
    }

    private async Task<Book> SeedBookAsync(Guid? categoryId = null)
    {
        var category = categoryId is { } id
            ? await Context.Categories.SingleAsync(category => category.Id == id)
            : new Category { Name = Guid.NewGuid().ToString()[..12] };
        var book = new Book
        {
            Title = "Sách kiểm thử",
            Author = "Tác giả",
            ISBN = Guid.NewGuid().ToString(),
            Category = category,
            BookCopies = [new BookCopy { Barcode = Guid.NewGuid().ToString() }]
        };
        Context.Books.Add(book);
        await Context.SaveChangesAsync();
        bookIds.Add(book.Id);
        return book;
    }

    private Task<BookResponse> ReadAsync(Guid id)
    {
        Context.ChangeTracker.Clear();
        return Books.GetByIdAsync(id);
    }

    private async Task<BorrowRecordResponse> BorrowAsync(params Book[] books)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "reader",
            FullName = "Người đọc",
            Address = "Địa chỉ kiểm thử",
            PhoneNumber = "0123456789"
        };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return await Borrows.CreateAsync(new CreateBorrowRecordRequest
        {
            UserId = user.Id,
            BookCopyIds = books.Select(book => book.BookCopies.Single().Id).ToList(),
            DueDate = DateTime.UtcNow.AddDays(7)
        });
    }

    public async Task DisposeAsync()
    {
        foreach (var id in bookIds)
        {
            await Cache.RemoveAsync($"books:{id}");
        }
        await scope.DisposeAsync();
        await provider.DisposeAsync();
        await connection.DisposeAsync();
    }
}
