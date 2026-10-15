using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using light_brary.Data;
using light_brary.DTOs.Requests;
using light_brary.Entities;
using light_brary.Repositories;
using light_brary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("IDENTITY_TEST_CONNECTION")
    ?? throw new InvalidOperationException("Set IDENTITY_TEST_CONNECTION to a NEW disposable PostgreSQL database.");
if (new NpgsqlConnectionStringBuilder(connectionString).Database?.StartsWith("identity_checks_", StringComparison.Ordinal) != true)
    throw new InvalidOperationException("Test database name must start with identity_checks_.");

using (var connection = new NpgsqlConnection(connectionString))
{
    await connection.OpenAsync();
    using var command = new NpgsqlCommand("SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public'", connection);
    if (Convert.ToInt64(await command.ExecuteScalarAsync()) != 0)
        throw new InvalidOperationException("Test database must be empty.");
}

var services = new ServiceCollection();
services.AddLogging();
services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
services.AddIdentityApiEndpoints<User>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<AppDbContext>();
services.AddScoped<UserRepository>();
services.AddScoped<BorrowRecordRepository>();
services.AddScoped<UserService>();
using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

var project = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../light_brary"));
var url = $"http://127.0.0.1:{Environment.GetEnvironmentVariable("IDENTITY_TEST_HTTP_PORT") ?? "5299"}";
var adminPassword = $"Admin-{Guid.NewGuid()}!";
var start = new ProcessStartInfo("dotnet", "bin/Debug/net10.0/light_brary.dll")
{
    WorkingDirectory = project,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false
};
start.Environment["ASPNETCORE_URLS"] = url;
start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
start.Environment["ConnectionStrings__DefaultConnection"] = connectionString;
start.Environment["Seed__AdminPhone"] = "0999999999";
start.Environment["Seed__AdminPassword"] = adminPassword;
var output = new List<string>();
using var app = Process.Start(start)!;
app.OutputDataReceived += (_, args) => { if (args.Data != null) lock (output) output.Add(args.Data); };
app.ErrorDataReceived += (_, args) => { if (args.Data != null) lock (output) output.Add(args.Data); };
app.BeginOutputReadLine();
app.BeginErrorReadLine();
using var http = new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(15) };

async Task<JsonElement> Request(HttpMethod method, string path, HttpStatusCode expected,
    object? body = null, string? token = null)
{
    using var request = new HttpRequestMessage(method, path);
    if (body != null) request.Content = JsonContent.Create(body);
    if (token != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    using var response = await http.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();
    if (response.StatusCode != expected)
        throw new Exception($"{method} {path}: expected {expected}, got {response.StatusCode}: {content}");
    return string.IsNullOrEmpty(content) ? default : JsonDocument.Parse(content).RootElement.Clone();
}

async Task<string> Login(string phone, string password, string? oldToken = null)
{
    var result = await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.OK,
        new { userName = phone, password }, oldToken);
    return result.GetProperty("accessToken").GetString()!;
}

