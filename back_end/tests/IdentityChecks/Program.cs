using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using light_brary.Data;
using light_brary.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("IDENTITY_TEST_CONNECTION")
    ?? throw new InvalidOperationException("Set IDENTITY_TEST_CONNECTION to a NEW disposable PostgreSQL database.");
var settings = new NpgsqlConnectionStringBuilder(connectionString);
if (settings.Database?.StartsWith("identity_checks_", StringComparison.Ordinal) != true)
    throw new InvalidOperationException("Test database name must start with identity_checks_. Never use an application database.");

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
using (var emptyCheck = new NpgsqlCommand("SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public'", connection))
    Check(Convert.ToInt64(await emptyCheck.ExecuteScalarAsync()) == 0, "Test database must be empty.");

using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options);
var migrator = context.GetService<IMigrator>();
await migrator.MigrateAsync("20261009010323_InitialCreate");

var adminId = Guid.NewGuid();
var roleId = Guid.NewGuid();
var recordId = Guid.NewGuid();
var legacyPassword = $"Legacy-{Guid.NewGuid()}!";
var legacyHash = new PasswordHasher<User>().HashPassword(new User(), legacyPassword);
using (var seed = new NpgsqlCommand("""
    INSERT INTO "AspNetRoles" ("Id", "Name") VALUES (@role, 'Admin');
    INSERT INTO "AspNetUsers" ("Id", "FullName", "PhoneNumber", "Address", "PasswordHash", "CreatedAt")
    VALUES (@admin, repeat('A', 51), '0999999999', NULL, @hash, now());
    INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") VALUES (@admin, @role);
    INSERT INTO "BorrowRecords" ("Id", "BorrowDate", "DueDate", "Status", "UserId")
    VALUES (@record, now(), now() + interval '14 days', 0, @admin);
    """, connection))
{
    seed.Parameters.AddWithValue("role", roleId);
    seed.Parameters.AddWithValue("admin", adminId);
    seed.Parameters.AddWithValue("hash", legacyHash);
    seed.Parameters.AddWithValue("record", recordId);
    await seed.ExecuteNonQueryAsync();
}
try
{
    await migrator.MigrateAsync();
    throw new Exception("Migration accepted a name exceeding the new length limit.");
}
catch (PostgresException exception) when (exception.SqlState == "P0001")
{
    using var verify = new NpgsqlCommand("SELECT length(\"FullName\") FROM \"AspNetUsers\" WHERE \"Id\" = @admin", connection);
    verify.Parameters.AddWithValue("admin", adminId);
    Check(Convert.ToInt32(await verify.ExecuteScalarAsync()) == 51, "Migration truncated existing name.");
    using var fix = new NpgsqlCommand("UPDATE \"AspNetUsers\" SET \"FullName\" = 'Legacy Admin' WHERE \"Id\" = @admin", connection);
    fix.Parameters.AddWithValue("admin", adminId);
    await fix.ExecuteNonQueryAsync();
}
await migrator.MigrateAsync();
var migratedUser = await context.Users.SingleAsync(user => user.Id == adminId);
Check(migratedUser.PasswordHash == legacyHash && migratedUser.UserName == "0999999999" &&
    migratedUser.NormalizedUserName == "0999999999" && migratedUser.SecurityStamp != null &&
    migratedUser.Address == "", "Migration must preserve hash and initialize Identity fields.");
Check(await context.UserRoles.AnyAsync(role => role.UserId == adminId && role.RoleId == roleId), "Legacy role assignment lost.");
Check(await context.BorrowRecords.AnyAsync(record => record.Id == recordId && record.UserId == adminId), "Borrow history lost.");
Check(!context.Database.HasPendingModelChanges(), "Model and migrations differ.");

