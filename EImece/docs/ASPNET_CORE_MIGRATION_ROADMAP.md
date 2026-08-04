# EImece — ASP.NET MVC 5 → ASP.NET Core 8 Migration Roadmap

**Status:** Phase 1 complete (assessment only — no runtime code changes)  
**Source stack:** ASP.NET MVC 5 / .NET Framework 4.8.1 / EF6 / MS.DI  
**Target stack:** ASP.NET Core 8 LTS / EF Core 8 / ASP.NET Core Identity / PackageReference / Minimal Hosting  
**Principle:** Incremental migration; preserve business behavior; keep Microsoft.Extensions.DependencyInjection; do not reintroduce Ninject.

---

## 1. Phase 1 objectives

1. Map the current solution architecture and project dependencies.  
2. Identify migration risks and non-portable APIs.  
3. Produce a phased, reviewable migration roadmap.  
4. Define decisions and trade-offs before any code modernization begins.

---

## 2. Current architecture summary

### 2.1 Solution projects (all `net481`)

| Project | Role | References |
|---------|------|------------|
| `EImece` | ASP.NET MVC 5 web app (storefront + Admin + Customers areas) | `EImece.Domain`, `Resources` |
| `EImece.Domain` | Entities, EF6 DbContexts, repositories, services, observability, DI helpers | `Resources` |
| `Resources` | Localized `.resx` (storefront + admin) | — |
| `EImece.Tests` | MSTest | `EImece`, `EImece.Domain` |
| `EImece.MyConsole` | Maintenance / migration utilities | `EImece`, `EImece.Domain` |

```
Resources
   ↑
EImece.Domain
   ↑
EImece  ←── EImece.Tests
   ↑
EImece.MyConsole
```

### 2.2 Entry points

| Component | Path | Notes |
|-----------|------|-------|
| Classic ASP.NET startup | `EImece/Global.asax.cs` | Areas, filters, routes, bundles, DI, request scopes, optional admin auth bypass |
| OWIN startup | `EImece/Startup.cs` + `App_Start/Startup.Auth.cs` | Cookie auth + external OAuth providers |
| DI composition root | `App_Start/DependencyInjectionConfig.cs` | `ServiceCollection` → `BuildServiceProvider(validateScopes: true)` |
| Routing / bundling / Web API | `RouteConfig`, `BundleConfig`, `WebApiConfig`, `FilterConfig` | SEO lowercase routes; System.Web.Optimization bundles |
| Observability | `ObservabilityBootstrap.cs` | NLog / Serilog / Application Insights / OpenTelemetry |

### 2.3 Dependency injection (already modernized)

- **Ninject removed** from packages and active code (only `.vshistory` remnants).  
- Container: `Microsoft.Extensions.DependencyInjection` **10.0.10**.  
- MVC + Web API resolvers: `MsDiDependencyResolver`, `MsDiWebApiDependencyResolver`.  
- Custom `[Inject]` attribute + `PropertyInjector` (~191 call sites) — property injection is still the dominant controller/base pattern.  
- Lifetimes: scoped DbContexts/repos/services; singleton cache, logging, observability, Quartz scheduler factory.

**Decision for Core:** Keep MS.DI as the sole container. Prefer constructor injection going forward; temporarily port `PropertyInjector` only where needed to avoid a big-bang controller rewrite.

### 2.4 Data layer

| Item | Detail |
|------|--------|
| ORM | Entity Framework **6.5.2** |
| Business context | `EImeceContext` — ~35 `IDbSet<>` |
| Identity context | `ApplicationDbContext` : `IdentityDbContext<ApplicationUser>` |
| Connection | `EImeceDbConnection` + env override `EIMECE_DB_CONNECTION_STRING` |
| Pattern | Generic repository + concrete repositories + service layer (no formal Unit of Work) |
| Entities | ~39 entity types (Product, Order, Brand, ShoppingCart, Story, FileStorage, Setting, …) |