try
{
    var ready = false;
    for (var attempt = 0; attempt < 60 && !app.HasExited; attempt++)
    {
        try { using var response = await http.GetAsync("/api/me"); ready = true; break; }
        catch (HttpRequestException) { await Task.Delay(500); }
    }
    if (!ready) throw new Exception("Application did not start: " + string.Join('\n', output.TakeLast(15)));

    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: "invalid-token");
    var adminToken = await Login("0999999999", adminPassword);
    var roles = await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.OK, token: adminToken);
    var adminRoleId = roles.EnumerateArray().Single(role => role.GetProperty("name").GetString() == "Admin").GetProperty("id").GetGuid();
    var memberRoleId = roles.EnumerateArray().Single(role => role.GetProperty("name").GetString() == "User").GetProperty("id").GetGuid();

    var phone = "0912345678";
    var password = $"Member-{Guid.NewGuid()}!";
    var registration = await Request(HttpMethod.Post, "/api/auth/register", HttpStatusCode.OK,
        new { fullName = "Session Member", phoneNumber = phone, address = "Test address", password });
    var memberToken = registration.GetProperty("accessToken").GetString()!;
    var profile = await Request(HttpMethod.Get, "/api/me", HttpStatusCode.OK, token: memberToken);
    var memberId = profile.GetProperty("id").GetGuid();
    await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.Forbidden, token: memberToken);

    var changedPassword = $"Changed-{Guid.NewGuid()}!";
    await Request(HttpMethod.Put, "/api/me/password", HttpStatusCode.Unauthorized,
        new { currentPassword = "incorrect", newPassword = changedPassword }, memberToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.OK, token: memberToken);
    await Request(HttpMethod.Put, "/api/me/password", HttpStatusCode.OK,
        new { currentPassword = password, newPassword = changedPassword }, memberToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: memberToken);
    memberToken = await Login(phone, changedPassword, memberToken);

    await Request(HttpMethod.Put, "/api/me", HttpStatusCode.OK,
        new { fullName = "Updated Member", phoneNumber = phone, address = "Updated address" }, memberToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.OK, token: memberToken);

    var resetPassword = $"Reset-{Guid.NewGuid()}!";
    await scope.ServiceProvider.GetRequiredService<UserService>().AdminUpdatePasswordAsync(memberId,
        new AdminUpdatePasswordRequest { NewPassword = resetPassword });
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: memberToken);
    memberToken = await Login(phone, resetPassword);

    // An actual role change revokes every old session; a no-op must preserve it.
    var secondToken = await Login(phone, resetPassword);
    await Request(HttpMethod.Post, $"/api/users/{memberId}/roles/{memberRoleId}", HttpStatusCode.NoContent, token: adminToken);
    await Request(HttpMethod.Delete, $"/api/users/{memberId}/roles/{adminRoleId}", HttpStatusCode.NoContent, token: adminToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.OK, token: memberToken);
    await Request(HttpMethod.Post, $"/api/users/{memberId}/roles/{adminRoleId}", HttpStatusCode.NoContent, token: adminToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: memberToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: secondToken);
    memberToken = await Login(phone, resetPassword);
    await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.OK, token: memberToken);
    await Request(HttpMethod.Delete, $"/api/users/{memberId}/roles/{adminRoleId}", HttpStatusCode.NoContent, token: adminToken);
    await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.Unauthorized, token: memberToken);
    memberToken = await Login(phone, resetPassword);
    await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.Forbidden, token: memberToken);

    var customRole = await Request(HttpMethod.Post, "/api/roles", HttpStatusCode.Created,
        new { name = "SessionReader" }, adminToken);
    var customRoleId = customRole.GetProperty("id").GetGuid();
    await Request(HttpMethod.Post, $"/api/users/{memberId}/roles/{customRoleId}", HttpStatusCode.NoContent, token: adminToken);
    memberToken = await Login(phone, resetPassword);
    await Request(HttpMethod.Put, $"/api/roles/{customRoleId}", HttpStatusCode.OK,
        new { name = "SessionReader" }, adminToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.OK, token: memberToken);
    await Request(HttpMethod.Put, $"/api/roles/{customRoleId}", HttpStatusCode.OK,
        new { name = "RenamedReader" }, adminToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: memberToken);
    memberToken = await Login(phone, resetPassword);

    // Pause a rename at the role row while another request assigns a new member.
    // Assignment must either wait for the rename or have its old token revoked by it.
    var concurrentRegistration = await Request(HttpMethod.Post, "/api/auth/register", HttpStatusCode.OK,
        new { fullName = "Concurrent Member", phoneNumber = "0912345680", address = "Test address", password = resetPassword });
    var concurrentProfile = await Request(HttpMethod.Get, "/api/me", HttpStatusCode.OK,
        token: concurrentRegistration.GetProperty("accessToken").GetString());
    var concurrentId = concurrentProfile.GetProperty("id").GetGuid();
    using (var locker = new NpgsqlConnection(connectionString))
    using (var observer = new NpgsqlConnection(connectionString))
    {
        await locker.OpenAsync();
        await observer.OpenAsync();
        using var transaction = await locker.BeginTransactionAsync();
        using (var command = new NpgsqlCommand("SELECT \"Id\" FROM \"AspNetRoles\" WHERE \"Id\" = @id FOR NO KEY UPDATE", locker, transaction))
        {
            command.Parameters.AddWithValue("id", customRoleId);
            await command.ExecuteScalarAsync();
        }

        async Task WaitForBlockedRoleWrites(int count, Task? alternative = null)
        {
            for (var attempt = 0; attempt < 100; attempt++)
            {
                if (alternative?.IsCompleted == true) return;
                using var command = new NpgsqlCommand("""
                    SELECT count(*) FROM pg_stat_activity
                    WHERE datname = current_database() AND wait_event_type = 'Lock'
                        AND query LIKE '%AspNetRoles%'
                    """, observer);
                if (Convert.ToInt32(await command.ExecuteScalarAsync()) >= count) return;
                await Task.Delay(50);
            }
            throw new Exception("Concurrent role requests did not reach the expected synchronization point.");
        }

        var rename = Request(HttpMethod.Put, $"/api/roles/{customRoleId}", HttpStatusCode.OK,
            new { name = "ConcurrentReader" }, adminToken);
        await WaitForBlockedRoleWrites(1);
        var assign = Request(HttpMethod.Post, $"/api/users/{concurrentId}/roles/{customRoleId}",
            HttpStatusCode.NoContent, token: adminToken);
        await WaitForBlockedRoleWrites(2, assign);
        string? tokenBeforeRename = null;
        if (assign.IsCompleted)
        {
            await assign;
            tokenBeforeRename = await Login("0912345680", resetPassword);
        }
        await transaction.CommitAsync();
        await Task.WhenAll(rename, assign);
        if (tokenBeforeRename != null)
            await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: tokenBeforeRename);
    }
    var concurrentToken = await Login("0912345680", resetPassword);
    concurrentProfile = await Request(HttpMethod.Get, "/api/me", HttpStatusCode.OK, token: concurrentToken);
    if (!concurrentProfile.GetProperty("roles").EnumerateArray().Any(role => role.GetString() == "ConcurrentReader"))
        throw new Exception("Concurrent assignment did not use the renamed role.");
    memberToken = await Login(phone, resetPassword);

    await db.Users.Where(user => user.Id == memberId).ExecuteUpdateAsync(setters => setters
        .SetProperty(user => user.LockoutEnabled, true)
        .SetProperty(user => user.LockoutEnd, DateTimeOffset.UtcNow.AddMinutes(10)));
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: memberToken);
    await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.Unauthorized,
        new { userName = phone, password = resetPassword });
    // Registration remains public even when the client attaches a locked user's token.
    await Request(HttpMethod.Post, "/api/auth/register", HttpStatusCode.OK,
        new { fullName = "Public Member", phoneNumber = "0912345679", address = "Test address", password = resetPassword }, memberToken);
    await db.Users.Where(user => user.Id == memberId).ExecuteUpdateAsync(setters => setters
        .SetProperty(user => user.LockoutEnd, (DateTimeOffset?)null));
    memberToken = await Login(phone, resetPassword);

    // Deletion must reject the token before AdminOnly authorization, rather than returning 403.
    await Request(HttpMethod.Delete, $"/api/users/{memberId}", HttpStatusCode.NoContent, token: adminToken);
    await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.Unauthorized, token: memberToken);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: memberToken);
    await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.OK, token: adminToken);
    Console.WriteLine("PASS: anonymous/invalid tokens, password change/reset, profile edits, role changes/rename/no-ops/concurrency, multiple sessions, lockout, deletion and public auth endpoints.");
}
finally
{
    if (!app.HasExited) app.Kill(entireProcessTree: true);
    await app.WaitForExitAsync();
}
