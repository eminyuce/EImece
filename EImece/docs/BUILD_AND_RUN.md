# EImece — Build, Run, and Verification Guide

This document describes how to build the EImece solution (the C# equivalent of `mvn clean install`), run it locally or on IIS, and verify that everything is working.

## Solution overview

| Project | Type | Purpose |
|---------|------|---------|
| `Resources` | Class library | Localized strings and resources |
| `EImece.Domain` | Class library | Business logic, EF6, services, observability |
| `EImece` | ASP.NET MVC 5 web app | Main website and admin area |
| `EImece.Tests` | MSTest library | Unit and integration tests |
| `EImece.MyConsole` | Console app | One-off maintenance / migration utilities |

**Stack:** .NET Framework 4.8.1, ASP.NET MVC 5, Entity Framework 6, Microsoft.Extensions.DependencyInjection, SQL Server, IIS / IIS Express. Observability: NLog/Serilog + OpenTelemetry (see [OPENTELEMETRY.md](OPENTELEMETRY.md)).

> **Important:** The solution can be **compiled on Linux** for CI. The web application itself must **run on Windows** with IIS or IIS Express and a reachable SQL Server database.

---

## Prerequisites

### For building (Linux or Windows)

- [.NET SDK 8+](https://dotnet.microsoft.com/download) (used only as the MSBuild host)
- Python 3 (for `scripts/restore-packages.py`)
- `unzip` (Linux only, used during package restore)

The build script installs the .NET SDK automatically on Linux if it is missing.

### For running (Windows only)

- Windows 10/11 or Windows Server
- Visual Studio 2019/2022 with **ASP.NET and web development** workload  
  — or full IIS + .NET Framework 4.8.1
- **SQL Server** (Express, Developer, or full edition)
- IIS Express (included with Visual Studio) or IIS

---

## Step 1 — Build the solution

### Option A: Linux / CI (recommended for compile verification)

From the repository root:

```bash
cd EImece
chmod +x scripts/build.sh   # first time only
./scripts/build.sh
```

What the script does:

1. Ensures .NET SDK 8 is available
2. Downloads all NuGet packages listed in each `packages.config`
3. Runs `Clean,Build` on `EImece.sln` in **Release** configuration
4. Verifies that `EImece.dll` was produced

**Expected output files:**

```
Resources/bin/Release/Resources.dll
EImece.Domain/bin/Release/EImece.Domain.dll
EImece/bin/EImece.dll
EImece.Tests/bin/Release/EImece.Tests.dll
EImece.MyConsole/bin/Release/EImece.MyConsole.exe
```

### Option B: Manual build (Linux or Windows)

```bash
cd EImece
python3 scripts/restore-packages.py

export PATH="$HOME/.dotnet:$PATH"   # Linux; adjust on Windows
dotnet msbuild EImece.sln /t:Clean,Build /p:Configuration=Release
```

### Option C: Windows with Visual Studio

1. Open `EImece/EImece.sln` in Visual Studio.
2. **Build → Rebuild Solution** (or `Ctrl+Shift+B`).
3. Set configuration to **Release** or **Debug** as needed.

Or from a **Developer Command Prompt**:

```powershell
cd EImece
nuget restore EImece.sln
msbuild EImece.sln /t:Clean,Build /p:Configuration=Release
```

### Build troubleshooting

| Problem | Likely cause | Fix |
|---------|--------------|-----|
| Missing NuGet package | Packages not restored | Run `python3 scripts/restore-packages.py` |
| `EImece.dll` not found after build | Web project failed silently | Re-run with `/v:normal` and check errors |
| MSTest attributes not found | Old GAC reference | Ensure `MSTest.TestFramework` is in `EImece.Tests/packages.config` |
| COM reference errors on Linux | Windows-only dependency | Use `./scripts/build.sh`; unused COM refs are removed from `EImece.MyConsole` |

---

## Step 2 — Configure before first run

Edit `EImece/Web.config` before starting the application.

### Database connection (required)

**Do not put real usernames/passwords in `Web.config`.** Secrets are resolved at runtime:

1. Environment variable `EIMECE_DB_CONNECTION_STRING` (preferred), or
2. A gitignored `ConnectionStrings.config` via `configSource` (see example file)

Safe placeholders ship in `Web.config`. Startup fails if the connection string is missing or still a placeholder.

```xml
<connectionStrings>
  <add name="EImeceDbConnection"
       connectionString="Data Source=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Full local / IIS / Azure / TLS / password-rotation steps: [SECURE_CONNECTION_STRINGS.md](SECURE_CONNECTION_STRINGS.md).

The database schema must already exist (apply your usual EF migrations or restore a backup).

### Dummy / demo data (optional)

To populate a realistic small shop for manual testing (admin grids, storefront, orders, reports). Catalog values look production-like (brand/product names, Turkish customers, EIMC order/coupon codes); cleanup uses technical markers rather than `SEED …` name prefixes.

1. Ensure the schema exists and the app can connect.
2. Run `EImece/SqlScripts/SeedDummyData.sql` in SSMS, or:

```powershell
cd EImece/SqlScripts
.\RunSeedDummyData.ps1 -ConnectionString "Server=.;Database=EImece;Trusted_Connection=True;TrustServerCertificate=True;"
# Optional: larger catalog/orders only (menus, slides, settings stay small)
.\RunSeedDummyData.ps1 -ConnectionString "..." -Scale 2
```

Default volumes (`@Scale = 1`): ~12 menus, ~6 homepage slides, ~20 brands, ~25 categories, ~150 products, ~30 stories, ~40 customers, ~100 orders — not thousands of rows in every table.

- Shared seed credential for all seed users: concatenate `Test` + `123` + `!` (local/test only)
- Known logins: `admin@eimece.test` (Admin), `editor@eimece.test` (NormalUser), `customer1@eimece.test` (Customer)
- Cleanup: `CleanupDummyData.sql`, or re-run the seed script (`@CleanupFirst = 1` by default)
- Tune bulk catalog/order size with `@Scale` / `-Scale`; edit individual `@Seed*` counts in the SQL for finer control
- To change the realistic catalog copy, edit `_gen_realistic_seed.py` and re-run it to regenerate `SeedDummyData.sql`
- After SQL seed, `RunSeedDummyData.ps1` generates JPEG placeholders under `media/images` (and `media/images/thumbs`) via `GenerateSeedImages.ps1`. IIS default:

```powershell
.\RunSeedDummyData.ps1 -ConnectionString "..." -MediaRoot "C:\inetpub\wwwroot\Eimece\media\images"
# Images only (SQL already seeded):
.\GenerateSeedImages.ps1 -MediaRoot "C:\inetpub\wwwroot\Eimece\media\images" -MarkExisting
```

File uploads use `~/media/images/` (`Constants.ServerMapPath` / `AppConfig.StorageRoot`). Seed `FileStorage.FileName` values (`product-00001.jpg`, …) must exist there for admin/storefront thumbnails.

### Application settings (review)

| Key | Purpose |
|-----|---------|
| `domain` | Public site domain used in links and emails |
| `SiteStatus` | `live` or maintenance modes |
| `Quartz_Scheduler_IsEnabled` | Background job scheduler (`False` for local dev) |
| `IyzicoBaseUrl` | Payment API base URL (sandbox or production) |
| `EnableRequestLogging` | Structured request logging (`true` recommended) |
| `EnableMetrics` | In-process metrics collection |

File uploads use `App_Data` under the web project (`StorageRoot` is resolved at runtime via `HostingEnvironment.MapPath`).

### Test project configuration

For unit tests, set `EIMECE_DB_CONNECTION_STRING` or use a gitignored `EImece.Tests/ConnectionStrings.config` (copy from `ConnectionStrings.config.example`). Do not commit real credentials in `App.config`.

---

## Step 3 — Run the application

### Option A: Visual Studio + IIS Express (local development)

1. Open `EImece.sln` in Visual Studio.
2. Set **EImece** as the startup project (right-click → *Set as Startup Project*).
3. Press **F5** (Debug) or **Ctrl+F5** (Run without debugging).

Default IIS Express URL (from the project file):

```
http://localhost:31544
```

Visual Studio may assign a different port; check the browser address bar or project properties → *Web* → *Project Url*.

### Option B: IIS (staging / production-like)

1. Install **IIS** and **ASP.NET 4.8** features on Windows.
2. In IIS Manager, create a new **Application Pool**:
   - .NET CLR version: **v4.0**
   - Managed pipeline mode: **Integrated**
3. Create a **Website** or **Application** pointing to the folder:
   ```
   EImece/EImece/
   ```
   (the folder containing `Web.config`, not the `bin` folder alone)
4. Grant the app pool identity **read/write** access to:
   - `EImece/App_Data/`
   - `EImece/App_Data/logs/`
   - Any media/upload directories your deployment uses

   Exact `icacls` commands for the published IIS site (`C:\inetpub\wwwroot\Eimece`) are documented in [IIS_APP_POOL_PERMISSIONS.md](IIS_APP_POOL_PERMISSIONS.md).
5. Browse to the site URL configured in IIS.

### Option C: Console utility (optional)

`EImece.MyConsole` is a developer utility, not the web app:

```powershell
cd EImece\EImece.MyConsole\bin\Release
.\EImece.MyConsole.exe
```

Review `Program.cs` before running — it executes maintenance tasks against the configured database.

---

## Step 4 — Verify the application is working

Use this checklist after the site is running.

### 4.1 Health endpoint (fastest signal)

Both URLs are equivalent:

```
GET http://localhost:31544/health
GET http://localhost:31544/healthz
```

**Healthy response:** HTTP **200** with JSON like:

```json
{
  "Status": "UP",
  "Components": {
    "allHealthChecks": {
      "Status": "UP",
      "Details": {
        "sqlServer": "connection alive",
        "externalApi": "404 reachable",
        "fileStorage": "read/write available",
        "backgroundServices": "scheduler disabled"
      }
    }
  }
}
```

**Unhealthy response:** HTTP **503** with `"Status": "DOWN"`. Check the `Details` map — `sqlServer` failures usually mean a bad connection string or unreachable database.

PowerShell example:

```powershell
Invoke-RestMethod http://localhost:31544/health | ConvertTo-Json -Depth 5
```

curl example:

```bash
curl -s http://localhost:31544/health | python3 -m json.tool
```

### 4.2 Browser smoke tests

| Check | URL | Expected |
|-------|-----|----------|
| Home page | `/` | Page renders, no HTTP 500 |
| Admin area | `/Admin` (or your admin route) | Login page loads |
| Static content | CSS / images on homepage | No 404 errors in DevTools |

### 4.3 Logs

After browsing a few pages, confirm log files are created:

```
EImece/App_Data/logs/EImeceLog.log      # plain text
EImece/App_Data/logs/EImeceLog.json     # structured JSON (correlation ID, path, etc.)
```

If these files are missing, check folder permissions and that `EnableRequestLogging` is `true` in `Web.config`.

### 4.4 Security headers

In browser DevTools → **Network** → select any response → **Headers**. Responses should include headers added by `SecurityHeadersHttpModule`, for example:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: SAMEORIGIN`

### 4.5 Metrics (admin only)

```
GET /metrics
```

Requires an authenticated **Administrator** session. Log in to the admin area first, then open `/metrics` in the same browser session. Returns JSON snapshots of in-process counters.

---

## Step 5 — Run unit tests

Tests compile on Linux but **must be executed on Windows** with Visual Studio and a configured test database.

1. Update `EImece.Tests/App.config` connection strings.
2. In Visual Studio: **Test → Test Explorer**.
3. Click **Run All Tests**.

**Suggested order for first run:**

1. `ImageUtilitiesTests` — fewer external dependencies
2. `HomeControllerTest` — integration tests; requires SQL Server and full MVC pipeline

A green Test Explorer result confirms regression coverage on the configured environment. Red tests often indicate database connectivity or missing test data, not necessarily a bad build.

Command-line alternative (Windows, with Visual Studio Build Tools):

```powershell
vstest.console.exe EImece.Tests\bin\Release\EImece.Tests.dll
```

---

## Verification decision tree

```
./scripts/build.sh succeeds?
│
├─ NO  → Fix compile errors (see Build troubleshooting)
│
└─ YES → Start app on Windows (IIS Express or IIS)
          │
          ├─ GET /health returns 200 "UP"?
          │   ├─ NO  → Fix SQL connection, DB schema, IIS permissions
          │   └─ YES → Browse homepage and admin login
          │             │
          │             └─ Run unit tests in Visual Studio
          │                   ├─ Pass → Application is working
          │                   └─ Fail → Check test DB config and test data
```

---

## What each verification level proves

| Level | Command / action | Proves |
|-------|------------------|--------|
| Compile | `./scripts/build.sh` | Source code and references are valid |
| Health | `GET /health` | App starts, DI works, SQL reachable, file storage writable |
| Smoke | Browse `/` and admin | Core UI and routing work |
| Logs | Files in `App_Data/logs/` | Observability pipeline is active |
| Tests | Visual Studio Test Explorer | Business logic and controllers behave as expected |

---

## Common runtime errors

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| HTTP 500 on every page | SQL connection failure | Verify `EImeceDbConnection` and that the database exists |
| `/health` returns 503, `sqlServer` down | Wrong server name or credentials | Test connection in SSMS with the same connection string |
| `/health` returns 503, `fileStorage` down | IIS identity cannot write to `App_Data` | Grant modify permission on `App_Data` to the app pool identity |
| Blank page, no error | `customErrors` hiding detail | Temporarily set `<customErrors mode="Off"/>` in dev only |
| Tests fail, site works | Test `App.config` points elsewhere | Align test connection strings with a dedicated test database |

---

## Quick reference

```bash
# Build (Linux / CI)
cd EImece && ./scripts/build.sh

# Build (Windows — Developer Command Prompt)
cd EImece
nuget restore EImece.sln
msbuild EImece.sln /t:Clean,Build /p:Configuration=Release

# Run (Windows — Visual Studio)
# Open EImece.sln → F5 → browse http://localhost:31544

# Verify
curl http://localhost:31544/health
```

---

## Related files

| File | Purpose |
|------|---------|
| `scripts/build.sh` | Automated clean + restore + build |
| `scripts/restore-packages.py` | Downloads NuGet packages from `packages.config` |
| `Directory.Build.props` | Cross-platform net472 reference assemblies and web targets |
| `EImece/Web.config` | Runtime configuration (DB, app settings, modules) |
| `EImece/NLog.config` | Logging targets and layouts |
| `EImece.Tests/App.config` | Test database configuration |
