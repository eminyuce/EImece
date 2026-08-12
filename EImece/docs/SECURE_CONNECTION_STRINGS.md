# Secure database connection strings

This project no longer stores real SQL credentials in source control (CWE-798 / CWE-259).

## Resolution order

1. Environment variable **`EIMECE_DB_CONNECTION_STRING`** (full connection string) — preferred for CI, IIS, and Azure
2. `connectionStrings` entry `EImeceDbConnection` in `Web.config` / `App.config` (or an external file via `configSource`)

If the value is missing or still contains placeholders (`YOUR_SERVER`, `YOUR_DATABASE`, `YOUR_USER`, `YOUR_PASSWORD`, `CHANGEME`, `REPLACE_ME`), startup fails with a clear `ConfigurationErrorsException`. There is no hard-coded credential fallback.

## TLS / TrustServerCertificate

Committed examples use:

```text
Encrypt=True;TrustServerCertificate=False;
```

- **Production:** keep `TrustServerCertificate=False`. Install a valid TLS certificate on SQL Server (or use Azure SQL, which already presents a trusted cert). Clients must trust the issuing CA.
- **Local SQL Server with a self-signed cert:** either trust that cert in the Windows certificate store, or (temporary local-only) set `TrustServerCertificate=True` in your **gitignored** local config / environment variable — never commit that setting for production.

## Local development

### Option A — Environment variable (recommended)

PowerShell:

```powershell
$env:EIMECE_DB_CONNECTION_STRING = "Data Source=localhost;Initial Catalog=EImece;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;"
```

CMD:

```cmd
set EIMECE_DB_CONNECTION_STRING=Data Source=localhost;Initial Catalog=EImece;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;
```

For a persistent user-level variable (new terminals):

```powershell
[System.Environment]::SetEnvironmentVariable(
  "EIMECE_DB_CONNECTION_STRING",
  "Data Source=localhost;Initial Catalog=EImece;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;",
  "User")
```

### Option B — Gitignored `ConnectionStrings.config`

1. Copy `EImece/ConnectionStrings.config.example` to `EImece/ConnectionStrings.config`
2. Put your real connection string in that file
3. In `Web.config`, replace the `<connectionStrings>...</connectionStrings>` block with:

```xml
<connectionStrings configSource="ConnectionStrings.config" />
```

`ConnectionStrings.config` is listed in `.gitignore` and must never be committed.

### Option C — Integrated Security in a local-only edit

You may replace the placeholder in `Web.config` with Integrated Security for your machine. Do not put SQL passwords in `Web.config`, and do not commit machine-specific secrets.

SQL auth example (prefer env var instead of committing):

```text
Data Source=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;User ID=...;Password=...;Encrypt=True;TrustServerCertificate=False;
```

### Tests

Same rules apply to `EImece.Tests/App.config`. Prefer `EIMECE_DB_CONNECTION_STRING`, or copy `EImece.Tests/ConnectionStrings.config.example` → `ConnectionStrings.config` and wire `configSource`.

## Production

**Do not** put plaintext SQL passwords into the committed `Web.config` or into `Web.Release.config` transforms. Release publishes should leave placeholders (or omit secrets) and inject credentials at deploy time.

### IIS (Windows Server)

1. Set a machine or app-pool environment variable `EIMECE_DB_CONNECTION_STRING`, **or**
2. Deploy a server-only `ConnectionStrings.config` next to `Web.config` (not from git) and use `configSource`, **or**
3. Set the connection string in IIS Manager → site → Connection Strings (overrides `Web.config` at runtime)

Prefer the app-pool identity with SQL **Integrated Security** when possible so no SQL password is stored.

IIS Manager → Application Pools → your pool → Advanced Settings → Identity.

To set an env var for the app pool (IIS 10+): Configuration Editor → `system.applicationHost/applicationPools` → `environmentVariables`, or set a system environment variable and recycle the pool.

### Azure App Service

Configuration → Application settings → New application setting:

| Name | Value |
|------|--------|
| `EIMECE_DB_CONNECTION_STRING` | full ADO.NET connection string |

Or use Connection strings in the Azure portal with type SQLAzure / SQLServer and map via your preferred config approach. Environment variable override remains the simplest path for this codebase.

For Azure SQL, use `Encrypt=True;TrustServerCertificate=False;` (Azure SQL certs are publicly trusted).

### Azure Key Vault (optional)

Store the secret in Key Vault and inject it as an App Setting / env var at deploy time (Key Vault reference, pipeline variable, or startup script). Do not bake the secret into the deployed `Web.config` in source control.

## Rotate the previously exposed password

The old credentials (`sqluser` / `sqluser` and any other values that appeared in git history) must be treated as compromised:

1. On SQL Server, create a new strong password (or better: switch to Integrated Security / a new least-privilege login).
2. `ALTER LOGIN [sqluser] WITH PASSWORD = '<new strong password>';` — or disable/drop the old login after cutting over.
3. Update `EIMECE_DB_CONNECTION_STRING` (or server-only config) everywhere the app runs.
4. Recycle IIS app pools / restart App Service so new values load.
5. Revoke any other places the old password was reused.
6. Optionally purge secrets from git history (`git filter-repo` / BFG) if the repository was or will be public; rotating the password is mandatory regardless.

## One-off migration tools (`DbMigration`)

Optional overrides:

- `EIMECE_DB_CONNECTION_STRING_PROD`
- `EIMECE_DB_CONNECTION_STRING_DEV`

If unset, those tools use `EIMECE_DB_CONNECTION_STRING` / configuration like the main app.
