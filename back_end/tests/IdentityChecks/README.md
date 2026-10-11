# Identity integration checks

Uses the project's existing .NET / EF Core / Npgsql libraries without adding test dependencies.
The checks migrate a legacy account, then run the real API to exercise registration, login,
JWT authorization, password changes/reset, phone changes, role management, user filtering,
and protection of borrow history and the last administrator. They also check that a migration
rejects an existing name longer than the new limit instead of truncating it.

Run from the repository root against a **new, disposable** PostgreSQL database. Its name must
start with `identity_checks_`, and its public schema must be empty. The checks create data and
apply migrations only in that database. They never read the application's connection string.

For example, start an isolated local PostgreSQL container:

```bash
docker run --detach --rm --name light-brary-identity-checks \
  --env POSTGRES_HOST_AUTH_METHOD=trust \
  --env POSTGRES_DB=identity_checks_main \
  --publish 127.0.0.1::5432 postgres:17-alpine
docker port light-brary-identity-checks 5432
```

Use the reported port in the test connection string:

```bash
IDENTITY_TEST_CONNECTION='Host=127.0.0.1;Port=<reported-port>;Database=identity_checks_main;Username=postgres' \
  dotnet run --project back_end/tests/IdentityChecks/IdentityChecks.csproj
```

The API uses port 5199 by default; set `IDENTITY_TEST_HTTP_PORT` if needed.
Stop the container after testing:

```bash
docker stop light-brary-identity-checks
```

# Application configuration

For a new application database with no users, provide `Seed:AdminPassword` through .NET User
Secrets or the environment variable `Seed__AdminPassword`. Do not put passwords in source code.
`Seed:AdminPhone` / `Seed__AdminPhone` is optional and defaults to `0999999999`.
Existing users keep their IDs, password hashes, role assignments and borrow history.

The application applies migrations on startup, as before. Review the new migrations first:
`UseAspNetCoreIdentity` initializes Identity fields; `SyncEntityConstraints` applies the current
entity limits for books/categories and whole-dong fine amounts. Existing values exceeding the
new limits cause migration failure so they can be corrected explicitly. Legacy null addresses
become empty strings; users must provide an address when updating their profile.