### 2.5 Authentication & authorization

- ASP.NET Identity **2.2.4** + OWIN cookie auth (`Microsoft.Owin.*` 4.2.3).  
- Roles: `Admin`, `NormalUser` (editor), `Customer`.  
- Areas: `Admin` (role-gated), `Customers` (customer role).  
- External logins: Google / Facebook / Twitter / Microsoft (config-gated).  
- Debug: `BypassAdminAuth` can attach an admin principal on `/admin`.

### 2.6 Application surface

| Area | Scale / notes |
|------|----------------|
| Controllers | ~53 (19 root, 33 Admin, 1 Customers) + 1 Web API controller |
| Views | ~283 `.cshtml` |
| Static assets | Large `Content/` + `Scripts/` tree (Bootstrap 3 / mstore theme) |
| Features | Catalog, brands, cart, Iyzico checkout, CMS/stories, SEO/sitemap/RSS, image proxy, localization, admin CRUD, health/metrics |
| Background jobs | Quartz present but **disabled** (`Quartz_Scheduler_IsEnabled=False`) |
| Localization | `Resources` + `AdminResource` resx; culture cookies `_culture` / `_adminCulture` |

---

## 3. Target architecture

| Concern | Target |
|---------|--------|
| Runtime | .NET **8 LTS** |
| Web | ASP.NET Core MVC (Areas preserved) |
| Host | Minimal hosting (`Program.cs`) |
| Data | EF Core 8 + SQL Server |
| Identity | ASP.NET Core Identity (cookie + optional external providers) |
| DI | Microsoft.Extensions.DependencyInjection (unchanged philosophy) |
| Config | `appsettings.json` + environment variables + Options pattern |
| Packages | SDK-style projects + PackageReference |
| Platform | Windows and Linux |

### Proposed project layout (Phase 2+)

```
EImece.sln
├── src/
│   ├── EImece.Web/              # ASP.NET Core MVC host
│   ├── EImece.Domain/           # Entities, repositories, services (net8)
│   └── EImece.Resources/        # Resx / localization
├── tests/
│   └── EImece.Tests/            # xUnit or MSTest on net8
└── tools/
    └── EImece.MyConsole/        # Optional maintenance tool
```

**Trade-off:** Renaming/moving into `src/` improves clarity but creates a large mechanical diff. Alternative: convert in place (`EImece` → Web SDK project) to minimize path churn, then reorganize later. **Recommendation:** convert in place first (Phase 2), reorganize folders only if it stays reviewable.

---

## 4. Package replacement matrix (high impact)

| Current | Issue on Core | Recommended replacement |
|---------|---------------|-------------------------|
| EF6 | Limited / not preferred | **EF Core 8** |
| ASP.NET Identity 2 + OWIN | System.Web / OWIN stack | **ASP.NET Core Identity** |
| `System.Web.Optimization` | Not available | LibMan / Vite / plain static files + optional bundler |
| ImageProcessor + System.Drawing | Windows GDI+ | **SkiaSharp** (already referenced) or ImageSharp |
| RazorEngine 3 | Framework Razor host | **RazorLight** or Fluid for email/RSS templates |
| Grid.Mvc / MVCGrid.Net | MVC5 + `.axd` | DataTables / custom tag helpers (admin UX change) |
| OpenTelemetry AspNet / AI Web | HttpModule | AspNetCore instrumentation packages |
| `System.Linq.Dynamic` | Old | `System.Linq.Dynamic.Core` |
| TidyManaged | Native tidy | HtmlAgilityPack / AngleSharp |
| SmtpClient usage | Obsolete patterns | MailKit |
| Iyzipay 2.1.78 (`net45`) | Verify Core support | Upgrade Iyzipay package or thin HttpClient wrapper |

---

## 5. Top migration risks