var project = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../light_brary"));
var processInfo = new ProcessStartInfo("dotnet", "bin/Debug/net10.0/light_brary.dll")
{
    WorkingDirectory = project,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false
};
var port = Environment.GetEnvironmentVariable("IDENTITY_TEST_HTTP_PORT") ?? "5199";
var url = $"http://127.0.0.1:{port}";
processInfo.Environment["ASPNETCORE_URLS"] = url;
processInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
processInfo.Environment["ConnectionStrings__DefaultConnection"] = connectionString;
processInfo.Environment["Jwt__SecretKey"] = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
processInfo.Environment["Jwt__Issuer"] = "IdentityChecks";
processInfo.Environment["Jwt__Audience"] = "IdentityChecks";
var output = new List<string>();
using var app = Process.Start(processInfo)!;
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
    Check(response.StatusCode == expected, $"{method} {path}: expected {expected}, got {response.StatusCode}: {content}");
    return string.IsNullOrEmpty(content) ? default : JsonDocument.Parse(content).RootElement.Clone();
}

try
{
    var ready = false;
    for (var attempt = 0; attempt < 60 && !app.HasExited; attempt++)
    {
        try { using var response = await http.GetAsync("/api/me"); ready = true; break; }
        catch (HttpRequestException) { await Task.Delay(500); }
    }
    Check(ready, "Application did not start: " + string.Join('\n', output.TakeLast(15)));
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.Unauthorized, token: "invalid-token");

    var adminLogin = await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.OK,
        new { phoneNumber = "0999999999", password = legacyPassword });
    var adminToken = adminLogin.GetProperty("accessToken").GetString()!;
    Check(adminLogin.GetProperty("user").GetProperty("id").GetGuid() == adminId, "Legacy login changed user ID.");
    var roles = await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.OK, token: adminToken);
    var memberRoleId = roles.EnumerateArray().Single(role => role.GetProperty("name").GetString() == "User").GetProperty("id").GetGuid();
    await Request(HttpMethod.Delete, $"/api/users/{adminId}", HttpStatusCode.Conflict, token: adminToken);
    await Request(HttpMethod.Delete, $"/api/users/{adminId}/roles/{roleId}", HttpStatusCode.BadRequest, token: adminToken);

    var phone = "0912345678";
    var password = $"Member-{Guid.NewGuid()}!";
    var registration = new { fullName = "Identity Member", phoneNumber = phone, address = "Test address", password };
    await Request(HttpMethod.Post, "/api/auth/register", HttpStatusCode.BadRequest,
        new { fullName = "Invalid Member", phoneNumber = phone, password });
    await Request(HttpMethod.Post, "/api/auth/register", HttpStatusCode.BadRequest,
        new { fullName = new string('A', 51), phoneNumber = phone, address = "Test address", password });
    var member = await Request(HttpMethod.Post, "/api/auth/register", HttpStatusCode.Created, registration);
    var memberId = member.GetProperty("user").GetProperty("id").GetGuid();
    var memberToken = member.GetProperty("accessToken").GetString()!;
    Check(memberId != Guid.Empty && member.GetProperty("user").GetProperty("roles").EnumerateArray()
        .Any(role => role.GetString() == "User"), "Registration missing ID or default role.");
    await Request(HttpMethod.Post, "/api/auth/register", HttpStatusCode.Conflict, registration);
    await Request(HttpMethod.Get, "/api/me", HttpStatusCode.OK, token: memberToken);
    await Request(HttpMethod.Get, "/api/roles", HttpStatusCode.Forbidden, token: memberToken);
    await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.Unauthorized,
        new { phoneNumber = phone, password = "incorrect" });

    var changedPassword = $"Changed-{Guid.NewGuid()}!";
    await Request(HttpMethod.Put, "/api/me/password", HttpStatusCode.Unauthorized,
        new { currentPassword = "incorrect", newPassword = changedPassword }, memberToken);
    await Request(HttpMethod.Put, "/api/me/password", HttpStatusCode.OK,
        new { currentPassword = password, newPassword = changedPassword }, memberToken);
    await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.Unauthorized,
        new { phoneNumber = phone, password });
    await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.OK,
        new { phoneNumber = phone, password = changedPassword });

    phone = "0912345679";
    await Request(HttpMethod.Put, "/api/me", HttpStatusCode.OK,
        new { fullName = "Updated Member", phoneNumber = phone, address = "Updated address" }, memberToken);
    await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.Unauthorized,
        new { phoneNumber = "0912345678", password = changedPassword });
    await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.OK,
        new { phoneNumber = phone, password = changedPassword });

    var resetPassword = $"Reset-{Guid.NewGuid()}!";
    await Request(HttpMethod.Put, $"/api/users/{memberId}/password", HttpStatusCode.OK,
        new { newPassword = resetPassword }, adminToken);
    await Request(HttpMethod.Post, "/api/auth/login", HttpStatusCode.OK,
        new { phoneNumber = phone, password = resetPassword });

    var customRole = await Request(HttpMethod.Post, "/api/roles", HttpStatusCode.Created,
        new { name = "TestReader" }, adminToken);
    var customRoleId = customRole.GetProperty("id").GetGuid();
    await Request(HttpMethod.Post, $"/api/users/{memberId}/roles/{customRoleId}", HttpStatusCode.NoContent, token: adminToken);
    var assigned = await Request(HttpMethod.Get, $"/api/users/{memberId}/roles", HttpStatusCode.OK, token: adminToken);
    Check(assigned.EnumerateArray().Any(role => role.GetProperty("id").GetGuid() == customRoleId), "Role was not assigned.");
    await Request(HttpMethod.Delete, $"/api/roles/{customRoleId}", HttpStatusCode.Conflict, token: adminToken);
    await Request(HttpMethod.Delete, $"/api/users/{memberId}/roles/{customRoleId}", HttpStatusCode.NoContent, token: adminToken);
    await Request(HttpMethod.Delete, $"/api/roles/{customRoleId}", HttpStatusCode.NoContent, token: adminToken);
    var filtered = await Request(HttpMethod.Get, "/api/users?role=user", HttpStatusCode.OK, token: adminToken);
    Check(filtered.EnumerateArray().Any(user => user.GetProperty("id").GetGuid() == memberId), "Role filter failed.");
    var history = await Request(HttpMethod.Get, $"/api/users/{adminId}/borrow-records", HttpStatusCode.OK, token: adminToken);
    Check(history.EnumerateArray().Any(record => record.GetProperty("id").GetGuid() == recordId), "Migrated borrow history not accessible.");
    var recordResponse = await Request(HttpMethod.Get, $"/api/borrow-records/{recordId}", HttpStatusCode.OK, token: adminToken);
    Check(recordResponse.GetProperty("user").GetProperty("roles").EnumerateArray().Any(role => role.GetString() == "Admin"),
        "Borrow record user roles missing.");
    using (var removeTestHistory = new NpgsqlCommand("DELETE FROM \"BorrowRecords\" WHERE \"Id\" = @record", connection))
    {
        removeTestHistory.Parameters.AddWithValue("record", recordId);
        await removeTestHistory.ExecuteNonQueryAsync();
    }
    await Request(HttpMethod.Delete, $"/api/users/{adminId}", HttpStatusCode.BadRequest, token: adminToken);
    await Request(HttpMethod.Delete, $"/api/users/{memberId}/roles/{memberRoleId}", HttpStatusCode.NoContent, token: adminToken);
    await Request(HttpMethod.Delete, $"/api/users/{memberId}", HttpStatusCode.NoContent, token: adminToken);
    Console.WriteLine("PASS: migration preserves old accounts/hashes/roles/history; registration, JWT 401/403, password changes/reset, phone changes, roles, filtering and admin safeguards.");
}
finally
{
    if (!app.HasExited) app.Kill(entireProcessTree: true);
    await app.WaitForExitAsync();
}
