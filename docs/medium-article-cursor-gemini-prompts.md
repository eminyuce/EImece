# From July to Today: How 200+ Commits, Cursor, and Gemini 3.7 Turned a Legacy .NET Hobby Into Production

**The unvarnished, commit-by-commit timeline of transforming [EImece](https://github.com/eminyuce/EImece) from an old ASP.NET MVC hobby project into a hardened, production-grade e-commerce engine without me reviewing a single line of code.**

---

## The Confession

Let’s get the uncomfortable truth out of the way immediately: **I did not sit down and review the code line-by-line.**

Not when refactoring legacy synchronous controllers to asynchronous endpoints. Not when migrating from Ninject to Microsoft Dependency Injection. Not when splitting the monolithic domain layer into a clean 3-tier architecture. Not even when replacing payment gateways with the Strategy Pattern or configuring OpenTelemetry traces.

In software engineering, manual line-by-line code review is treated as the sacred shield preventing production disasters. But between **July 2026 and September 2026**, I ran a deliberate, real-world experiment with **Cursor** and **Gemini 3.7**:

> **What happens if you stop acting as a human code reviewer and start acting purely as an Engineering Director and Lead Architect who steers AI models through rigorous prompt contracts and automated machine verification gates?**

If you look at the git log of [**EImece**](https://github.com/eminyuce/EImece), you don't see hand-typed artisan diffs. You see a raw, high-velocity record of 200+ commits showing how an old .NET monolith was transformed into an enterprise-ready system.

Here is the month-by-month journey from July to today.

---

## July 2026: The Awakening & Baseline Resuscitation

In early July, EImece was an old, working e-commerce codebase (.NET Framework 4.7.2, Entity Framework 6, ASP.NET MVC 5) that had sat in the "hobby drawer" for years. The goal in July was simply to bring it back to life, fix broken builds, and add modern foundational capabilities.

### The First Steps: Resuscitating the Build
```git
commit 77aaa9ae  2026-07-05  Modernize legacy EImece app: security, performance, and reliability fixes
commit 7cadffbb  2026-07-05  Add enterprise observability: health checks, structured logging, Polly HTTP, metrics
commit df72aa1c  2026-07-05  Fix Linux solution build: MSTest NuGet, remove COM refs, build script paths
commit f540f40d  2026-07-12  Bump AutoMapper from 10.1.1 to 15.1.3
```

The initial commits in July focused on getting the solution to build cleanly in modern CI environments (including Linux runner validation), updating obsolete NuGet dependencies (AutoMapper, jQuery), and wiring initial health endpoints.

### The "Messy Middle" of Early AI Prompting
In mid-July, before I developed a structured prompt system, the git log reveals the classic stumbling blocks of unstructured AI assistance:

```git
commit f9e9f137  2026-07-12  not fixed
commit d87a59f5  2026-07-22  fix
commit 945c0419  2026-07-22  fix
commit 449085cd  2026-07-23  health endpoint fix
commit 5ca6256a  2026-07-26  fix
commit 533725c5  2026-07-27  Ngrok fix
```

Those commit messages (`"fix"`, `"not fixed"`, `"fix published"`) tell a crucial story: **vague prompts produce vague results.** When you ask an AI model to simply "fix the health endpoint," you get quick patches that break under edge cases.

By late July, I realized that if this was going to work at enterprise scale, I needed to change the rules: **every prompt had to be a complete, self-contained specification with strict constraints and clear acceptance criteria.**

---

## Early August 2026: Runtime Upgrades & Security Hardening

In the first week of August, the prompt engineering strategy matured. We tackled framework upgrades and critical security debt in rapid succession.

### 1. Upgrading the Runtime & Dependency Injection (Aug 1–4)
```git
commit a94af83d  2026-08-02  Upgrade solution from .NET Framework 4.7.2 to 4.8.1
commit 140c17b3  2026-08-03  Migrate ASP.NET MVC DI from Ninject to Microsoft.Extensions.DependencyInjection
commit d3a7c23d  2026-08-03  Fix MS.DI circular property injection duplicate-key crash
commit fc62949c  2026-08-04  Improve DI: circular deps, ctor injection, error handling
```
Upgrading from Ninject to `Microsoft.Extensions.DependencyInjection` on .NET 4.8.1 initially triggered circular dependency resolution errors. Instead of manually debugging object graphs, the error stack trace was fed back into Gemini 3.7, which resolved circular dependencies through clean constructor refactoring.

### 2. Eliminating Critical Security Vulnerabilities (Aug 3–6)
```git
commit 1828893d  2026-08-03  Remove hard-coded SQL credentials from config (CWE-798)
commit c6fbb4fd  2026-08-03  Fix reflected and stored XSS from unsafe Html.Raw usage
commit bff328d3  2026-08-03  Fix critical hard-coded encryption key and fixed IV vulnerabilities
commit 03df19f1  2026-08-03  Add optional reCAPTCHA with Legacy captcha backward compatibility
commit 623690fc  2026-08-06  Harden payment callbacks, admin CSRF, and auth bypass gates
```
A comprehensive security prompt audited the codebase for OWASP Top 10 vulnerabilities. In one afternoon, hardcoded cryptographic keys were replaced with secure PBKDF2/AES routines, SQL credentials were moved to environment overrides, and unsafe `Html.Raw` outputs were sanitized.

### 3. Production Observability & Admin UX (Aug 4–6)
```git
commit fdcc2282  2026-08-03  Redesign admin area with fixed left sidebar navigation
commit 9711a89a  2026-08-05  Add production-grade OpenTelemetry instrumentation for net481 MVC
commit f259f86a  2026-08-06  Enhance Admin exports with Excel/CSV choice and NPOI formatting
commit d0e31cb5  2026-08-06  Localize admin Grid.Mvc column headers to Turkish
```
OpenTelemetry was integrated across HTTP requests, SQL queries, and background tasks. The outdated admin top-bar layout was converted into a sleek, responsive sidebar navigation with localized Turkish labels and NPOI-powered Excel/CSV exports.

---

## Mid August 2026: Clean Architecture & Payment Strategy

By mid-August, we tackled the deeper architectural smells that plague monolithic MVC applications.

### 1. The Great Domain-MVC Decoupling (Aug 15–20)
In old .NET projects, domain entities often reference `System.Web` types, making headless execution impossible. We instructed Gemini 3.7 to execute a clean separation of concerns:

```git
commit a6fd7eec  2026-08-20  refactor(domain): move MVC types out of Domain so it no longer depends on System.Web
commit d80f2db5  2026-08-20  feat(web): add EImece.Web library for shared MVC infrastructure
commit acc98fbb  2026-08-20  refactor(domain): extract IHttpRuntimeCacheClearer from ApplicationCacheClearer
commit ab09521b  2026-08-20  fix(web): remaining Domain MVC decoupling compile and IIS runtime wiring
commit 3550830b  2026-08-20  feat(di): standardize pure constructor dependency injection across entire solution
```
In a clean, multi-file refactoring pass, the solution was split into three distinct layers:
1. **`EImece.Domain`:** Pure C# domain models, interfaces, and business logic with zero Web dependencies.
2. **`EImece.Web`:** Shared MVC filters, model binders, and HTTP infrastructure.
3. **`EImece`:** The web presentation host.

### 2. Strategy Pattern for Payments & Stripe-Inspired Admin UI
```git
commit d56a71e6  2026-08-20  feat(admin): premium System Settings center redesign — Stripe/Linear inspired
commit ac66b040  2026-08-21  feat(admin): replace left nav with a mega menu and modernize trees
commit 48a68847  2026-08-21  feat(admin): upgrade auth pages to local Bootstrap 5.3.8 and remove Modernizr
commit cad5d1aa  2026-08-21  feat(admin): upgrade Font Awesome from 4.2.0 to self-hosted 7.3.1 Free
```
Payment processing was abstracted into an `IPaymentStrategy` pattern (supporting Iyzico sandbox and pluggable gateways). Meanwhile, the admin System Settings UI was redesigned from scratch with a clean, Stripe/Linear-inspired aesthetic.

---

## Late August to Today (Sep 2): Production Hardening, Cache Tuning & The Razor Compiler War

The final two weeks were dedicated to proving production readiness under real IIS execution, load testing, and browser automation.

### 1. Bringing Modern `Microsoft.Extensions` to .NET 4.8.1 (Aug 30 – Sep 1)
```git
commit 82507608  2026-09-01  Migrate application logging to ILogger<T> with media/logs default
commit 9d0d2b22  2026-09-01  feat(infra): add MEL Options, IHttpClientFactory, and shared IMemoryCache
commit 01b99120  2026-09-01  feat(infra): migrate to Microsoft.Extensions packages (HealthChecks, Polly, Json, Localization)
commit 55227bed  2026-09-01  fix(infra): add Microsoft.Extensions.FileSystemGlobbing dependency and update binding redirects
```

### 2. High-Performance Caching & Automated Stress Testing (Sep 1)
```git
commit b0da3aa6  2026-09-01  test(perf): add automated stress testing and telemetry suite
commit 399e808a  2026-09-01  perf(cache): configure outputCacheProfiles to location=Server with varyByParam/Custom
commit b86c0fa8  2026-09-01  fix(cache): correctly record OutputCache hits by removing early MvcKey return in probe
commit 59e7e36a  2026-09-01  test(perf): add cache stress test and admin live monitor script
commit 1d2acf10  2026-09-01  feat(cache): add automated background cache warmup service and tracking parameter normalization
```
Under load testing, we discovered that marketing UTM query parameters were bypassing the server cache. The AI implemented tracking parameter normalization, custom `varyByCustom` rules for anonymous vs cart users, and a background cache warmup worker (`1d2acf10`), dropping catalog response times below **20ms**.

### 3. The Painful Razor Compiler Battle on IIS (Sep 1)
This sequence of commits from September 1st shows why **automated compiler gates beat human visual review**:

```git
commit 54455280  2026-09-01  aspnet_compiler command
commit 525924c6  2026-09-01  fix(hosting): remove system.codedom to prevent Roslyn csc.exe group policy block on shared IIS/Plesk
commit 32641eac  2026-09-01  fix(views): ensure C# 5 compatibility for Razor views and update system.codedom in Web.config
commit a4cfa9b4  2026-09-01  fix(views): replace C# 6 null-conditional operators with C# 5 syntax for Razor compatibility
commit 94f63e67  2026-09-01  fix(admin-views): replace C# 6 string interpolations with string.Format for Razor compatibility
commit ad7cf289  2026-09-01  fix(breadcrumbs): replace tuple parameter syntax with C# 5 compatible overloads for Razor views
```

When building Razor views on .NET Framework 4.8.1, the IDE might show valid C# 6/7 syntax (`?.`, `$"..."`, tuple parameters), but when precompiling with `aspnet_compiler.exe` without Roslyn providers (which are blocked in many shared IIS hosting environments), the Razor compiler crashes with syntax errors.

Instead of hunting through hundreds of views manually, `aspnet_compiler.exe` generated the exact file and line errors, and Gemini 3.7 systematically converted string interpolations to `string.Format`, null-conditionals to defensive ternaries, and tuples to concrete parameter overloads.

---

## The Verification Loop That Made Zero-Review Possible

How did 200+ commits land safely without human line-by-line review? By replacing manual reading with an unyielding **Machine Verification Loop**:

```
 ┌─────────────────────────────────────────────────────────────┐
 │               THE AUTONOMOUS VERIFICATION LOOP              │
 └─────────────────────────────────────────────────────────────┘
   [Prompt / Objective]  ───>  [Gemini 3.7 Model in Cursor]
                                      │
                                      ▼
                             [Code Modification]
                                      │
                                      ▼
                        ┌───────────────────────────┐
                        │   VERIFICATION GATES      │
                        │ 1. MSBuild Release Build  │
                        │ 2. aspnet_compiler Precomp│
                        │ 3. Playwright E2E Tests   │
                        │ 4. Local IIS Smoke Run    │
                        │ 5. Stress Test Suite      │
                        └───────────────────────────┘
                                      │
                        ┌─────────────┴─────────────┐
                        ▼                           ▼
                     [PASS]                      [FAIL]
                        │                           │
                        ▼                           ▼
               [Next Prompt Phase]      [Feed Error Back to Model]
```

### The Quality Gates:
1. **MSBuild Release Mode:** Proves compile-time type safety across Domain, Web, and UI layers.
2. **`aspnet_compiler.exe`:** Ensures all 200+ Razor views compile cleanly ahead of runtime.
3. **Local IIS at `http://localhost:81/`:** Tests the real web server lifecycle, application pool recycling, and OWIN pipeline.
4. **Playwright E2E Suite:** Walks the complete customer journey (catalog ➔ cart ➔ discount coupon ➔ Iyzico checkout ➔ order confirmation).
5. **Telemetry & Live Diagnostics:** Verifies OpenTelemetry traces and the `/health` diagnostic dashboard.

---

## Key Lessons from the July-to-September Journey

1. **The Prompts Are Your Source Code:** Code comes and goes; clear, structured architectural prompts can be rerun in minutes. All 40 prompts now live permanently in [`docs/prompts`](https://github.com/eminyuce/EImece/tree/master/docs/prompts).
2. **Never Let AI Move Your Stack:** The easiest AI pitfall is letting a model rewrite your app into a trendy framework. The real engineering value is forcing the model to make your *existing* stack enterprise-grade.
3. **Machine Verification Beats Eye Fatigue:** A human cannot catch a subtle N+1 query in a Razor partial or a C# 5 Razor parser failure across 150 files. Compilers, stress harnesses, and Playwright tests can.
4. **Reasoning Models (Gemini 3.7) Excel at Graph Refactorings:** Cross-layer refactors (Controller ➔ Service ➔ Repository ➔ DTO ➔ View) require multi-file awareness that modern reasoning models execute with incredible precision.

---

## Conclusion

From the rough initial commits in July to the hardened, observable, stress-tested release today, **EImece** made the leap from a dormant hobby to a production software platform.

I didn't achieve this by writing thousands of lines of boilerplate. I achieved it by defining strict architectural contracts, letting AI execute the heavy lifting, and letting the compiler and real servers verify every single step.

*Explore the full commit history and reusable prompt library at [github.com/eminyuce/EImece](https://github.com/eminyuce/EImece).*

---

*Author: Emin Yüce — Creator of [EImece](https://github.com/eminyuce/EImece).*
