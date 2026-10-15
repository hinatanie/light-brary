# Session integration checks

Uses the existing Identity / EF Core / Npgsql dependencies and runs the real API.
Covers token revocation after password changes/reset, role assignment/removal/rename,
multiple sessions, concurrent role rename/assignment, lockout and deletion. Also verifies role-change no-ops and profile
edits preserve sessions, and public login/register remain accessible with stale tokens.

Run against a **new, disposable** PostgreSQL database whose name starts with
`identity_checks_` and whose public schema is empty. The checks apply the current
migrations and create test data only in that database; they do not read the application's
connection string. No test cleanup is performed against an existing database.

```bash
docker run --detach --rm --name light-brary-session-checks \
  --env POSTGRES_HOST_AUTH_METHOD=trust \
  --env POSTGRES_DB=identity_checks_sessions \
  --publish 127.0.0.1::5432 postgres:17-alpine
docker port light-brary-session-checks 5432
```

Use the reported port:

```bash
IDENTITY_TEST_CONNECTION='Host=127.0.0.1;Port=<reported-port>;Database=identity_checks_sessions;Username=postgres' \
  dotnet run --project back_end/tests/SessionChecks/SessionChecks.csproj
```

The API uses port 5299 by default; override with `IDENTITY_TEST_HTTP_PORT` if needed.
Use a fresh database for each run. After testing, stop the disposable container:

```bash
docker stop light-brary-session-checks
```

# Session validation

`ValidateUserMiddleware` runs between authentication and authorization for endpoints
with `[Authorize]`, except `[AllowAnonymous]`. It rejects invalid user IDs, deleted
users, mismatched security stamps and locked accounts with HTTP 401 using the existing
JSON exception handler. Missing/invalid tokens remain handled by ASP.NET Core authorization.

Role assignment/removal and role renaming update affected users' security stamps in the
same database transaction. Identity already updates stamps on password changes/reset.
Role mutations lock the role row before reading membership so concurrent assignments
cannot escape a rename's invalidation or a deletion's membership guard.
The next protected request using an old token is rejected; the client must log in again.
All devices using the old stamp are affected. Requests already past validation can finish.
Direct changes to roles outside these application services must also update affected
users' stamps. Each authenticated request to a protected endpoint queries the user in
the database, so lockout/deletion and stamp changes take effect without a cache delay.
