using Library.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly string TestConnectionString =
        Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=lightbrary_db;Username=postgres;Password=Asdf1234!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["Jwt:SecretKey"] = "IntegrationTest_Secret_Key_At_Least_32_Bytes_Long!",
                ["Database:AutoMigrate"] = "true"
            });
        });

        builder.UseEnvironment("Development");
    }
}
