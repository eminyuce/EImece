# EImece Scripts Guide

Central reference for every automation script in the repository. All paths are relative to the repository root `EImece/`.

## Quick index

| Script | Purpose | Typical invocation |
|--------|---------|-------------------|
| `scripts/build.sh` | Linux CI build (SDK 8 + restore + `msbuild Release`) | `cd EImece && ./scripts/build.sh` |
| `scripts/restore-packages.py` | NuGet restore from `packages.config` | `python3 scripts/restore-packages.py` |
| `scripts/deploy-local-iis.ps1` | Publish to `C:\inetpub\wwwroot\Eimece` | `.\scripts\deploy-local-iis.ps1` |
| `scripts/deploy-ftps.ps1` | **Legacy** FTPS deploy — replaced by MSDeploy, kept for reference | `.\scripts\deploy-ftps.ps1 -Server ftps.example.com` |
| `scripts/verify-iis.ps1` | Probe `http://localhost:81/health`, sitemap, admin login | `.\scripts\verify-iis.ps1` |
| `scripts/verify-full-deployment.ps1` | Full smoke: health + sitemap 276 URLs + grids | `.\scripts\verify-full-deployment.ps1` |
| `scripts/verify-full-regression.ps1` | Storefront + admin + cart + upload regression | `.\scripts\verify-full-regression.ps1` |
| `scripts/test-*.ps1` | Focused probes (admin health, sitemap, products, stories) | `.\scripts\test-sitemap-urls.ps1` |
| `scripts/playwright/e2e-regression.mjs` | Playwright E2E (Chrome) for storefront + admin | `cd scripts/playwright && npx playwright test` |
| `EImece/SqlScripts/SeedDummyData.sql` | Realistic Turkish demo data (Lang=1) | `sqlcmd -f 65001 -i SeedDummyData.sql` or `RunSeedDummyData.ps1 -SeedDatabase` |
| `EImece/SqlScripts/SeedDummyData_EN_Real.sql` | Real English demo data (Lang=2) — generated from `_gen_realistic_seed.py` | `sqlcmd -f 65001 -i SeedDummyData_EN_Real.sql` |
| `EImece/SqlScripts/RunSeedDummyData.ps1` | Orchestrator for SQL + images | `.\RunSeedDummyData.ps1 -SeedDatabase` |
| `EImece/SqlScripts/GenerateSeedImages.ps1` | Create JPEGs for `FileStorages` | `.\GenerateSeedImages.ps1 -MediaRoot C:\inetpub\wwwroot\Eimece\media\images` |
| `EImece/SqlScripts/CleanupDummyData.sql` | Delete all `SEED` rows (both languages) | `sqlcmd -f 65001 -i CleanupDummyData.sql` |
| `EImece/SqlScripts/AddPerformanceIndexes.sql` | Missing index report + DDL | `sqlcmd -i AddPerformanceIndexes.sql` |
| `EImece/SqlScripts/MonitorQueryExecutionPlans.sql` | Live query plan monitor | `sqlcmd -i MonitorQueryExecutionPlans.sql` |
| `EImece/SqlScripts/_gen_realistic_seed.py` | Generator for `SeedDummyData.sql` | `python _gen_realistic_seed.py` |
| `EImece/SqlScripts/GenerateEnglishSeed.py` | Generates `SeedDummyData_EN_Real.sql` with real English | `python GenerateEnglishSeed.py` |

## 1. Build & restore

### `scripts/build.sh` (Linux CI)
Ensures .NET SDK 8, runs `restore-packages.py`, then `dotnet msbuild EImece.sln /t:Clean,Build /p:Configuration=Release`. Verifies `EImece.dll`, `EImece.Domain.dll`, `EImece.Tests.dll` exist. Exit 0 = green.

### `scripts/restore-packages.py`
Parses every `packages.config`, downloads from nuget.org, unzips to `packages/`. Idempotent. Required before `msbuild` on Linux where `nuget.exe` is not available.

## 2. Deploy

### `scripts/deploy-local-iis.ps1`
Publishes `EImece` to `C:\inetpub\wwwroot\Eimece` via `msbuild /t:Publish /p:PublishProfile=FolderProfile`, fixes `media/` ACL (`icacls "IIS AppPool\Eimece":(OI)(CI)M`), recycles app pool. Requires admin PowerShell.

### `scripts/deploy-ftps.ps1`
Legacy FTPS uploader for old hosting. **Do not use for new deploys** — `ENTERPRISE_ARCHITECTURE_REVIEW.md` marks it for replacement by `MSDeploy`/`DbUp`. Kept only to document the previous process.

## 3. Verification & health

All verification scripts share the same DB connection resolution as the app (`EIMECE_DB_CONNECTION_STRING` env → `C:\inetpub\wwwroot\ConnectionStrings.config` → `Web.config`).

