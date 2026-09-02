# The Git Log Is the Review: How EImece Went From Hobby to Production in Two Months

**I did not read the diffs. I read the commit messages — after Cursor and Gemini 3.7 had already shipped them.**

If you want the honest history of [EImece](https://github.com/eminyuce/EImece), do not start with the README. Start with `git log`.

From **1 July to 2 September 2026** the repo took **708 commits**. I wrote almost none of the code by hand. I wrote prompts. I ran them in Cursor on Cursor’s models and Gemini 3.7. I did not sit in the diff viewer. I kept building until the site compiled, published to IIS, and a customer could buy something.

The commit comments are what I actually shipped. This article is that log, in English.

---

## What the repo was in July

EImece is an open-source ASP.NET MVC shop: catalog, content, cart, Iyzico checkout, admin, IIS, SQL Server. For years it was a hobby that worked. The early July messages still sound like a hobby.

```
2026-07-12  not fixed
2026-07-22  fix
2026-07-26  Fix
2026-07-26  fix published
2026-07-27  Ngrok fix
```

That is a person poking a live site until the yellow screen goes away.

Then, on **5 July**, the log changes register:

```
Modernize legacy EImece app: security, performance, and reliability fixes
Add enterprise observability: health checks, structured logging, Polly HTTP, metrics
Add cross-platform build support and fix compile errors
Fix Linux solution build: MSTest NuGet, remove COM refs, build script paths
Add BUILD_AND_RUN guide with build, run, and verification steps
```

The modernization commit is specific, not vibes. Parameterized queries in `AppLogRepository`. A MenuService cache that hit the database on cache hits. Path traversal in file delete. Open redirects. Order authorization on thank-you. `AsNoTracking` pagination. Category-tree N+1. Bounded command timeouts.

The observability commit is the first time the hobby starts talking like an operator:

```
Implement aggregated /health and /healthz endpoint with SQL, Redis, RabbitMQ,
external API, file storage, and background service checks
Add Serilog compact JSON logging with correlation ID enrichment
Introduce ResilientHttpClient with Polly retry, circuit breaker, timeout, jitter
Add in-memory application metrics with admin /metrics endpoint
```

A week later those pieces land as real PRs: `#44 cursor/observability-enterprise`, compiler-error restores, a Dependabot AutoMapper bump, then `metrics page` and `health endpoint fix`. July is the turn: still messy subjects, but the work is no longer “add a feature on Sunday.”

---

## Early August: make the old stack hold

**2 August** is the line in the sand I should have drawn years earlier:

```
Upgrade solution from .NET Framework 4.7.2 to 4.8.1.
```

I did not migrate to .NET 8. The commit says what I meant by production: retarget the thing that already ran, align OWIN/Web API and tests, keep IIS.

**3 August** is a security week written as conventional commits, most of them `cursor/` branches I never line-reviewed:

```
Remove hard-coded SQL credentials from config (CWE-798)
Fix reflected and stored XSS from unsafe Html.Raw usage
Fix critical hard-coded encryption key and fixed IV vulnerabilities
Add optional reCAPTCHA with Legacy captcha backward compatibility
Upgrade Application Insights from 2.1.0 to 3.1.2
Migrate ASP.NET MVC DI from Ninject to Microsoft.Extensions.DependencyInjection
```

Same day, the DI commit replaces `NinjectWebCommon` with an MS.DI composition root and keeps the old property injection working so the app does not die on first request. That is the pattern for the whole summer: **do not rewrite the product. Change the load-bearing parts and keep shipping.**

**4 August** the admin stops looking like 2014:

```
Redesign admin area with fixed left sidebar navigation
Localize admin left menu labels to Turkish
Admin auth is on again: BypassAdminAuth is set to false in Web.config
```

I had turned auth off to smoke-test the sidebar. The next commit turns it back on. The log is more honest than I am. It records the shortcut and the correction.

**5–7 August** the subjects start sounding like a product owner with a checklist:

```
Add production-grade OpenTelemetry instrumentation for net481 MVC.
Remove unused code, orphan views, and dead static assets
Enhance Admin exports with Excel/CSV choice and NPOI formatting.
Localize admin Grid.Mvc column headers to Turkish.
Hide price-related pages when IsProductPriceEnable is false
Fix Lighthouse mobile performance and accessibility regressions.
Harden payment callbacks, admin CSRF, and auth bypass gates.
```

---

## Mid August: themes, money, and the compiler as reviewer

**8–10 August** the storefront becomes a design system instead of one Razor tree:

```
Add multi-design Razor view architecture plan
feat: implement fixed production Razor designs with strict no view fallback
Add Crizal Razor theme with visual audit fixes
Replace CKEditor with TinyMCE
Add TOTP authenticator 2FA for admin login
Convert public (non-admin) site to end-to-end async/await
Convert Admin area controllers to async (no business logic changes)
Make Admin panel fully usable on modern phones
Add production CI/CD pipeline (Windows MSBuild + manual FTPS)
```

“No business logic changes” is doing a lot of work in that async PR. I was not asking the model for a new shop. I was asking it to stop blocking threads.

**11–14 August** is checkout and data access, which is where hobby shops usually lie to themselves:

```
keep PaymentContext Strategy and PlaceOrder init resilience
Fix QA critical/high bugs: contact NRE, add-to-cart, auth/config, redirects
A storefront request must retrieve the minimum data required to render that request
no full entity + Include on any public storefront path. Only projections + AsNoTracking
Add an admin test-email environment for mail templates.
Load the SQL connection string from a parent-folder config file so IIS publish cannot delete credentials.
```

That last one is a production scar. IIS publish had been wiping secrets. The commit comment is the postmortem.

**17 August** is the day the admin stack actually moved:

```
feat(media): replace blueimp/jQuery-File-Upload with FilePond
Migrate Admin Products Grid from Grid.Mvc to Griddly
feat(admin): complete full migration from Grid.Mvc to Griddly across all admin modules
chore(admin): migrate Admin markup and CSS to Bootstrap 5.3.8
chore(storefront): switch Crizal, Modern, and shared layouts to Bootstrap 5.3.8
ASP.NET MVC 5 - Precompiled Deployment / IIS Publish steps
```

Then a dozen polish commits that only make sense if you are staring at the live grid, not the source: floating scrollbars, sticky columns, “Total Records” chrome, IIS child-action sync bridges, `Name filter binding`. I was not reviewing C#. I was clicking IIS at `localhost:81` until the grid behaved.

The recurring subject through August and September is not a feature. It is a gate:

```
aspnet_compiler command
Add example PowerShell command for aspnet_compiler.exe
fix(views): resolve Razor compilation errors and precompilation issues
```

MSBuild can be green and Razor can still be red. `aspnet_compiler` became the code review I refused to do by eye.

---

## Late August: DTOs, IIS, and taking Domain off System.Web

**20–22 August** the log stops talking about screens and starts talking about boundaries:

```
feat(settings): migrate high-value Web.config settings to Settings table with DB-first fallback
refactor: wrap ApplicationDbContext and encapsulate access in the service layer
fix: resolve concurrency, blocking I/O, and sync-over-async issues
Refactor ViewModels to use DTOs and verify IIS deployment with Playwright E2E tests
perf: exhaustive DTO projection refactor across storefront/customers
security: remove auth bypasses, 2FA bypass list, and ExposeDetailedErrors
fix(core): atomic checkout transactions, repository isolation levels, async NLog targets
```

“Verify IIS deployment with Playwright” is the method in one subject line. The model changes the code. The proof is the deployed site, not my opinion of the PR.

**25–29 August** is merchant software:

```
Advanced coupon management + enforce layered architecture (no DbContext in services)
feat(admin): add user audit reports
feat(e2e): add Chromium-only iyzico sandbox E2E suite + shopping guide
feat(observability): add Castle TimedInterceptor + ProxyFactory
feat(observability): add in-memory PerfStats metrics and admin dashboard
feat(admin): premium System Settings center redesign — Stripe/Linear inspired
feat(admin): replace left nav with a mega menu
```

Two SonarQube PRs land the same week. I still did not read the diffs. I merged `#153` and `#154` because the subjects said critical/major findings were addressed, then I hit the site again.

**30–31 August** is the architectural sentence I had been circling since July:

```
feat(di): standardize pure constructor dependency injection across entire solution
refactor(domain): move MVC types out of Domain so it no longer depends on System.Web
feat(web): add EImece.Web library for shared MVC infrastructure
Migrate application logging to ILogger<T> with media/logs default
feat(infra): add MEL Options, IHttpClientFactory, and shared IMemoryCache
docs: rewrite README for current three-project architecture
```

A hobby MVC app keeps `System.Web` in the domain because it is convenient. A production one stops. The commit messages are blunt about the cost: `resolve compile errors after Domain MVC decoupling`, `remaining Domain MVC decoupling compile and IIS runtime wiring`. That is not a clean-room refactor. That is an agent breaking the build and gluing IIS back together until `/health` answers.

---

## September: cache, stress, and C# 5 because IIS said so

**1 September** the log sounds like someone who already has customers, even if the only customer is still me:

```
feat(infra): migrate to Microsoft.Extensions packages (HealthChecks, Http.Polly, Configuration.Json, Localization, ObjectPool)
feat(admin): add interactive accordion breakdown and rich telemetry data to system health page
test(perf): add automated stress testing and telemetry suite
perf(cache): configure outputCacheProfiles to location=Server
feat(cache): add automated background cache warmup service
perf(http-cache): optimize HTTP caching, ETags, and security headers for multi-server production
fix(hosting): remove system.codedom to prevent Roslyn csc.exe group policy block on shared IIS/Plesk
```

Then four commits in a row that only exist because the host is not Visual Studio:

```
fix(views): ensure C# 5 compatibility for Razor views
fix(views): replace C# 6 null-conditional operators with C# 5 syntax
fix(admin-views): replace C# 6 string interpolations with string.Format
fix(breadcrumbs): replace tuple parameter syntax with C# 5 compatible overloads
```

I shipped modern infrastructure and then downgraded the view language so shared IIS/Plesk would compile the site. That is production. The commit comments do not romanticize it.

**2 September** I finally checked the prompts into the repo. The code had already been there for weeks. The briefs were the missing artifact.

---

## What 708 commit messages taught me

The log has three voices.

**1. The hobby voice.** `fix`, `Fix`, `not fixed`, `exceptions`, `Cursor FIxes`. July still has this. It never fully dies. When the agent is stuck, I still type `fix` and push.

**2. The prompt voice.** Conventional subjects that are really specs: `no full entity + Include on any public storefront path`, `Convert Admin area controllers to async (no business logic changes)`, `Admin auth is on again`. Those lines are the review. I wrote them before the model ran, or I accepted them after it named the work.

**3. The IIS voice.** `aspnet_compiler`, `Precompiled Deployment / IIS Publish`, `IIS publish cannot delete credentials`, `Roslyn csc.exe group policy block`, `C# 5 compatibility for Razor views`. This voice wins arguments. If the deployed shop is down, the elegant refactor did not happen.

I used Cursor as the factory and Gemini 3.7 when I wanted a model that would stay inside a long brief. I did not use them as a rubber duck. I used them as a build crew. The artifact I trusted was not my memory of the code. It was:

- a green solution build
- a clean `aspnet_compiler` run
- IIS at `localhost:81`
- Playwright on the real storefront, including Iyzico sandbox
- `/health` not lying
- a sitemap URL that did not 500
- a commit subject I could still explain in a month

That last one is why this article is a git log. A chat thread dies. A subject line on `master` is the product.

---

## I still did not review the code

I will say it plainly. I did not read 708 diffs.

I read failing pages. I read yellow screens. I read compiler output. I read Playwright screenshots. I read stress-test numbers. When something was wrong I wrote the next prompt, or I accepted the next commit message and kept going.

That is reckless if you pretend it is a substitute for a security audit. It is a valid way to drag a one-person ASP.NET hobby across the line into something you can operate — if you let the host and the browser veto the model.

The PRs even advertise the method in their branch names: `cursor/admin-sidebar-layout-3411`, `cursor/opentelemetry-instrumentation-9079`, `cursor/storefront-data-access-optimization-3805`, `cursor/domain-mvc-decoupling-finish-56c9`. Cursor did the typing. The commit comment is what I was willing to put on `master`.

---

## If you read only the log

From July to today, EImece’s `git log` says this:

- July: health, metrics, structured logs, a Linux build, a guide — and a lot of `fix`.
- 2 August: .NET 4.8.1, not a rewrite.
- 3 August: secrets, XSS, encryption, Ninject out.
- 4 August: a real admin shell, auth turned back on.
- Mid August: Crizal, 2FA, TinyMCE, async, FilePond, Griddly, Bootstrap 5, projections.
- Late August: settings in the database, DTOs at the view, Domain off `System.Web`, `ILogger<T>`, Playwright on IIS.
- September: cache warmup, stress tests, and C# 5 views because the server said so.

The prompts I used are in the repo now: [docs/prompts](https://github.com/eminyuce/EImece/tree/master/docs/prompts).

The review is `git log --since=2026-07-01`.

---

*Emin Yüce builds [EImece](https://github.com/eminyuce/EImece), an open-source ASP.NET MVC e-commerce platform. Support the project on [Buy Me a Coffee](https://buymeacoffee.com/eminyuce).*
