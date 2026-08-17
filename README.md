# EImece — Open-Source E-Commerce Platform

**EImece** is a full-featured, open-source e-commerce web application for product catalogs, content, checkout, and store operations. It is built with **ASP.NET MVC 5**, **Entity Framework 6**, and **Microsoft.Extensions.DependencyInjection**, using a clear **Repository + Service Layer** architecture.

| | |
|---|---|
| **Runtime** | .NET Framework **4.8.1** |
| **Web** | ASP.NET MVC **5.3**, ASP.NET Identity, OWIN |
| **Data** | Entity Framework **6.5**, SQL Server |
| **Admin lists** | **Griddly 3.8** (`Griddly.Core`) — Grid.Mvc has been removed |
| **Payments** | [Iyzico](https://www.iyzico.com/en) (Strategy pattern) |
| **Storefront designs** | Interchangeable Razor designs (**Crizal**, **Modern**) |
| **License** | [Apache License 2.0](LICENSE) |

> **Platform note:** The solution **compiles on Linux** (CI / `scripts/build.sh`). The web app **runs on Windows** with IIS or IIS Express and a reachable SQL Server database.

---

## Table of contents

- [Why EImece](#why-eimece)
- [Key features](#key-features)
- [Admin grids (Griddly)](#admin-grids-griddly)
- [Architecture](#architecture)
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

EImece is aimed at shops that need a complete storefront and admin back office without a heavyweight SaaS platform:

- **Catalog & merchandising** — categories, brands, tags, galleries, filters, and configurable product sorting
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
- **TinyMCE** for rich admin content (products, stories, menus, mail templates)
- Media library with **FilePond-style** uploads
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
- Product FAQs visible on the customer account experience

### Operations & security

| Capability | Details |
|------------|---------|
| Health | `GET /health` and `GET /healthz` (SQL, file storage, background services) |
| Metrics | `GET /metrics` (authenticated administrators) |
| Telemetry | OpenTelemetry (OTLP primary; optional Azure Monitor exporter) |
| Logging | NLog / Serilog; structured JSON under `media/logs/` with CorrelationId / TraceId / SpanId |
| Headers | `SecurityHeadersHttpModule` (`X-Content-Type-Options`, `X-Frame-Options`, …) |
| Secrets | DB credentials and encryption keys via environment variables — not committed |

---

## Admin grids (Griddly)

Admin list pages use **Griddly 3.8** (`Griddly.Core` / `Griddly.Mvc`). **Grid.Mvc has been removed** — do not restore that package or its helpers.

### How lists load

- Each list has a normal `Index` view plus an **`IndexGrid`** action that returns `QueryableResult<T>`.
- Keep **`IndexGrid` async** (`async Task<ActionResult>`) so EF6 queries can `await` without blocking the thread pool.
- The list view hosts `@Html.Partial("Grid/_GridChrome")` and a `.js-griddly-async` container whose `data-url` points at `IndexGrid`.
- **First paint** is full table HTML: column headers, pager, and the blue record-count bar.
- Later **pager / sort** requests replace **tbody** via AJAX (not a full page reload).

### Skin and ProductState badges

- Skin: `Content/adminGridModern.css` + `Content/adminGriddlyCompat.css` (loaded with `Content/griddly.css`).
- **ProductState** badges are color-coded, for example: in stock **green**, out of stock **red**, pre-order **blue** (also discontinued, backorder, coming soon, limited stock, reserved, awaiting restock, not for sale).

### Adding a new admin list

Copy an existing pair rather than inventing a new grid stack:

1. `Areas/Admin/Views/{Controller}/IndexGrid.cshtml` — `GriddlySettings<T>` columns and templates
2. `{Controller}Controller.IndexGrid` — **async**, `CanRenderGrid()` guard, `return new QueryableResult<T>(query)`
3. `Index.cshtml` — chrome + `.js-griddly-async` pointing at `IndexGrid`

Reference implementations: `Brands`, `Products`, `Orders`, `Stories`.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  EImece (ASP.NET MVC 5)                                     │
│  Controllers · Areas/Admin · Areas/Customers · Razor Views  │
│  DesignAwareRazorViewEngine · OWIN Identity · Bundles       │
│  Admin lists: Griddly 3.8 (async IndexGrid)                 │
└────────────────────────────┬────────────────────────────────┘
                             │ DI (Microsoft.Extensions.DependencyInjection)
┌────────────────────────────▼────────────────────────────────┐
│  EImece.Domain                                              │
│  Entities · EF6 DbContext · Repositories · Services         │
│  Payment Strategy · Caching · Observability · Scheduler     │
└────────────────────────────┬────────────────────────────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
         SQL Server      Iyzico API    File storage
                                         (media/)
```

**Layering**

| Layer | Responsibility |
|-------|----------------|
| **Web (`EImece`)** | HTTP, MVC controllers, Razor views, admin/customer areas, Griddly lists, design resolution, OWIN auth |
| **Domain (`EImece.Domain`)** | Entities, EF6 context, generic repositories, business services, caching, payments, jobs |
| **Resources** | Localized string resources |
| **Tests / Console** | MSTest coverage and one-off maintenance utilities |

**Patterns in use**

- Repository pattern over Entity Framework 6
- Service layer for business rules and orchestration
- **Payment Strategy** (`IPaymentStrategy` / `PaymentContext` / `IyzicoPaymentStrategy`)
- Design-aware Razor view engine with fallback to shared views
- Memory cache with hierarchical keys and prefix invalidation
- **Storefront projections + `AsNoTracking`** — public catalog reads project to DTOs at the repository boundary (see `ProductCategoryRepository` storefront methods). Admin CRUD keeps full tracked EF entities.

---

## Technology stack

| Area | Stack |
|------|--------|
| Runtime | .NET Framework **4.8.1** |
| Web | ASP.NET MVC **5.3**, ASP.NET Identity, OWIN |
| Data | Entity Framework **6.5**, SQL Server |
| DI | **Microsoft.Extensions.DependencyInjection** |
| Mapping | AutoMapper |
| Payments | Iyzico (`Iyzipay`) |
| Logging / telemetry | NLog, Serilog, Application Insights **3.x**, OpenTelemetry (OTLP / Azure Monitor) |
| Jobs | Quartz.NET (optional; disable locally with `Quartz_Scheduler_IsEnabled=False`) |
| Front end | jQuery, Bootstrap, **Griddly 3.8** (admin grids), TinyMCE (admin content), FilePond-style media uploads |
| E2E tests | Playwright (Crizal theme against local IIS) |
| Unit tests | MSTest |

---

## Repository layout

```
EImece/                          # Repository root
├── README.md                    # This file
├── DEPLOYMENT.md                # Production CI/CD (GitHub Actions + FTPS)
├── LICENSE                      # Apache License 2.0
├── .github/workflows/           # Deploy Production workflow
├── Playwright/                  # End-to-end tests (Crizal / IIS)
└── EImece/                      # Solution folder
    ├── EImece.sln
    ├── EImece/                  # Web app (Controllers, Views, Areas, Web.config)
    │   ├── Areas/Admin/
    │   ├── Areas/Customers/
    │   ├── Views/Designs/       # Crizal, Modern storefront designs
    │   ├── SqlScripts/          # Seed data, indexes, cleanup
    │   └── media/               # Uploads + logs (needs write access)
    ├── EImece.Domain/           # Domain, data access, services, observability
    ├── EImece.Tests/            # MSTest unit / integration tests
    ├── EImece.MyConsole/        # Maintenance utilities
    ├── Resources/               # Localized strings
    ├── scripts/                 # build.sh, restore-packages.py
    └── docs/                    # Detailed guides
```

| Project | Purpose |
|---------|---------|
| `EImece` | ASP.NET MVC 5 site and Admin / Customers areas |
| `EImece.Domain` | Entities, EF, repositories, services, observability, DI registration |
| `Resources` | Localized strings |
| `EImece.Tests` | MSTest unit and integration tests |
| `EImece.MyConsole` | One-off maintenance utilities |

---

## Getting started

### Prerequisites

| Goal | Requirements |
|------|----------------|
| **Run the site** | Windows 10/11 or Windows Server; Visual Studio 2019/2022 (ASP.NET workload) **or** IIS + .NET Framework 4.8.1; **SQL Server** (Express, Developer, or full) |
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

Or use a gitignored `ConnectionStrings.config` via `configSource`. Full options (local, IIS, Azure, TLS):  
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
| `IyzicoBaseUrl` | Payment API base URL (sandbox or production) |
| `CaptchaProvider` | `Legacy`, `Recaptcha`, or `None` |
| `EnableRequestLogging` | Structured request logging |
| `EnableMetrics` | In-process metrics collection |

reCAPTCHA setup: [EImece/RECAPTCHA.md](EImece/RECAPTCHA.md)

---

## Multi-design storefront

EImece resolves Razor views through a **design-aware view engine**. Set the active design in `Web.config`:

```xml
<add key="ActiveDesign" value="Crizal" />
```

| Design | Description |
|--------|-------------|
| **Crizal** | Default multipurpose responsive e-commerce design |
| **Modern** | Alternate modern e-commerce design system |

Design views live under `EImece/EImece/Views/Designs/{DesignName}/`. Missing design-specific views fall back to shared / default views. Admin and Customers areas are unaffected by `ActiveDesign`.

To remove a design without breaking MSBuild publish, follow [DESIGN_REMOVAL_GUIDE.md](EImece/docs/DESIGN_REMOVAL_GUIDE.md).

---

## Payments (Iyzico)

Checkout uses **Iyzico** for real-time payment (guest and registered users), with confirmation and cargo / tracking support. PCI-sensitive card handling stays with Iyzico's infrastructure.

Implementation uses a **Strategy pattern**:

- `IPaymentStrategy` — payment provider contract
- `IyzicoPaymentStrategy` — Iyzico implementation
- `PaymentContext` — selects and runs the active strategy

Configure Iyzico keys and base URL on the server (environment / server-only config). Do not commit production API keys.

---

## Observability & operations

| Endpoint / artifact | Who | Purpose |
|---------------------|-----|---------|
| `GET /health`, `GET /healthz` | Public | Liveness / dependency checks (SQL, file storage, scheduler) |
| `GET /metrics` | Admin session | In-process counter snapshots |
| `media/logs/EImeceLog.log` | Operators | Plain-text logs |
| `media/logs/EImeceLog.json` | Operators | Structured JSON (CorrelationId, TraceId, SpanId, path) |

OpenTelemetry configuration (OTLP, sampling, Azure Monitor):  
[EImece/docs/OPENTELEMETRY.md](EImece/docs/OPENTELEMETRY.md)

Performance (EF6 queries, SQL indexes, MemoryCache, storefront projections):  
[EImece/docs/PERFORMANCE_AND_CACHING.md](EImece/docs/PERFORMANCE_AND_CACHING.md)

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

The `Playwright/` suite targets a local IIS site (default `http://localhost:81`) for the Crizal theme. It covers cart, checkout, auth, navigation, and responsive checks. It is **not** part of the production deploy workflow.

```bash
cd Playwright
npm install
npx playwright install
npm test
```

---

## Deployment

Production deploy is **manual** via GitHub Actions (`workflow_dispatch`). Pushing to `master` does **not** auto-deploy.

**Pipeline overview** (`.github/workflows/deploy.yml`):

1. Restore NuGet + MSBuild **Release** on `windows-latest`
2. Run MSTest (Helpers + Infrastructure)
3. Publish with the `GitHubActions` FileSystem profile
4. Upload artifact `eimece-production-publish`
5. Optionally FTPS upload + `GET /health` smoke test when `deploy_to_production=true`

Required secrets: `FTP_HOST`, `FTP_USERNAME`, `FTP_PASSWORD`, `FTP_PATH`.  
Optional: `FTP_PORT`, `PRODUCTION_BASE_URL`.

Server-side only (never in the pipeline secrets dump / never in git):

- `EIMECE_DB_CONNECTION_STRING`
- `EIMECE_ENCRYPTION_KEY`
- Iyzico, Application Insights, OTLP, SMTP credentials

Full guide: **[DEPLOYMENT.md](DEPLOYMENT.md)**

---

## Documentation

| Document | Contents |
|----------|----------|
| [DEPLOYMENT.md](DEPLOYMENT.md) | Production CI/CD (Windows MSBuild, FTPS, secrets, rollback) |
| [BUILD_AND_RUN.md](EImece/docs/BUILD_AND_RUN.md) | Build, run, health checks, seed data, tests, common errors |
| [SECURE_CONNECTION_STRINGS.md](EImece/docs/SECURE_CONNECTION_STRINGS.md) | Env vars, `configSource`, TLS, production |
| [OPENTELEMETRY.md](EImece/docs/OPENTELEMETRY.md) | OTLP, sampling, Azure Monitor exporter |
| [LATENCY_PERCENTILES.md](EImece/docs/LATENCY_PERCENTILES.md) | P90/P95/P99 metrics and Admin `/metrics` |
| [PERFORMANCE_AND_CACHING.md](EImece/docs/PERFORMANCE_AND_CACHING.md) | EF6 query tuning, SQL indexes, MemoryCache, storefront projections |
| [ASYNC_AWAIT_GUIDE.md](EImece/docs/ASYNC_AWAIT_GUIDE.md) | Async EF6 / thread-pool guidance |
| [DESIGN_REMOVAL_GUIDE.md](EImece/docs/DESIGN_REMOVAL_GUIDE.md) | Removing a storefront design from the `.csproj` safely |
| [IIS_APP_POOL_PERMISSIONS.md](EImece/docs/IIS_APP_POOL_PERMISSIONS.md) | `media/` ACL for IIS |
| [MEDIA_AND_SEED_IMAGES_GUIDE.md](EImece/docs/MEDIA_AND_SEED_IMAGES_GUIDE.md) | Media management, seed images, and Storefront vs Admin behavior |
| [RECAPTCHA.md](EImece/RECAPTCHA.md) | Captcha providers and Web.config keys |

---

## Contributing

1. Fork the repository and create a feature branch from `master`.
2. Keep secrets out of commits (`ConnectionStrings.config`, real API keys, encryption keys, production connection strings).
3. Prefer small, focused pull requests with a clear description of behavior changes.
4. Build locally (or via `./scripts/build.sh` on Linux) before opening a PR.
5. Add or update tests when changing business logic, payments, or design resolution.
6. **Admin lists:** keep `IndexGrid` **async**; copy an existing `*Grid.cshtml` + `IndexGrid` action. **Do not add Grid.Mvc back.**

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