| # | Risk | Severity | Mitigation |
|---|------|----------|------------|
| 1 | EF6 → EF Core model / query / transaction differences | Critical | Side-by-side schema validation; migrate repositories incrementally; keep SQL Server; add EF Core migrations after parity checks |
| 2 | Identity 2 + OWIN → Core Identity (cookies, 2FA, external logins, roles) | Critical | Preserve AspNet* tables where possible; end-to-end auth tests for Admin/Customer/payment |
| 3 | Imaging pipeline (Drawing / ImageProcessor / captcha / `/images/{size}/{id}`) | Critical | Replace with SkiaSharp before Linux run; golden-image tests for resize/WebP |
| 4 | Domain coupled to `HttpContext.Current` (~88 usages) | Critical | Introduce `IHttpContextAccessor` / URL generators; stop URL building inside entities |
| 5 | Property injection (`[Inject]` × ~191) | High | Port injector for BaseAdminController short-term; migrate controllers to ctor injection over time |
| 6 | ~283 Razor views + Areas + helpers | High | Keep MVC; migrate layouts/partials first; Tag Helpers where low-risk |
| 7 | Iyzico checkout / encrypted callbacks | High | Freeze payment behavior; regression suite for PlaceOrder / PaymentResult / guest checkout |
| 8 | RazorEngine email/RSS templating | High | Swap renderer early in infrastructure phase |
| 9 | Admin grids (Grid.Mvc / MVCGrid) | High | Choose one Core-friendly grid strategy before Admin CRUD migration |
| 10 | Web.config → appsettings + middleware | Medium–High | Map `AppConfig` to Options; replace HttpModules with middleware |

### Cross-platform blockers (must clear for Linux)

- System.Drawing / ImageProcessor  
- `Server.MapPath` / `HostingEnvironment` → `IWebHostEnvironment.ContentRootPath` / `IFileProvider`  
- IIS modules/handlers → middleware  
- Full-trust / `TrySkipIisCustomErrors` assumptions  

---

## 6. Phased plan (approval gate after each phase)

### Phase 1 — Architecture assessment ✅ (this document)

**Deliverables:** architecture map, risks, package matrix, roadmap.  
**Code changes:** none (documentation only).

### Phase 2 — Solution modernization

**Objectives**

- Create/convert SDK-style projects targeting `net8.0`.  
- Move from `packages.config` to PackageReference.  
- Establish Minimal Hosting skeleton (`Program.cs`) that boots with health endpoint.  
- Keep legacy projects buildable or clearly marked until cutover (prefer parallel Core host if needed).

**Required changes**

- Convert `Resources`, `EImece.Domain`, web host to SDK-style.  
- Add `EImece.Web` (or convert `EImece`) as `Microsoft.NET.Sdk.Web`.  
- Stub DI registration without pulling System.Web.  
- Baseline `appsettings.json` for connection string + feature flags.

**Risks**

- Domain still references System.Web / EF6 — Phase 2 may need temporary `net8.0` multi-target or a thin web skeleton that does not yet compile all Domain code.  
- **Trade-off:** (A) Parallel Core host + leave legacy intact vs (B) in-place conversion that won’t fully compile until later phases. **Recommend A for safety.**

**Exit criteria**

- `dotnet build` succeeds for the new Core solution skeleton.  
- App starts and answers `/health` (even if business features are not wired).

### Phase 3 — Domain and data layer

- EF Core `EImeceContext` + Identity `ApplicationDbContext`.  
- Fluent configurations for key relationships.  
- Repository/service compile against EF Core APIs.  
- Initial EF Core migrations (or scaffold from existing DB).  
- Preserve connection-string env override behavior.

### Phase 4 — Infrastructure

- Options pattern replacing static `AppConfig` / `ConfigurationManager`.  
- Logging (NLog.Extensions.Logging or Serilog.AspNetCore).  
- Caching (`IMemoryCache` / existing LazyCache adapter).  
- File providers for `~/media`.  
- Hosted services for Quartz (optional, still off by default).  
- Polly / HttpClientFactory for outbound HTTP.

### Phase 5 — Authentication and security