| Script | What it checks |
|--------|----------------|
| `verify-iis.ps1` | `GET /health` → `Status:UP`, `GET /` 200, `GET /admin` 302→200 with bypass, `GET /sitemap.xml` count |
| `verify-full-deployment.ps1` | Above + every `sitemap.xml` `<loc>` returns 200 (276 TR, 551 bilingual), admin grids (`/admin/products` etc) |
| `verify-full-regression.ps1` | Storefront (desktop+mobile), admin, auth, cart→checkout, AJAX, uploads, reports, error pages |
| `test-sitemap-urls.ps1` | Loops `sitemap.xml` URLs, reports `404/500` |
| `test-admin-health.ps1` | Admin login + health with `BypassAdminAuth`/`SiteStatus` handling |
| `test-product-badges.mjs` | Checks `HasDiscount/IsOnSale` badge rendering |

**Example — full bilingual sitemap check:**
```powershell
$xml=[xml](curl -s http://localhost:81/sitemap.xml)
$urls=$xml.urlset.url | % {$_.loc}
$urls | % { $c=curl -s -o NUL -w "%{http_code}" $_; if($c -ne "200"){Write-Host "BAD $_ $c"} }
# TR only: 276, TR+EN: 551
```

### Playwright E2E

`scripts/playwright/e2e-regression.mjs` (Playwright 1.62.1, `node_modules` committed for CI). Covers:

- Storefront: home, category `c/pc/{seo}`, product `p/{cat}/{seo}`, cart, checkout, search
- Admin: login `account/adminlogin`, grids, `SaveOrEdit`, file upload, `ClearCache`
- Mobile viewport `375x812` + desktop `1280x720`
- Sitemap warm-up and 404 checks

```bash
cd EImece/scripts/playwright
npm ci
npx playwright test --project=chromium
npx playwright show-report
```

`playwright.config.js` sets `baseURL: http://localhost:81`, `webServer` disabled (expects IIS already running).

## 4. SQL — seed, images, maintenance

All `SqlScripts/*.sql` are UTF-8, **must be executed with `-f 65001` (UTF-8) and `-I` (quoted identifier)** for Turkish characters:

```powershell
sqlcmd -S YUCE\SQLEXPRESS -d yuva8905_yuvadan -U sqluser -P sqluser -C -I -f 65001 -i SeedDummyData.sql
```

### `SeedDummyData.sql` (TR) and `SeedDummyData_EN_Real.sql` (EN)

- **TR:** `@Lang=1`, `@Scale=1` → 150 products, 25 categories, 20 brands, 30 stories, 40 customers, 100 orders, 594 FileStorages. Turkish names/descriptions from `_gen_realistic_seed.py`.
- **EN:** `@Lang=2`, same volumes but real English names (`BRANDS_EN`, `CATEGORIES_EN`, `PRODUCTS_EN` — e.g., `Wireless Bluetooth Headset Pro`, `Electronics`, `Fashion & Apparel`). Generated via `GenerateEnglishSeed.py` → `SeedDummyData_EN_Real.sql` (patched for `Lang` filter on `MIN/COUNT`, `Coupon`/`OrderNumber` `-EN` suffix, `AspNetUsers` guard).
- Both use technical markers for idempotent cleanup: `AddUserId='SEED'`, `FileUrl='/media/seed/%'`, `Email LIKE '%@eimece.test'`, `Code LIKE 'EIMC-%'`, `Position 900000`.

**Bilingual setup (current production):**
```powershell
# 1. Clean slate (optional)
sqlcmd -C -I -f 65001 -i CleanupDummyData.sql
# 2. TR
sqlcmd -C -I -f 65001 -i SeedDummyData.sql
# 3. EN (real English)
sqlcmd -C -I -f 65001 -i SeedDummyData_EN_Real.sql
# Or orchestrated:
.\RunSeedDummyData.ps1 -ConnectionString "Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;" -Scale 1
# Then EN via the generated file as above
```

### `RunSeedDummyData.ps1` — orchestrator

| Mode | Command | Effect |
|------|---------|--------|
| Images only (default) | `.\RunSeedDummyData.ps1` | `GenerateSeedImages.ps1` for existing `FileStorages` |
| Full seed | `.\RunSeedDummyData.ps1 -SeedDatabase` | `SeedDummyData.sql` + images |
| Data only | `.\RunSeedDummyData.ps1 -SeedDatabase -SkipImages` | SQL only |
| Theme pages | `.\RunSeedDummyData.ps1 -ThemePages` | Upsert `PT Dummy T1-8` menus + `MenuMainImage`/`MenuGallery` |
| Cleanup | `.\RunSeedDummyData.ps1 -CleanupDatabase` | `CleanupDummyData.sql` + delete `product-*.jpg`/`thbproduct-*.jpg` |

