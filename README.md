# EImece — ECommerce Web Application

**EImece** is an open-source, full-featured eCommerce web application built with **ASP.NET MVC 5**, **Entity Framework 6**, and **Microsoft.Extensions.DependencyInjection**. It uses the **Repository Pattern** and a **Service Layer** for clear separation of concerns.

> The solution **compiles on Linux** (CI). The web app **runs on Windows** with IIS / IIS Express and SQL Server.

---

## Technologies

| Area | Stack |
|------|--------|
| Runtime | .NET Framework **4.8.1** |
| Web | ASP.NET MVC **5.3**, ASP.NET Identity, OWIN |
| Data | Entity Framework **6.5**, SQL Server |
| DI | **Microsoft.Extensions.DependencyInjection** |
| Payments | [Iyzico](https://www.iyzico.com/en) (`Iyzipay`) |
| Logging / telemetry | NLog, Serilog, Application Insights **3.x**, OpenTelemetry (OTLP / Azure Monitor) |
| Jobs | Quartz.NET (optional) |
| Front end | jQuery, Bootstrap |

---

## Highlight: Iyzico payments

- Real-time checkout via **Iyzico** (guest and registered users)
- Payment confirmation and cargo / tracking numbers
- PCI-DSS–compliant payment infrastructure through Iyzico

---

## Key features

### Storefront
- Banner carousels, custom menus, and themed content pages
- Product categories, tags, brands, galleries, and filters (price, rating, brand)
- Cart, guest/member checkout, order tracking
- Contact form with email and WhatsApp support
- Optional **Google reCAPTCHA v2** (legacy arithmetic captcha still available)

### Admin
- Customers, orders, cargo numbers, status, and internal notes
- Media library management
- Product FAQs visible on the customer account page
- Price updates by category, tag, or brand

### Operations & security
- Health endpoints: `GET /health` and `GET /healthz`
- Admin metrics: `GET /metrics` (authenticated administrators)
- OpenTelemetry traces/metrics (OTLP primary; optional Azure Monitor exporter)
- Security response headers (`SecurityHeadersHttpModule`)
- DB credentials and encryption keys via environment variables (not committed secrets)
- Structured request logging under `media/logs/` with CorrelationId / TraceId / SpanId

---

## Solution structure

| Project | Purpose |
|---------|---------|
| `EImece` | ASP.NET MVC 5 site and Admin area |
| `EImece.Domain` | Entities, EF, repositories, services, observability, DI |
| `Resources` | Localized strings |
| `EImece.Tests` | MSTest unit / integration tests |
| `EImece.MyConsole` | One-off maintenance utilities |

```
EImece/
├── EImece/                 # Web app (Controllers, Views, Areas/Admin, Web.config)
├── EImece.Domain/          # Domain, data access, services
├── EImece.Tests/
├── EImece.MyConsole/
├── Resources/
├── scripts/                # build.sh, restore-packages.py
└── docs/                   # Detailed guides
```

---

## Getting started

### Prerequisites

- **Windows** to run the site (Visual Studio 2019/2022 with ASP.NET workload, or IIS + .NET Framework 4.8.1)
- **SQL Server** (Express, Developer, or full)
- To **build** only (Windows or Linux): .NET SDK 8+ and Python 3

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

Or use a gitignored `ConnectionStrings.config`. Full options (local, IIS, Azure, TLS):  
[EImece/docs/SECURE_CONNECTION_STRINGS.md](EImece/docs/SECURE_CONNECTION_STRINGS.md)

Encryption secrets: prefer `EIMECE_ENCRYPTION_KEY` over storing keys in `Web.config`.

### 3. Build

**Visual Studio:** open `EImece/EImece.sln` → Rebuild Solution.

**Command line (Windows Developer Prompt):**

```powershell
cd EImece
nuget restore EImece.sln
msbuild EImece.sln /t:Clean,Build /p:Configuration=Release
```

**Linux / CI:**

```bash
cd EImece
chmod +x scripts/build.sh
./scripts/build.sh
```

### 4. Run

1. Set **EImece** as the startup project.
2. Press **F5** (or host the `EImece/EImece` folder in IIS).
3. Default IIS Express URL is typically `http://localhost:31544` (check project Web properties if different).
4. Confirm: `GET http://localhost:31544/health` → HTTP 200 and `"Status": "UP"`.

Step-by-step build, IIS, verification, and troubleshooting:  
[EImece/docs/BUILD_AND_RUN.md](EImece/docs/BUILD_AND_RUN.md)

### Optional: reCAPTCHA

See [EImece/RECAPTCHA.md](EImece/RECAPTCHA.md) to switch between Legacy captcha and Google reCAPTCHA v2.

---

## Documentation

| Doc | Contents |
|-----|----------|
| [DEPLOYMENT.md](DEPLOYMENT.md) | Production CI/CD (Windows MSBuild, FTPS, secrets, rollback) |
| [BUILD_AND_RUN.md](EImece/docs/BUILD_AND_RUN.md) | Build, run, health checks, tests, common errors |
| [SECURE_CONNECTION_STRINGS.md](EImece/docs/SECURE_CONNECTION_STRINGS.md) | Env vars, `configSource`, TLS, production |
| [OPENTELEMETRY.md](EImece/docs/OPENTELEMETRY.md) | OTLP, sampling, Azure Monitor exporter |
| [PERFORMANCE_AND_CACHING.md](EImece/docs/PERFORMANCE_AND_CACHING.md) | EF6 query tuning, SQL indexes, MemoryCache strategy |
| [RECAPTCHA.md](EImece/RECAPTCHA.md) | Captcha providers and Web.config keys |

---

## Contributing

Fork the repo, create a branch, and open a pull request. Keep secrets out of commits (`ConnectionStrings.config`, real API keys, encryption keys).

---

## License

MIT — see [LICENSE](LICENSE).
