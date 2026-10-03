using Library.Application.DTOs.Auth;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Library.IntegrationTests;

public class DuplicateConstraintTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DuplicateConstraintTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterAsync_DuplicatePhoneOrEmail_ThrowsConflictException()
    {
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var phone = $"09{Random.Shared.Next(10000000, 99999999)}";
        var email = $"user_{Guid.NewGuid():N}@test.com";

        var req1 = new RegisterRequest("User One", email, "StrongPassword#2026!", phone, "Address 1");
        await authService.RegisterAsync(req1);

        // Đăng ký lại cùng email -> ConflictException
        var reqSameEmail = new RegisterRequest("User Two", email, "StrongPassword#2026!", $"09{Random.Shared.Next(10000000, 99999999)}", "Address 2");
        await Assert.ThrowsAsync<ConflictException>(() => authService.RegisterAsync(reqSameEmail));

        // Đăng ký lại cùng số điện thoại -> ConflictException
        var reqSamePhone = new RegisterRequest("User Three", $"unique_{Guid.NewGuid():N}@test.com", "StrongPassword#2026!", phone, "Address 3");
        await Assert.ThrowsAsync<ConflictException>(() => authService.RegisterAsync(reqSamePhone));
    }

    [Fact]
    public async Task AddBook_DuplicateIsbn_DatabaseThrowsConflictException()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        var isbn = $"978{Random.Shared.Next(100000000, 999999999)}X";

        var book1 = new Book { Title = "Book 1", Author = "Author 1", CategoryId = 1, Isbn = isbn };
        await context.Books.AddAsync(book1);
        await context.SaveChangesAsync();

        var book2 = new Book { Title = "Book 2", Author = "Author 2", CategoryId = 1, Isbn = isbn };
        await context.Books.AddAsync(book2);

        // Bị chặn bởi filtered unique index UX_Books_Isbn và ném ConflictException
        await Assert.ThrowsAsync<ConflictException>(() => context.SaveChangesAsync());
    }
}
