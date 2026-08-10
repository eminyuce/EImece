# EImece Production Deployment

This document describes the GitHub Actions CI/CD pipeline for the **EImece** ASP.NET MVC 5.3 / .NET Framework **4.8.1** web application.

> Production is **never** deployed automatically on `git push`.  
> You must trigger **Deploy Production** manually after reviewing the build artifact.

---

## Prerequisites

### Local / server

| Requirement | Notes |
|-------------|--------|
| Windows Server / IIS | App pool: .NET CLR v4.0, Integrated |
| .NET Framework 4.8.1 | Runtime on the IIS host |
| SQL Server | Schema already applied (pipeline does **not** migrate DB) |
| FTP or FTPS access | To the IIS site content root |
| App pool write access | On `media/` (uploads + logs) — see `EImece/docs/IIS_APP_POOL_PERMISSIONS.md` |

### GitHub

| Requirement | Notes |
|-------------|--------|
| Repository Secrets | See [Required GitHub Secrets](#required-github-secrets) |
| Environment `production` | Used by the deploy job (create under Settings → Environments) |
| Actions enabled | Workflows must be allowed for the repository |

---

## Repository facts (discovered)

| Item | Path / value |
|------|----------------|
| Solution | `EImece/EImece.sln` |
| Web project (ASP.NET MVC Web Application) | `EImece/EImece/EImece.csproj` (`ProjectTypeGuids` includes `{349c5851-…}`) |
| Target framework | `v4.8.1` |
| Configurations | `Debug`, `Release` (pipeline uses **Release**) |
| Package style | `packages.config` + solution `packages/` folder |
| Existing local publish profile | `EImece/EImece/Properties/PublishProfiles/FolderProfile.pubxml` → `C:\inetpub\wwwroot\Eimece` |
| CI publish profile | `EImece/EImece/Properties/PublishProfiles/GitHubActions.pubxml` |
| Test project | `EImece/EImece.Tests` (MSTest) |
| Health endpoints | `GET /health` and `GET /healthz` (`HealthController`) |
| Playwright suite | `Playwright/` — targets local IIS `http://localhost:81`, **not** used in this production pipeline |
| Existing Linux compile helper | `EImece/scripts/build.sh` (compile verification only; **not** used for IIS publish) |

---

## GitHub Actions workflow

**File:** [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml)

**Trigger:** `workflow_dispatch` only (Actions tab → **Deploy Production** → **Run workflow**).

### Jobs

```
Checkout
   ↓
Setup MSBuild + NuGet
   ↓
Restore NuGet packages
   ↓
Build solution (Release / MSBuild)
   ↓
Run MSTest unit tests (Helpers + Infrastructure)
   ↓
Publish web app (MSBuild FileSystem / GitHubActions profile)
   ↓
Upload GitHub Actions artifact (eimece-production-publish)
   ↓
[only if deploy_to_production=true]
Download artifact → FTPS upload → GET /health smoke test
```

### Workflow inputs

| Input | Default | Purpose |
|-------|---------|---------|
| `deploy_to_production` | `false` | When `true`, runs the FTPS deploy job after a successful publish |
| `use_ftps` | `true` | Prefer FTPS (explicit TLS). Turn off only if the host rejects FTPS |
| `skip_tests` | `false` | Skip MSTest (not recommended) |

### First validation run (recommended)

1. Configure secrets (at least leave deploy off).
2. Run the workflow with **`deploy_to_production = false`**.
3. Confirm Build / Tests / Publish / Artifact all succeed.
4. Download the `eimece-production-publish` artifact and inspect it.
5. Only then run again with **`deploy_to_production = true`**.

---

## Required GitHub Secrets

Configure under **Settings → Secrets and variables → Actions**.

### Required for FTPS deployment

| Secret | Example / format | Description |
|--------|------------------|-------------|
| `FTP_HOST` | `ftp.example.com` | Hostname only (no `ftp://`) |
| `FTP_USERNAME` | `deploy-user` | FTP username |
| `FTP_PASSWORD` | `(secret)` | FTP password |
| `FTP_PATH` | `/` or `/site/wwwroot` | Remote directory that maps to the IIS site root |

### Optional

| Secret | Description |
|--------|-------------|
| `FTP_PORT` | Port number (default `21` if omitted) |
| `PRODUCTION_BASE_URL` | Public site origin for smoke tests, e.g. `https://www.example.com` (no trailing slash required). Enables `GET {BASE}/health` after deploy. |

### Not used by the pipeline (configure on the server)

These must **not** be committed. Prefer IIS / machine environment variables or a server-only `ConnectionStrings.config` (see `EImece/docs/SECURE_CONNECTION_STRINGS.md`):

- `EIMECE_DB_CONNECTION_STRING`
- `EIMECE_ENCRYPTION_KEY`
- Iyzico keys / Application Insights / OTLP / SMTP credentials

The workflow never prints secret values.

---

## Build process

On `windows-latest`:

1. `microsoft/setup-msbuild@v2` + `nuget/setup-nuget@v2`
2. `nuget restore EImece/EImece.sln`
3. `msbuild EImece/EImece.sln /t:Clean,Build /p:Configuration=Release /p:DeployOnBuild=false`

This matches the project type: classic ASP.NET Web Application with `packages.config`.  
It does **not** use `dotnet publish` and does **not** retarget the framework.

---

## Publish process

MSBuild Web Publishing (FileSystem) with profile **`GitHubActions`**:

```text
msbuild EImece/EImece/EImece.csproj
  /p:Configuration=Release
  /p:DeployOnBuild=true
  /p:PublishProfile=GitHubActions
  /p:PublishUrl=<workspace>\artifacts\publish\
  /p:WebPublishMethod=FileSystem
  /p:DeleteExistingFiles=false
```

**Output:** `artifacts/publish/` — IIS-ready content (`Web.config`, `bin\`, Views, Content, Areas, …).

`DeleteExistingFiles=false` is intentional (same convention as `FolderProfile.pubxml`) so publish/deploy never wipes persistent media.

Release transforms from `Web.Release.config` are applied by the Web Publishing Pipeline during this publish (for example `SiteStatus=live`, `customErrors=RemoteOnly`, debug attribute removed). Connection strings are **not** injected from source transforms — keep production DB credentials on the server / env vars.

---

## FTP / FTPS deployment

**Script:** [`EImece/scripts/deploy-ftps.ps1`](EImece/scripts/deploy-ftps.ps1)

Behavior:

- Uploads from the published artifact (not the git working tree)
- Uses **FTPS** by default (`EnableSsl` / explicit TLS)
- **Never deletes** remote files or directories (no mirror purge)
- Uploads **only** these folders (allowlist):
  - `bin/`
  - `Views/`
  - `Content/`
  - `Scripts/`
- **Never overwrites** production `Web.config` or anything under `media/`
- Fails the job if upload fails, required folders are missing, or zero files are uploaded

Credentials come exclusively from GitHub Actions Secrets.

Everything else in the publish artifact (for example `Areas/`, `fonts/`, `App_Data/`, `Global.asax`, `NLog.config`, root static files) is kept in the downloadable GitHub artifact for inspection, but is **not** FTPS-uploaded. Manage those on the server separately when needed.

---

## Persistent / protected server files

These must remain as already configured on the production server:

| Path | Contents | Deploy behavior |
|------|----------|-----------------|
| `Web.config` | Production runtime config / connection wiring | **Never uploaded** |
| `media/` (entire tree) | Uploads (`images/`), logs (`logs/`), media rules | **Never uploaded** |
| `ConnectionStrings.config` | Server-only DB config (`configSource`) | **Never uploaded** |

Grant the IIS app pool modify rights on `media/` (see `EImece/docs/IIS_APP_POOL_PERMISSIONS.md`).

---

## Configuration and secrets handling

| Concern | Approach |
|---------|----------|
| SQL connection | `EIMECE_DB_CONNECTION_STRING` on the server, or server-only `ConnectionStrings.config` |
| Encryption key | `EIMECE_ENCRYPTION_KEY` |
| Iyzico / AI / OTLP / SMTP | Server env vars or IIS settings — keep empty/placeholder in git |
| Web.config transforms | `Web.Release.config` for non-secret Release settings |
| Pipeline credentials | GitHub Actions Secrets only |

Do **not** put production passwords into `Web.Release.config` or commit them into the repository.

---

## Testing and validation

### Before deploy (every workflow run)

| Check | What runs |
|-------|-----------|
| Restore / Build | Full solution Release build |
| Unit tests | MSTest filter: `EImece.Tests.Helpers` + `EImece.Tests.Infrastructure` |
| Publish validation | Asserts `Web.config` and `bin\EImece.dll` exist in the artifact |

**Why not all tests?**  
`EImece.Tests.Controllers` (e.g. `HomeControllerTest`) requires a live SQL Server and external SMTP; several methods are integration tests unsuitable for a secret-free build agent. They remain available locally / on a dedicated test host.

**Why not Playwright?**  
`Playwright/` is configured for `http://localhost:81` against a local IIS site. It is not wired as a production gate and is not executed by this workflow.

### After deploy (when `deploy_to_production=true`)

If `PRODUCTION_BASE_URL` is set:

```http
GET {PRODUCTION_BASE_URL}/health
```

Expect HTTP **200** and JSON containing `"status":"UP"` (anonymous payload). Retries a few times to allow app-pool warm-up.

If `PRODUCTION_BASE_URL` is unset, deploy still runs but the smoke test is skipped with a warning.

---

## Status reporting

Each run prints a summary similar to:

```text
Build:       PASS/FAIL
Tests:       PASS/FAIL/SKIPPED
Publish:     PASS/FAIL
Artifact:    created/not created
Deployment:  PASS/FAIL/NOT RUN
Smoke Test:  PASS/FAIL/SKIPPED/NOT RUN
```

The same table is written to the GitHub Actions job summary.

---

## How to manually trigger production deployment

1. Open the repository on GitHub → **Actions**.
2. Select **Deploy Production**.
3. Click **Run workflow**.
4. Set:
   - `deploy_to_production` → **true**
   - `use_ftps` → **true** (unless your host requires plain FTP)
   - `skip_tests` → **false**
5. Run the workflow and monitor both jobs.
6. Confirm smoke test / browse the site.

---

## Rollback procedure

The pipeline retains the publish artifact (`eimece-production-publish`) for **30 days**.

### Redeploy a previous good artifact

1. Open the GitHub Actions run that produced the last known-good build.
2. Download the `eimece-production-publish` artifact (zip).
3. Extract it locally.
4. Upload that folder to the same `FTP_PATH` using the same non-destructive approach:
   - Re-run a known-good workflow commit with `deploy_to_production=true`, **or**
   - Manually FTPS-upload the extracted artifact with `EImece/scripts/deploy-ftps.ps1` from a trusted machine.

Example local rollback upload (PowerShell):

```powershell
.\EImece\scripts\deploy-ftps.ps1 `
  -LocalPath "C:\temp\eimece-production-publish" `
  -FtpHost "YOUR_HOST" `
  -FtpUsername "YOUR_USER" `
  -FtpPassword "YOUR_PASSWORD" `
  -FtpPath "/YOUR/PATH" `
  -UseFtps:$true
```

Because remote delete is disabled and the FTPS allowlist is limited to `bin/`, `Views/`, `Content/`, and `Scripts/`, rollback overwrites only those folders and **does not** modify `Web.config` or `media/`.

There is no automatic database rollback (schema changes are out of scope for this pipeline).

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| NuGet restore fails | Network / feed issue on runner | Re-run job; confirm nuget.org is reachable |
| MSBuild compile error | Code/package problem | Open `EImece/EImece.sln` in Visual Studio; fix locally |
| `vstest.console.exe not found` | Unexpected runner image | Confirm job uses `windows-latest` |
| Unit tests fail | Regression in Helpers/Infrastructure | Fix tests/code; do not skip unless diagnosing publish-only issues |
| Publish missing `bin\EImece.dll` | Web publish targets failed | Check MSBuild log; confirm `build-tools/MSBuild.Microsoft.VisualStudio.Web.targets.*` present |
| Deploy job skipped | `deploy_to_production` left false | Re-run with the input enabled |
| Missing FTP secrets | Secrets not configured | Add `FTP_HOST`, `FTP_USERNAME`, `FTP_PASSWORD`, `FTP_PATH` |
| FTPS authentication / TLS errors | Host requires plain FTP or different port | Try `use_ftps=false` or set `FTP_PORT`; confirm host FTPS support |
| Smoke test skipped | `PRODUCTION_BASE_URL` unset | Add the secret and re-run deploy |
| Smoke test 503 / DOWN | DB or `media` permissions | Fix server env connection string / IIS ACLs; see BUILD_AND_RUN.md |
| Missing images after deploy | Expected — `media/` is never uploaded | Production media stays on the server |
| `Web.config` unexpectedly changed | Should not happen with this pipeline | Confirm deploy used `deploy-ftps.ps1` allowlist |
| Admin / Areas views not updated | `Areas/` is outside the FTPS allowlist | Update `Areas/` on the server manually, or extend the allowlist if desired |

---

## Post-deployment validation checklist

1. `GET /health` → 200 and `"status":"UP"`
2. Homepage `/` renders (no HTTP 500)
3. Admin login page loads
4. Existing media under `/media/images/...` still present
5. New log lines appear under `media/logs/` after traffic
6. Payments / email only after confirming production secrets on the server (not from git)

---

## Safety constraints (pipeline design)

The workflow intentionally does **not**:

- Deploy on every push
- Use Linux to publish the .NET Framework web app
- Call `dotnet publish` for the MVC project
- Change target framework or migrate to ASP.NET Core
- Run EF migrations or destructive SQL
- Delete remote files over FTP
- Overwrite `Web.config` or anything under `media/`
- Upload folders outside `bin/`, `Views/`, `Content/`, `Scripts/`
- Echo credentials to logs
- Execute Playwright against production

---

## Manual steps you must perform

1. Create GitHub Actions secrets listed above.
2. Create the GitHub Environment named **`production`** (Settings → Environments).
3. Ensure IIS + SQL + server-side secrets (`EIMECE_DB_CONNECTION_STRING`, etc.) are already configured on the host.
4. Run the workflow once with **`deploy_to_production=false`** and review the artifact.
5. Run again with **`deploy_to_production=true`** when ready.
6. Optionally set `PRODUCTION_BASE_URL` for automated smoke tests.

---

## Related documentation

- [`EImece/docs/BUILD_AND_RUN.md`](EImece/docs/BUILD_AND_RUN.md) — build, IIS, health checks
- [`EImece/docs/SECURE_CONNECTION_STRINGS.md`](EImece/docs/SECURE_CONNECTION_STRINGS.md) — DB secret handling
- [`EImece/docs/IIS_APP_POOL_PERMISSIONS.md`](EImece/docs/IIS_APP_POOL_PERMISSIONS.md) — `media/` ACLs
- [`README.md`](README.md) — project overview