`-Scale 2` doubles catalog/order volumes (menus/slides/settings stay small). `-MediaRoot` defaults to `EImece/media/images` or `C:\inetpub\wwwroot\Eimece\media\images`.

### `GenerateSeedImages.ps1`

Creates JPEG placeholders for every `FileStorages` row with `FileUrl='/media/seed/%'` and `IsFileExist=0`. Uses `System.Drawing` to render 1200x900 JPEG + `thumbs/thb*.jpg` 300px, updates `IsFileExist=1`. Syncs to IIS if `MediaRoot` differs from `C:\inetpub\wwwroot\Eimece\media\images`.

```powershell
.\GenerateSeedImages.ps1 -MediaRoot C:\inetpub\wwwroot\Eimece\media\images -ConnectionString "..." -MarkExisting
```

### `CleanupDummyData.sql`

Deletes all `SEED` markers in dependency order (OrderProducts→Orders→Products→Categories, StoryTags→Stories, etc.). Idempotent, safe to re-run. For bilingual, run before full re-seed; for single-language delete, filter `WHERE Lang=2`.

### Other SQL

| File | Purpose |
|------|---------|
| `AddPerformanceIndexes.sql` | `sys.dm_db_missing_index_details` report + `CREATE INDEX` DDL for `Products`, `Orders`, `ProductCategories` |
| `AddAuthenticatorTwoFactor.sql` | Adds `AspNetUsers.TwoFactorAuthenticatorEnabled` + `AuthenticatorKey` for TOTP 2FA |
| `Fix_GetRegionalSalesReport.sql` | Patch for `GetRegionalSalesReport` stored proc |
| `MonitorQueryExecutionPlans.sql` | `sys.dm_exec_query_stats` + `sys.dm_exec_query_plan` live monitor |
| `SeedThemePages.sql` | Upserts `PT Dummy T1-8` menus only (no catalog wipe) — used by `-ThemePages` |
| `UpsertComprehensiveMailTemplates.sql` | `MERGE` for `MailTemplates` (order confirmation, etc.) |
| `ledampulburada-SQLQuery2.sql` | 2.4 MB full backup dump — **source of truth for live DB schema** (do not edit, use for `check-schema.ps1`) |

### `_gen_realistic_seed.py` / `GenerateEnglishSeed.py`

- `_gen_realistic_seed.py` — Python 3 generator for `SeedDummyData.sql` from `BRANDS`, `CATEGORIES`, `PRODUCTS` (40 patterns), `TAGS`, `STORIES` with Turkish copy. Edit arrays, run `python _gen_realistic_seed.py` → overwrites `SeedDummyData.sql`.
- `GenerateEnglishSeed.py` — imports `BRANDS_EN`, `CATEGORIES_EN`, `PRODUCTS_EN` (real English translations), sets `base.* = *_EN`, writes `SeedDummyData_EN_Real.sql` with `Lang=2`, `CleanupFirst=0`, and patches (`AspNetUsers` guard, `Lang` filter, `Coupon`/`OrderNumber` `-EN`).

## 5. Frontend assets (not Playwright)

`EImece/Scripts/` (capital S) contains jQuery, Bootstrap, `adminEimece.js`, `griddly.js`, `filepond/`, `blueimp-gallery2/`, `tinymce/` — **not** to be confused with `scripts/playwright`. See `EImece/Scripts/README.md`.

## 6. Conventions

- **Encoding:** All `.sql` with Turkish must be saved UTF-8 and executed with `-f 65001`. PowerShell `Get-Content -Raw` + `[IO.File]::WriteAllText(..., UTF8)` preserves correctly; `Set-Content -Encoding UTF8` may add BOM — both acceptable for `sqlcmd`.
- **Connection:** Never hard-code passwords. Scripts resolve `EIMECE_DB_CONNECTION_STRING` env → `C:\inetpub\wwwroot\ConnectionStrings.config` (parent folder, outside publish) → `Web.config` placeholder. See `docs/SECURE_CONNECTION_STRINGS.md`.
- **Idempotence:** Seed scripts are idempotent when `@CleanupFirst=1`; for bilingual, second language must use `@CleanupFirst=0` + Lang-filtered `MIN/COUNT` patches (as in `*_EN_Real_Patched.sql`).
- **Images:** SQL alone is invisible in admin until `GenerateSeedImages.ps1` creates files under `media/images` + `thumbs` and sets `IsFileExist=1`. See `docs/MEDIA_AND_SEED_IMAGES_GUIDE.md`.

## 7. Related docs

- `docs/BUILD_AND_RUN.md` — build, `Web.config`, dummy data quickstart, `media/` permissions.
- `docs/MEDIA_AND_SEED_IMAGES_GUIDE.md` — DB vs filesystem image layers.
- `docs/SECURE_CONNECTION_STRINGS.md` — secret resolution order.
- `docs/PERFORMANCE_AND_CACHING.md` — why seed volumes stay small (cache prefixes).
