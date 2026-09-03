# EImece Production Readiness Review

**Repository:** [eminyuce/EImece](https://github.com/eminyuce/EImece)  
**Date:** 4 September 2026  
**Scope:** Operational and architectural gaps on the current stack. This is not a rewrite plan.  
**Runtime:** .NET Framework 4.8.1  
**Stack:** ASP.NET MVC 5.3, ASP.NET Identity, OWIN, Entity Framework 6.5, SQL Server, IIS  
**Evidence:** Local solution plus a live probe of `http://localhost:81`  
**Supersedes:** [ENTERPRISE_ARCHITECTURE_REVIEW.md](ENTERPRISE_ARCHITECTURE_REVIEW.md) for current go-live status (that document still describes Release-config auth bypasses and a synchronous NLog database rule that are no longer in the tree)

---

## 1. Verdict

**Enterprise readiness: 5.5 / 10**

EImece already has better observability, Identity, payment binding checks, and unit-test volume than a typical MVC 5 hobby shop. It is **not ready to sell as an enterprise product**. The blockers are operational:

- Secrets still live in `Web.config` (source and the IIS site)
- The documented GitHub Actions pipeline is not in the repository
- Commercial APIs and Swagger are anonymous on HTTP
- Customer lockout and rate limiting are off
- Deploy scripts can wipe uploads or copy a debug `Web.config` onto the server

Stay on ASP.NET MVC 5 / .NET 4.8.1. Close operations first.

| Dimension | Score (1–10) | Summary |
| --- | ---: | --- |
| Error handling and logging | 7.5 | NLog async, correlation IDs, custom error pages, health checks exist |
| Testing and maintainability | 6.0 | 555 MSTest methods; almost no controller or CI coverage |
| Configuration and secrets | 5.5 | Env-var design is right; defaults are still in git and on IIS |
| Security | 5.0 | Identity + admin TOTP + payment checks; APIs, HTTPS, lockout are not |
| CI/CD and deployment | 3.0 | Docs and scripts exist; `.github/workflows` does not |

---

## 2. What is already in place

Do not rebuild these. The gap is consistency and operations, not a missing architecture.

| Capability | Status | Where |
| --- | --- | --- |
| Central logging | NLog 6 async + MEL + JSON files + `AppLogs` (Warn+) | `EImece/NLog.config`, `LoggingBootstrap.cs` |
| Correlation / tracing | `X-Correlation-Id`, W3C `traceparent`, OpenTelemetry hooks | `CorrelationIdHttpModule.cs`, `OpenTelemetryBootstrap.cs` |
| Error pages | `customErrors` + `ErrorController` + `Application_Error` fallback | `Global.asax.cs`, `Views/Error` |
| AuthN / AuthZ | Identity + admin TOTP + role split + captcha hooks | `Startup.Auth.cs`, `TwoFactorTokenService.cs` |
| Security headers | CSP, `X-Frame-Options`, nosniff, strip `Server` | `SecurityHeadersHttpModule.cs` |
| Payment integrity | Iyzico token retrieve, basket bind, ±0.05 price check, idempotent order | `PaymentController.cs`, `IyzicoService.cs` |
| Secret resolution design | Env vars documented; committed connection string is a placeholder | `ConnectionStringProvider.cs`, [SECURE_CONNECTION_STRINGS.md](SECURE_CONNECTION_STRINGS.md) |
| Unit tests | 555 methods — coupons, cart, orders, 2FA, cache, NLog | `EImece.Tests` |
| Release transform | Sets `SiteStatus=live`, `customErrors=RemoteOnly`, removes `debug` | `Web.Release.config` (HTTPS block still commented) |

`Web.Release.config` no longer contains `BypassAdminAuth`, `ExposeDetailedErrors`, or `TwoFactorBypassUsers`. NLog database and file targets are wrapped in `AsyncWrapper` and only Warn+ goes to SQL.

---

## 3. Live IIS snapshot (`http://localhost:81`, 4 Sep 2026)

| Check | Result |
| --- | --- |
| `GET /health` (15s) | Timed out — health checks have no timeout |
| `GET /swagger` | 301 → `/swagger/ui/index` (public) |
| `GET /swagger/docs/v1` | 200 — documents cart/order APIs |
| `GET /api/v1/orders/track/1` | 404 anonymous (endpoint is live) |
| `compilation debug` | Attribute removed (Release transform applied) |
| `customErrors` | `RemoteOnly` |
| `UseSSL` / `RateLimit:Enabled` / `Quartz_Scheduler_IsEnabled` | `false` / `false` / `False` |
| Iyzico keys and `encrypt-password` | Same values as source `Web.config` |

The Release transform helped `debug` / `customErrors`. It did not remove secrets or enable HTTPS.

---

## 4. Findings

### Critical

#### C1 — Payment and encryption secrets in source and on IIS

Iyzico sandbox API key/secret and `encrypt-password` are committed in `EImece/EImece/Web.config` and present on `C:\inetpub\wwwroot\Eimece\Web.config`. `EncryptionSecretProvider` can read `EIMECE_ENCRYPTION_KEY` and Iyzico can read `EIMECE_IYZICO_*`; the IIS site is not using those overrides.

**Fix:** Rotate all three. Remove values from `Web.config`. Set environment variables on the app pool. Purge git history if those keys were ever used in production.

#### C2 — Unauthenticated Web API v1 plus public Swagger on HTTP

`CartApiController`, `OrdersApiController`, and `SubscribersApiController` have no `[Authorize]`. Swagger UI is registered unconditionally in `SwaggerConfig.cs`. A live GET of `/swagger/docs/v1` returned 200 and documented `/api/v1/cart/{orderGuid}`. Order tracking by order number is reachable without a session.

**Fix:** Disable Swagger in Release (or protect it). Put Web API behind API keys or Identity using `System.Web.Http.AuthorizeAttribute`. Rate-limit coupon validate and subscribe. Return minimal DTOs.

### High

| ID | Area | Finding | Evidence |
| --- | --- | --- | --- |
| H1 | Security | HTTPS is not enforced. HSTS rewrite is commented out. OWIN cookies have no explicit `Secure`, `HttpOnly`, or `SameSite`. | `UseSSL=false`; `Web.Release.config`; `Startup.Auth.cs` |
| H2 | Security | `PasswordSignInAsync` uses `shouldLockout: false` on admin and customer login. `RateLimit:Enabled` is `false` on source and IIS. `CaptchaProvider` defaults to `None`. | `AccountController.cs`; `Web.config` |
| H3 | CI/CD | [DEPLOYMENT.md](../../DEPLOYMENT.md) specifies `.github/workflows/deploy.yml`. That directory does not exist locally or on GitHub. | Repo root; GitHub code search |
| H4 | Testing | 555 MSTest methods and 28 Playwright specs are never run as a merge gate. `scripts/build.sh` compiles only. | `EImece.Tests`; `Playwright/`; `scripts/build.sh` |
| H5 | Logging | `/health` is `[AllowAnonymous]` and returns SQL `DataSource`, database name, storage paths, and Iyzico `BaseUrl`. Live call timed out. `AddCheck` has no timeout. | `HealthController.cs`; `SqlServerHealthCheck.cs`; `EimeceHealthCheckRegistration.cs` |
| H6 | Logging | Admin `OnException` always embeds `ex.ToString()` in HTML for authenticated staff, independent of `compilation debug`. | `BaseAdminController.cs` |
| H7 | Deploy | Source `Web.config` has `compilation debug="true"`. `deploy-local-iis.ps1` uses `robocopy /MIR` and can copy that file over IIS. `FolderProfile.pubxml` sets `DeleteExistingFiles=true` while the comment says keep it `false`. | `Web.config`; `scripts/deploy-local-iis.ps1`; `FolderProfile.pubxml` |
| H8 | Security | Admin `[AllowHtml]` fields render through `Html.Raw` on the storefront. A compromised editor account is persistent XSS. | `AdminHtmlFieldMetadata.cs`; design `Detail` and page-theme views |
| H9 | CI/CD | Schema is manual SQL (`01_CreateDatabase.sql` + `SqlScripts`). No EF Migrations, DbUp, or Flyway. Documented deploy skips the database. | `App_Data/`; `SqlScripts/`; `DEPLOYMENT.md` |
| H10 | Secrets | `BitlyRepository` ships a hardcoded default access token when appSettings is empty. | `EImece.Domain/ApiRepositories/BitlyRepository.cs` |

### Medium

- Storefront `PaymentController.AddToCart` and several `ReportController` POSTs lack `[ValidateAntiForgeryToken]`.
- `Quartz_Scheduler_IsEnabled=False`, so `ClearLogsFromDbJob` never runs and `AppLogs` can grow unbounded.
- `SensitiveDataMasker` is not applied to the general MEL pipeline; Iyzico checkout still logs buyer email/GSM.
- Cache and rate limiter are in-process only — a second IIS node splits state.
- Password policy is 6 characters with no symbol requirement. Customer 2FA is not enforced.
- No `machineKey` — antiforgery tokens will not survive a web farm.
- File uploads are extension-checked, not magic-byte validated.
- No `[Bind(Include=...)]` on POST actions (mass-assignment surface).
- Application Insights and OTLP stay off unless env vars are set at deploy.
- Playwright is not in CI. Controller test coverage is about 11% by file count.
- NLog async targets use `overflowAction="Discard"`.
- `WebApiExceptionFilter` logs but does not shape a structured error body.

### Low

- Unstructured string-concat logging in several controllers.
- File log retention is count-based (10 archives), not time-based.
- `HealthController` contains large commented deployment scratch notes.
- EPPlus 4.5.3 is EOL.
- No dedicated `CONTRIBUTING.md` or formal PR checklist.

---

## 5. Dimension notes

### 5.1 Error handling and logging

Exception flow is layered and appropriate for MVC 5:

```
CorrelationIdHttpModule
  → Telemetry / request-logging filters
  → StructuredExceptionFilter (AJAX JSON)
  → BaseController / BaseAdminController.OnException
  → HandleErrorAttribute
  → Application_Error
  → ErrorController or static 500 HTML
```

What still matters in production:

- Admin HTML always includes the stack; show a correlation ID instead.
- AJAX errors from `Application_Error` omit `CorrelationId`.
- Apply `SensitiveDataMasker` in the MEL pipeline.
- Enable Quartz (or a SQL Agent job) for `ClearLogsFromDbJob`.
- Put timeouts on health checks. Restrict `/health` to an internal bind, IP allowlist, or auth. Public payload should be `{ "status": "UP" }` only.
- Configure App Insights and/or `OtlpEndpoint` if you want centralized APM.

### 5.2 Security

Identity lockout is configured (5 failures / 5 minutes) but sign-in passes `shouldLockout: false`, so the counter never moves for customers. Combined with `RateLimit:Enabled=false` and `CaptchaProvider=None`, login is open to online guessing.

Form POSTs on account and payment are mostly protected. Storefront cart AJAX and some admin report POSTs are not. MVC `AuthorizeRolesAttribute` on `UrlController` (`ApiController`) does not run on the Web API pipeline — treat `/api/*` as public until verified with `System.Web.Http.AuthorizeAttribute`.

CSP exists but allows `'unsafe-inline'`, `'unsafe-eval'`, and `img-src http:`. Tighten it after sanitizing stored HTML.

### 5.3 Configuration and secrets

Intended resolution order is sound:

1. `EIMECE_DB_CONNECTION_STRING` or parent `C:\inetpub\wwwroot\ConnectionStrings.config`
2. `EIMECE_IYZICO_API_KEY` / `EIMECE_IYZICO_SECRET_KEY`
3. `EIMECE_ENCRYPTION_KEY`

Committed `connectionStrings` is a placeholder. `EncryptionSecretProvider` fails closed and does not log the secret.

Reality: Iyzico keys and `encrypt-password` are still defaulted in source and on the IIS site. There is no Staging transform — only Debug/Release. Feature toggles are AppSettings, which means an IIS recycle to change them.

### 5.4 CI/CD and deployment

Publish profiles, FTPS/robocopy scripts, health smoke scripts, and a long [DEPLOYMENT.md](../../DEPLOYMENT.md) exist. A pipeline does not. GitHub has zero workflows. Nothing restores NuGet, builds Release, runs MSTest, or publishes on PR.

Three deploy paths disagree:

| Path | Behavior | Risk |
| --- | --- | --- |
| `GitHubActions.pubxml` | `artifacts/publish`, `DeleteExistingFiles=false`, excludes `media/` | Correct shape; unused because the workflow is missing |
| `FolderProfile.pubxml` | Publishes to `C:\inetpub\wwwroot\Eimece` with `DeleteExistingFiles=true` | Can wipe `media/images` |
| `scripts/deploy-local-iis.ps1` | `robocopy /MIR` from source project | Overwrites IIS `Web.config` with source `debug="true"`; can delete server-only files |

Schema changes are operator-run SQL. That will drift the moment a second environment exists. Backup/DR is not documented (no RTO/RPO).

### 5.5 Testing and maintainability

Layering is Repository + Service + `Microsoft.Extensions.DependencyInjection`. `FakeServiceProxy` lets commerce tests avoid a live EF context — that is the pattern to extend. `HomeControllerTest` still mixes database, mail, and Razor in one large class.

| Surface | Impression |
| --- | --- |
| Helpers / infrastructure / cache | Strong |
| Domain commerce services (coupon, cart, order, 2FA, Iyzico validation) | Moderate |
| Controllers | Weak (~6 of ~55 have tests) |
| Playwright E2E in CI | None (28 specs, local IIS only) |

Untested at controller level: `PaymentController` checkout, `AccountController`, most Admin CRUD, file upload, and live Iyzico HTTP. Tests that need SQL Server will not run on Linux compile agents. No coverlet/OpenCover is configured.

---

## 6. 90-day plan (same stack)

Stay on MVC 5 / .NET 4.8.1. Do not start an ASP.NET Core port until one node is boringly reliable.

### P0 — before go-live

1. Rotate Iyzico, `encrypt-password`, and Bitly. Remove values from `Web.config`. Set `EIMECE_*` on the IIS app pool. Purge history if those keys were ever production.
2. Disable Swagger in Release. Authenticate Web API. Rate-limit subscribers and coupon validate.
3. Set `shouldLockout: true`, `RateLimit:Enabled=true`, `UseSSL=true`, uncomment HSTS, set `CookieSecure=Always` + SameSite, enable captcha in production.
4. Add `.github/workflows`: restore, MSBuild Release, MSTest (Helpers + Infrastructure + Services), publish with `GitHubActions.pubxml`.
5. Set `compilation debug="false"` in source. Fix or delete `deploy-local-iis.ps1` `/MIR` and `FolderProfile` `DeleteExistingFiles=true`.

### P1 — first hardening sprint

6. Admin errors: correlation ID only. Health: auth or IP allowlist, timeouts, public `{status}` only. Apply `SensitiveDataMasker` in the MEL pipeline. Enable Quartz log purge or a SQL Agent job.
7. HTML sanitizer on `[AllowHtml]` save. Antiforgery on `AddToCart` and Report POSTs. `System.Web.Http.Authorize` on API controllers.
8. Introduce versioned SQL migrations (DbUp is the least disruptive on 4.8.1). Stop treating `01_CreateDatabase.sql` as the live contract.

### P2 — first quarter

9. `PaymentController` and `AccountController` tests. Playwright smoke on a Windows runner after deploy. Coverage on the test project.
10. Only if a second node is required: shared cache and rate limiter, explicit `machineKey`, sticky sessions or out-of-process session. Write a backup/DR runbook with RPO/RTO.

---

## 7. Scope reminder

This review does not recommend ASP.NET Core, Blazor, or a SPA. The current stack can be operated as a single-tenant IIS product if the P0 items land. Multi-instance HA is optional and should wait until one node is operationally quiet.

Related docs: [SECURE_CONNECTION_STRINGS.md](SECURE_CONNECTION_STRINGS.md), [IIS_APP_POOL_PERMISSIONS.md](IIS_APP_POOL_PERMISSIONS.md), [OPENTELEMETRY.md](OPENTELEMETRY.md), [BUILD_AND_RUN.md](BUILD_AND_RUN.md), [DEPLOYMENT.md](../../DEPLOYMENT.md).
