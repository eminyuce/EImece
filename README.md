# EImece — Open-Source E-Commerce Platform

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8.1-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET MVC](https://img.shields.io/badge/ASP.NET%20MVC-5.3-512BD4?logo=.net&logoColor=white)](https://www.nuget.org/packages/Microsoft.AspNet.Mvc)
[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-Support%20Development-yellow.svg?style=flat-square&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/eminyuce)

**EImece** is a full-featured, open-source e-commerce web application for product catalogs, content, checkout, and store operations. It runs on **ASP.NET MVC 5**, **Entity Framework 6**, and **.NET Framework 4.8.1**, with a three-project layout and **Microsoft.Extensions** infrastructure on classic ASP.NET.

| | |
|---|---|
| **Runtime** | .NET Framework **4.8.1** |
| **Web** | ASP.NET MVC **5.3**, ASP.NET Identity, OWIN |
| **Data** | Entity Framework **6.5**, SQL Server |
| **DI & infra** | Microsoft.Extensions.DependencyInjection, Logging, Http, Options, Caching.Memory |
| **Admin lists** | **Griddly 3.8** (`Griddly.Core`) — legacy **Grid.Mvc** removed |
| **Payments** | [Iyzico](https://www.iyzico.com/en) (Strategy pattern) |
| **Storefront designs** | Interchangeable Razor designs (**Crizal**, **Modern**) |
| **License** | [Apache License 2.0](LICENSE) |

> **Platform note:** The solution **compiles on Linux** (CI / `EImece/scripts/build.sh`). The web app **runs on Windows** with IIS or IIS Express and a reachable SQL Server database.

---

## Support & Sponsorship

If you find this project useful, consider supporting continued development:

<a href="https://buymeacoffee.com/eminyuce" target="_blank">
  <img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" width="200" />
</a>

👉 **[Support development on Buy Me a Coffee](https://buymeacoffee.com/eminyuce)**

---

## Table of contents

- [Why EImece](#why-eimece)
- [Key features](#key-features)
- [Architecture](#architecture)
- [Microsoft.Extensions infrastructure](#microsoftextensions-infrastructure)
- [Logging](#logging)
- [Technology stack](#technology-stack)
- [Repository layout](#repository-layout)
- [Getting started](#getting-started)
- [Configuration](#configuration)
- [Multi-design storefront](#multi-design-storefront)
- [Payments (Iyzico)](#payments-iyzico)
- [Observability & operations](#observability--operations)
- [Testing](#testing)
- [Deployment](#deployment)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [Security](#security)
- [License](#license)

---

## Why EImece

EImece targets shops that need a complete storefront and admin back office without a heavyweight SaaS platform:

- **Catalog & merchandising** — categories, brands, tags, galleries, filters, configurable sorting
- **Content** — menus, banner carousels, stories/blog, themed pages, mail templates
- **Commerce** — cart, guest and member checkout, coupons, order tracking, cargo numbers
- **Payments** — real-time Iyzico checkout with a pluggable payment Strategy
- **Operations** — health checks, metrics, structured logging, OpenTelemetry
- **Theming** — switch storefront UI via `ActiveDesign` without forking the whole app

---

## Key features

### Storefront

- Home page with banner carousels, custom menus, and themed content
- Product listing by category, brand, and tag, with filters (price, rating, brand)
- Product detail pages with galleries, FAQs, share links, and configurable payment HTML
- Shopping cart with AJAX updates; guest and registered checkout
- Order confirmation and cargo / tracking numbers
- Stories (blog) with short category URLs (`/s/sc/…`)
- Contact form with email and WhatsApp support options
- RSS, sitemap, and robots endpoints
- Optional **Google reCAPTCHA v2** (legacy arithmetic captcha still available)
- Responsive storefront designs: **Crizal** (default) and **Modern**

### Admin area (`/Admin`)

- Dashboard and operational reports
- Customers, users, and customer roles
- Orders — status, cargo numbers, internal notes
- Products, categories, brands, tags, and bulk price updates
- List pages powered by **Griddly 3.8** (async `IndexGrid`, AJAX pager/sort)
- **TinyMCE** for rich admin content; **FilePond-style** media uploads
- Menus, main-page slides, stories, FAQs, coupons, subscribers
- Mail templates, settings, and application logs
- Authenticated **metrics** endpoint for in-process counters

**Logins**

| Who | URL |
|-----|-----|
| Administrators | `/account/adminlogin/` |
| Customers | `/account/login/` |

### Customer area (`/Customers`)

- Account management and order history
- Product FAQs on the customer account experience

### Operations & security

| Capability | Details |
|------------|---------|
| Health | `GET /health` and `GET /healthz` (SQL, file storage, background services, external APIs) |
| Metrics | `GET /metrics` (authenticated administrators) |
| Telemetry | OpenTelemetry (OTLP primary; optional Azure Monitor exporter) |
| Logging | Constructor-injected `ILogger<T>` → NLog async rolling files + optional AppLogs DB |
| Headers | `SecurityHeadersHttpModule` in **EImece.Web** |
| Correlation | `CorrelationIdHttpModule` in **EImece.Web** |
| Secrets | DB credentials and encryption keys via environment variables — not committed |

**Admin grids:** Legacy **Grid.Mvc** is gone. Admin lists use **Griddly 3.8** — see [BUILD_AND_RUN.md](EImece/docs/BUILD_AND_RUN.md) and existing `Brands` / `Products` / `Orders` / `Stories` controllers for patterns (async `IndexGrid`, `.js-griddly-async`).

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  EImece (ASP.NET MVC 5 IIS host)                            │
│  Controllers · Areas/Admin · Areas/Customers · Razor Views  │
│  OWIN Identity · Bundles · Composition root (Global.asax)   │
└────────────────────────────┬────────────────────────────────┘
                             │ references
┌────────────────────────────▼────────────────────────────────┐
│  EImece.Web (net481 MVC plumbing)                           │
│  Base controllers · Filters · DesignAwareRazorViewEngine    │
│  SecurityHeadersHttpModule · CorrelationIdHttpModule        │
│  Captcha / reCAPTCHA services · Admin grid helpers           │
└────────────────────────────┬────────────────────────────────┘
                             │ DI (Microsoft.Extensions.DependencyInjection)
┌────────────────────────────▼────────────────────────────────┐
│  EImece.Domain (no System.Web)                              │
│  Entities · EF6 DbContext · Repositories · Services         │
│  Payment Strategy · LazyCache · Observability · Scheduler   │
└────────────────────────────┬────────────────────────────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
         SQL Server      Iyzico API    File storage
                                         (media/)
```

**Layering**

| Project | Responsibility |
|---------|----------------|
| **`EImece`** | IIS web host — HTTP entry, MVC controllers, Razor views, admin/customer areas, OWIN auth, `Web.config` |
| **`EImece.Web`** | Shared MVC infrastructure — base controllers, action filters, HTTP modules, design resolution, captcha, grid helpers |
| **`EImece.Domain`** | Business logic — entities, EF6, repositories, services, caching, payments, jobs, observability (no `System.Web`) |
| **`Resources`** | Localized string resources |
| **`EImece.Tests` / `EImece.MyConsole`** | MSTest coverage and one-off maintenance utilities |

**Patterns in use**

- Repository pattern over Entity Framework 6
- Service layer for business rules and orchestration
- **Payment Strategy** (`IPaymentStrategy` / `PaymentContext` / `IyzicoPaymentStrategy`)
- Design-aware Razor view engine with fallback to shared views
- **LazyCache** over `IMemoryCache` with hierarchical keys and prefix invalidation
- **Storefront projections + `AsNoTracking`** — public catalog reads project to DTOs at the repository boundary; admin CRUD keeps tracked EF entities

Open `EImece/EImece.sln` in **Visual Studio 2022** (solution format **18**).

---

## Microsoft.Extensions infrastructure

Configuration is bound from **`Web.config` `appSettings` and environment variables** — there is no `appsettings.json`. Registration lives in `DependencyInjectionConfig` and `EimeceOptionsRegistration`.

| Package area | Role in EImece |
|--------------|----------------|
| **DependencyInjection** | Composition root, request scopes, Quartz job factory |
| **Logging** | Application code uses constructor-injected **`ILogger<T>`** |
| **Http** | Named **`IHttpClientFactory`** clients for outbound calls |
| **Options** | Strongly typed settings from `Web.config` / env |
| **Caching.Memory** | Shared **`IMemoryCache`** backing **LazyCache** |

**Options types** (all bound via `AddEimeceOptions()`):

| Type | Purpose |
|------|---------|
| `LoggingOptions` | Minimum level, file/DB/console sinks, default path `media/logs` |
| `ObservabilityOptions` | Request logging, metrics, tracing, OTLP, HTTP resilience |
| `IyzicoOptions` | API key, secret, base URL |
| `CacheOptions` | TTLs, cache on/off (`Cache:SizeLimit` documented but not applied to `IMemoryCache`; see below) |
| `OutboundHttpOptions` | reCAPTCHA siteverify URL, Bitly base URL, secrets |

**Named HttpClients** (`HttpClientNames`):

| Name | Typical use |
|------|-------------|
| `EImece.Resilient` | General resilient outbound HTTP (Polly-backed) |
| `EImece.Iyzico` | Iyzico payment API |
| `EImece.Recaptcha` | Google reCAPTCHA verification |
| `EImece.ExternalApi` | Short-timeout external probes (health checks, Bitly, etc.) |

**Memory cache note:** `AddEimeceMemoryCache()` registers `IMemoryCache` **without** `SizeLimit`. LazyCache does not set per-entry sizes; enabling `SizeLimit` would throw at runtime. `CacheOptions.SizeLimit` remains in config for documentation only.

---

## Logging

- **Application contract:** `Microsoft.Extensions.Logging.ILogger<T>` (constructor injection throughout controllers, services, repositories, and jobs).
- **Provider:** **NLog** via `NLogLoggerProvider`, wrapped in `FailSafeLoggerProvider` for sink resilience.
- **Default directory:** **`media/logs`** (same writable root as uploads; HTTP access denied via `media/Web.config`). Override with `Logging:File:Path`.
- **Outputs:** async rolling **`EImeceLog.log`** (plain text) and **`EImeceLog.json`** (structured, with CorrelationId / TraceId / SpanId); optional **AppLogs** SQL table when `Logging:Database:Enabled=true`.
- **Serilog is not used** in the current stack — do not add it back for app logging.

Bootstrap: `LoggingBootstrap.Configure()` in `EImece.Domain/Observability/Logging/`.

---

## Technology stack

| Area | Stack |
|------|--------|
| Runtime | .NET Framework **4.8.1** |
| Web | ASP.NET MVC **5.3**, ASP.NET Identity, OWIN |
| Data | Entity Framework **6.5**, SQL Server |
| DI / infra | Microsoft.Extensions.DependencyInjection, Logging, Http, Options, Caching.Memory **10.0.10** |
| Mapping | AutoMapper |
| Payments | Iyzico (`Iyzipay`) |
| Logging / telemetry | NLog + MEL, Application Insights **3.x**, OpenTelemetry **1.15** (OTLP / Azure Monitor) |
| Jobs | Quartz.NET (optional; disable locally with `Quartz_Scheduler_IsEnabled=False`) |
| Front end | jQuery, Bootstrap, **Griddly 3.8**, TinyMCE, FilePond-style admin uploads |
| E2E tests | Playwright (Crizal theme against local IIS) |
| Unit tests | MSTest |

---

## Repository layout

```
EImece/                          # Repository root
├── README.md                    # This file
├── DEPLOYMENT.md                # Production CI/CD guidance
├── LICENSE                      # Apache License 2.0
├── Playwright/                  # End-to-end tests (Crizal / IIS)
└── EImece/                      # Solution folder
    ├── EImece.sln
    ├── EImece/                  # Web host (Controllers, Views, Areas, Web.config)
    │   ├── Areas/Admin/
    │   ├── Areas/Customers/
    │   ├── Views/Designs/       # Crizal, Modern storefront designs
    │   ├── SqlScripts/          # Seed data, indexes, cleanup
    │   └── media/               # Uploads + logs (needs write access)
    ├── EImece.Web/              # MVC plumbing, filters, HTTP modules
    ├── EImece.Domain/           # Domain, EF6, services, observability
    ├── EImece.Tests/            # MSTest unit / integration tests
    ├── EImece.MyConsole/        # Maintenance utilities
    ├── Resources/               # Localized strings
    ├── scripts/                 # build.sh, restore-packages.py, verify-*.ps1
    └── docs/                    # Detailed guides
```

| Project | Purpose |
|---------|---------|
| `EImece` | ASP.NET MVC 5 IIS host — controllers, views, areas, composition root |
| `EImece.Web` | Shared MVC infrastructure — filters, HTTP modules, base controllers, design engine |
| `EImece.Domain` | Entities, EF6, repositories, services, caching, payments, observability |
| `Resources` | Localized strings |
| `EImece.Tests` | MSTest unit and integration tests |
| `EImece.MyConsole` | One-off maintenance utilities |

---

## Getting started

### Prerequisites

| Goal | Requirements |
|------|----------------|
| **Run the site** | Windows 10/11 or Windows Server; **Visual Studio 2022** (ASP.NET workload) **or** IIS + .NET Framework 4.8.1; **SQL Server** (Express, Developer, or full) |
| **Build only** (Windows or Linux) | .NET SDK 8+ and Python 3 |

### 1. Clone

```bash
git clone https://github.com/eminyuce/EImece.git
cd EImece
```

### 2. Configure the database (no secrets in git)

Prefer an environment variable:

```powershell
$env:EIMECE_DB_CONNECTION_STRING = "Data Source=localhost;Initial Catalog=EImece;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;"
```

Or use a gitignored `ConnectionStrings.config` via `configSource`. Full options:  
[EImece/docs/SECURE_CONNECTION_STRINGS.md](EImece/docs/SECURE_CONNECTION_STRINGS.md)

Encryption secrets: prefer `EIMECE_ENCRYPTION_KEY` over storing keys in `Web.config`.

The database **schema must already exist** (apply your usual EF migrations or restore a backup). The deploy pipeline does **not** migrate the database.

#### Optional: seed demo data

For a realistic small shop (admin grids, storefront, orders):

```powershell
cd EImece/EImece/SqlScripts
.\RunSeedDummyData.ps1 -ConnectionString "Server=.;Database=EImece;Trusted_Connection=True;TrustServerCertificate=True;"
```

- Shared seed password pattern for test users: concatenate `Test` + `123` + `!` (local/test only)
- Known logins include `admin@eimece.test` (Admin), `editor@eimece.test`, `customer1@eimece.test`
- Admin sign-in: `/account/adminlogin/` — customer sign-in: `/account/login/`
- Cleanup: `CleanupDummyData.sql`, or re-run the seed script (`@CleanupFirst = 1` by default)
- Details: [BUILD_AND_RUN.md](EImece/docs/BUILD_AND_RUN.md)

### 3. Build

**Visual Studio:** open `EImece/EImece.sln` → Rebuild Solution.

**Command line (Windows Developer Prompt):**

```powershell
cd EImece
nuget restore EImece.sln
msbuild EImece.sln /t:Clean,Build /p:Configuration=Release
```

**Linux / CI (compile verification):**

```bash
cd EImece
chmod +x scripts/build.sh
./scripts/build.sh
```

### 4. Run

1. Set **EImece** as the startup project.
2. Press **F5**, or host the `EImece/EImece` folder in **IIS**.
3. Typical local URLs:
   - **IIS (common):** `http://localhost:81/`
   - **IIS Express:** `http://localhost:31544/` (check project Web properties if different)
4. Sign in:
   - Admin: `http://localhost:81/account/adminlogin/`
   - Customer: `http://localhost:81/account/login/`
5. Confirm health:

```bash
curl -s http://localhost:81/health
# IIS Express: curl -s http://localhost:31544/health
# Expect HTTP 200 and "Status": "UP"
```

Grant the IIS app pool identity **read/write** on `media/` (uploads + logs). See [IIS_APP_POOL_PERMISSIONS.md](EImece/docs/IIS_APP_POOL_PERMISSIONS.md).

Full walkthrough: [EImece/docs/BUILD_AND_RUN.md](EImece/docs/BUILD_AND_RUN.md)

---

## Configuration

Important `Web.config` / environment settings (never commit real secrets):

| Key / variable | Purpose |
|----------------|---------|
| `EIMECE_DB_CONNECTION_STRING` | Preferred SQL connection string (env var) |
| `EIMECE_ENCRYPTION_KEY` | Preferred encryption key (env var) |
| `ActiveDesign` | Storefront design name (`Crizal` or `Modern`) |
| `domain` | Public site domain used in links and emails |
| `SiteStatus` | `live` or maintenance modes |
| `Quartz_Scheduler_IsEnabled` | Background jobs (`False` for local dev) |
| `IyzicoBaseUrl` / `IyzicoOptions` | Payment API base URL (sandbox or production) |
| `CaptchaProvider` | `Legacy`, `Recaptcha`, or `None` |
| `Logging:MinimumLevel` | MEL minimum log level |
| `Logging:File:Path` | Log directory (default `media/logs`) |
| `EnableRequestLogging` | Structured request logging |
| `EnableMetrics` | In-process metrics collection |

reCAPTCHA setup: [EImece/RECAPTCHA.md](EImece/RECAPTCHA.md)

---

## Multi-design storefront

EImece resolves Razor views through a **design-aware view engine** (`EImece.Web`). Set the active design in `Web.config`:

```xml
<add key="ActiveDesign" value="Crizal" />
```

| Design | Description |
|--------|-------------|
| **Crizal** | Default multipurpose responsive e-commerce design |
| **Modern** | Alternate modern e-commerce design system |

Design views live under `EImece/EImece/Views/Designs/{DesignName}/`. Missing design-specific views fall back to shared / default views. Admin and Customers areas are unaffected by `ActiveDesign`.

To remove a design without breaking MSBuild publish: [DESIGN_REMOVAL_GUIDE.md](EImece/docs/DESIGN_REMOVAL_GUIDE.md)

---

## Payments (Iyzico)

Checkout uses **Iyzico** for real-time payment (guest and registered users), with confirmation and cargo / tracking support. PCI-sensitive card handling stays with Iyzico's infrastructure.

Implementation uses a **Strategy pattern**:

- `IPaymentStrategy` — payment provider contract
- `IyzicoPaymentStrategy` — Iyzico implementation
- `PaymentContext` — selects and runs the active strategy

Configure Iyzico keys and base URL on the server (`IyzicoOptions` / environment / server-only config). Do not commit production API keys.

---

## Observability & operations

| Endpoint / artifact | Who | Purpose |
|---------------------|-----|---------|
| `GET /health`, `GET /healthz` | Public | Liveness / dependency checks (SQL, file storage, scheduler, external APIs) |
| `GET /metrics` | Admin session | In-process counter snapshots |
| `media/logs/EImeceLog.log` | Operators | Plain-text logs |
| `media/logs/EImeceLog.json` | Operators | Structured JSON (CorrelationId, TraceId, SpanId, path) |

OpenTelemetry (OTLP, sampling, Azure Monitor): [EImece/docs/OPENTELEMETRY.md](EImece/docs/OPENTELEMETRY.md)

Performance (EF6 queries, SQL indexes, LazyCache, storefront projections): [EImece/docs/PERFORMANCE_AND_CACHING.md](EImece/docs/PERFORMANCE_AND_CACHING.md)

---

## Testing

### Unit / integration (MSTest)

Tests compile on Linux but **run on Windows** with Visual Studio and a configured test database.

1. Set `EIMECE_DB_CONNECTION_STRING` or a gitignored `EImece.Tests/ConnectionStrings.config`.
2. Visual Studio: Test Explorer, Run All, or:

```powershell
vstest.console.exe EImece.Tests/bin/Release/EImece.Tests.dll
```

Suggested first run: `ImageUtilitiesTests`, then controller integration tests that need SQL.

### End-to-end (Playwright)

The `Playwright/` suite targets a local IIS site (default `http://localhost:81`) for the Crizal theme. It covers cart, checkout, auth, navigation, and responsive checks.

```bash
cd Playwright
npm install
npx playwright install
npm test
```

---

## Deployment

Production deploy is described in **[DEPLOYMENT.md](DEPLOYMENT.md)** — manual GitHub Actions workflow, MSBuild Release publish, optional FTPS upload, and `/health` smoke test. Pushing to `master` does **not** auto-deploy.

Server-side only (never in git):

- `EIMECE_DB_CONNECTION_STRING`
- `EIMECE_ENCRYPTION_KEY`
- Iyzico, Application Insights, OTLP, SMTP credentials

---

## Documentation

| Document | Contents |
|----------|----------|
| [DEPLOYMENT.md](DEPLOYMENT.md) | Production CI/CD (Windows MSBuild, FTPS, secrets, rollback) |
| [BUILD_AND_RUN.md](EImece/docs/BUILD_AND_RUN.md) | Build, run, health checks, seed data, tests, common errors |
| [SECURE_CONNECTION_STRINGS.md](EImece/docs/SECURE_CONNECTION_STRINGS.md) | Env vars, `configSource`, TLS, production |
| [OPENTELEMETRY.md](EImece/docs/OPENTELEMETRY.md) | OTLP, sampling, Azure Monitor exporter |
| [LATENCY_PERCENTILES.md](EImece/docs/LATENCY_PERCENTILES.md) | P90/P95/P99 metrics and Admin `/metrics` |
| [PERFORMANCE_AND_CACHING.md](EImece/docs/PERFORMANCE_AND_CACHING.md) | EF6 query tuning, SQL indexes, LazyCache, storefront projections |
| [ASYNC_AWAIT_GUIDE.md](EImece/docs/ASYNC_AWAIT_GUIDE.md) | Async EF6 / thread-pool guidance |
| [DESIGN_REMOVAL_GUIDE.md](EImece/docs/DESIGN_REMOVAL_GUIDE.md) | Removing a storefront design from the `.csproj` safely |
| [IIS_APP_POOL_PERMISSIONS.md](EImece/docs/IIS_APP_POOL_PERMISSIONS.md) | `media/` ACL for IIS |
| [MEDIA_AND_SEED_IMAGES_GUIDE.md](EImece/docs/MEDIA_AND_SEED_IMAGES_GUIDE.md) | Media management, seed images, Storefront vs Admin behavior |
| [SCRIPTS.md](EImece/docs/SCRIPTS.md) | PowerShell verify/deploy scripts |
| [RECAPTCHA.md](EImece/RECAPTCHA.md) | Captcha providers and Web.config keys |

---

## Contributing

1. Fork the repository and create a feature branch from `master`.
2. Keep secrets out of commits (`ConnectionStrings.config`, real API keys, encryption keys, production connection strings).
3. Prefer small, focused pull requests with a clear description of behavior changes.
4. Build locally (or via `EImece/scripts/build.sh` on Linux) before opening a PR.
5. Add or update tests when changing business logic, payments, or design resolution.
6. **Admin lists:** keep `IndexGrid` **async**; copy an existing `*Grid.cshtml` + `IndexGrid` action. **Do not add legacy Grid.Mvc back.**

Questions about runtime errors? Start with the troubleshooting tables in [BUILD_AND_RUN.md](EImece/docs/BUILD_AND_RUN.md).

---

## Security

- Never commit production connection strings, Iyzico keys, encryption keys, or SMTP credentials.
- Prefer environment variables (`EIMECE_DB_CONNECTION_STRING`, `EIMECE_ENCRYPTION_KEY`) or server-only config files.
- Keep `TrustServerCertificate=False` in production when the SQL Server certificate chain is valid.
- Use FTPS (or equivalent) for deploys; rotate FTP credentials if they leak.
- Review security response headers after deploy; confirm `/health` does not expose sensitive internals beyond operational status.

If you discover a vulnerability, do not open a public issue with exploit details — contact the maintainers privately when possible.

---

## License

Licensed under the **Apache License, Version 2.0** — see [LICENSE](LICENSE).

```
Copyright contributors to EImece
```

You may use, modify, and distribute this software under the terms of the Apache License 2.0.