- ASP.NET Core Identity registration.  
- Cookie auth parity with current login paths.  
- Role policies for Admin / NormalUser / Customer.  
- External providers as configured.  
- Security headers middleware (replace HttpModule).  
- Data protection keys for Linux-friendly persistence.

### Phase 6 — Application layer

- Controllers → ASP.NET Core MVC (Areas).  
- Filters → attributes / endpoint filters / middleware.  
- Routing parity (SEO product/story/sitemap routes).  
- Model binding / validation.  
- Preserve service-layer business rules.

### Phase 7 — Presentation layer

- Views/layouts/partials with minimal markup changes.  
- Static files middleware; retire BundleConfig (or replace with modern bundling).  
- Tag Helpers where beneficial.  
- Localization via resx + request culture middleware.

### Phase 8 — Integrations

- Iyzico payment flow end-to-end.  
- Email (MailKit + new template engine).  
- Image upload/resize pipeline (SkiaSharp).  
- Background jobs if re-enabled.  
- Captcha / WebPush / Excel (NPOI) as applicable.

### Phase 9 — Testing, optimization, deployment

- Functional verification checklist (catalog → cart → checkout → admin).  
- Performance (EF queries, image cache, response compression).  
- Security review (secrets, cookies, CSRF, headers).  
- Deployment guidance (Kestrel + reverse proxy; Windows/Linux).  
- Final cleanup of legacy System.Web artifacts.

---

## 7. Architectural decisions (locked for now)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| ASP.NET Core version | **.NET 8 LTS** | Stable LTS; matches stated preference |
| UI style | **MVC + Areas** (not Razor Pages) | Lowest behavioral churn for ~283 views / Admin area |
| DI | **MS.DI only** | Already adopted; avoid second IoC |
| ORM | **EF Core 8** (not EF6-on-Core long term) | Cross-platform + long-term maintainability |
| Migration style | **Incremental phases with approval gates** | Reviewable diffs; safer for commerce |
| Hosting model | **Minimal hosting** | Current ASP.NET Core standard |
| Imaging | **SkiaSharp-first** | Already in solution; Linux-capable |
| Grids | **Defer replacement design to Phase 6/7** | Admin UX impact; needs product choice |

---

## 8. Phase 1 testing checklist

Phase 1 is documentation-only. Verify:

- [ ] Solution projects and dependency graph match this document.  
- [ ] Ninject is absent from active `packages.config` / DI code.  
- [ ] MS.DI composition root is documented accurately.  
- [ ] Dual DbContexts (`EImeceContext`, `ApplicationDbContext`) confirmed.  
- [ ] Top risks include imaging, Identity, EF Core, HttpContext coupling, Iyzico.  
- [ ] Phase boundaries are clear; no Phase 2+ code changes in this PR.  
- [ ] Stakeholder approval recorded before Phase 2 starts.

---

## 9. What was completed in Phase 1

- Full architecture assessment of the EImece MVC 5 solution.  
- Inventory of projects, entry points, DI, data, auth, controllers/areas, config, packages, views, jobs.  
- Cross-platform and package-replacement risk analysis.  
- Detailed Phase 2–9 roadmap with objectives, changes, risks, and exit criteria for Phase 2.  
- Explicit architectural decisions for .NET 8 LTS + MVC + EF Core + MS.DI.

---

## 10. Approval gate

**Stop here.** Do not begin Phase 2 (SDK-style / Core host scaffolding) until this roadmap is approved.

When approving, please confirm or adjust:

1. Parallel Core host vs in-place conversion (recommendation: **parallel Core host**).  
2. Keep project names/paths vs introduce `src/` layout (recommendation: **in-place names first**).  
3. Admin grid strategy preference (DataTables vs commercial vs custom) — can decide in Phase 6 if needed.  
4. Whether Quartz should remain disabled through migration (recommendation: **yes**).

Reply with approval (and any overrides) to proceed to **Phase 2 — Solution modernization**.
