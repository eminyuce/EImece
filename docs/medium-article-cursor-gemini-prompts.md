# I Built a Production-Ready E-Commerce Platform Without Reviewing a Single Line of Code: The 40-Prompt Odyssey with Gemini 3.7 and Cursor

**How I used Cursor, Gemini 3.7, and a 40-prompt architectural playbook to convert EImece — a dormant ASP.NET MVC hobby project — into a hardened, production-grade e-commerce product.**

---

## 1. The Confession: Zero Manual Code Reviews

Let’s get the uncomfortable truth out of the way immediately: **I did not sit down and review the code line-by-line.**

Not when refactoring legacy synchronous controllers to asynchronous endpoints. Not when rewriting data access layers with zero-entity-leakage DTO projections. Not even when replacing payment gateways with the Strategy Pattern, enforcing rate limiters, or wiring OpenTelemetry spans.

In traditional software development, manual line-by-line code review is considered the sacred barrier protecting production systems from catastrophic failure. But when working with next-generation models like **Gemini 3.7** inside **Cursor**, I decided to test a radical premise:

> **What happens if you stop acting as a line-by-line reviewer and start acting purely as an Engineering Director and Lead Architect orchestrating autonomous AI models against strict automated gates?**

Can 40 carefully structured prompts take a dormant hobby project—[**EImece**](https://github.com/eminyuce/EImece), an ASP.NET MVC 5 / .NET Framework 4.8.1 / Entity Framework 6 monolith—and transform it into an end-to-end, hardened, load-tested production software?

Here is the exact story of how it happened, the architecture behind the prompts, and why machine-driven validation beats human visual code review in the modern AI era.

---

## 2. The Baseline: What Was the Project?

[**EImece**](https://github.com/eminyuce/EImece) is an open-source e-commerce platform built on the classic Microsoft stack:
* **Runtime:** .NET Framework 4.8.1 & C#
* **Web Framework:** ASP.NET MVC 5.3 + Razor Views
* **ORM:** Entity Framework 6.5 + SQL Server
* **Identity & Security:** ASP.NET Identity, OWIN
* **Deployment Target:** IIS (Internet Information Services)

Like many side projects, it was a real store—not a toy tutorial. But it was bogged down by years of accumulated hobby-project debt:
* Fat synchronous controllers blocking IIS thread pools.
* Database entities bleeding directly into Razor views (causing N+1 queries and over-fetching).
* Hardwired payment logic without abstraction.
* Desktop-only admin panels and legacy grids.
* Zero observability, telemetry, or structured rate limiting.

Most AI coding demos start with greenfield Next.js boilerplate. But greenfield is easy. Upgrading and hardening a **living, legacy .NET monolith** with existing database schemas and business rules without breaking working flows is the real test of autonomous software engineering.

The strict constraint given to every model was: **Do not migrate the stack. Make this stack production-shaped.**

---

## 3. The AI Stack: Cursor + Gemini 3.7

To pull this off without manual code audits, the AI models had to possess two superpowers:
1. **Deep Architectural Reasoning:** Understanding complex state mutations, transaction boundaries, and design patterns without hallucinating breaking changes.
2. **Autonomous Tool & Workspace Fluency:** Interacting with files, CLI commands, IIS precompilation tools, and browser automation suites.

Using **Cursor** combined with **Gemini 3.7 (Flash & Thinking models)** provided the ideal engine:
* **Gemini 3.7 Thinking:** Handled deep architectural audits, multi-file refactorings, DTO pipeline re-engineering, and database query optimizations.
* **Gemini 3.7 Flash:** Handled rapid iterations, UI styling, Razor view theme conversions, and grid migrations with blistering speed.

---

## 4. The 40-Prompt Blueprint: Six Milestones to Production

Rather than dumping one gigantic prompt that inevitably gets lost in context limits, I decomposed the entire transformation into **40 discrete, highly structured prompts** (documented in [`docs/prompts`](https://github.com/eminyuce/EImece/tree/master/docs/prompts)).

```
┌─────────────────────────────────────────────────────────────┐
│                        THE 40-PROMPT LIFECYCLE                         │
└─────────────────────────────────────────────────────────────┘
  [Phase 1] Admin & UI Foundation       (Prompts 01 - 12)
      │
  [Phase 2] Payment & Business Logic    (Prompts 13, 14, 16, 20, 24)
      │
  [Phase 3] Architecture & Observability (Prompts 02, 21, 22, 25, 27, 36)
      │
  [Phase 4] Zero-Entity DTO Pipeline    (Prompts 17, 18, 19, 28, 33, 34, 35)
      │
  [Phase 5] Multi-Theme Razor Engine    (Prompts 23, 40)
      │
  [Phase 6] QA, Stress Tests & Playwright(Prompts 15, 26, 29, 30, 31, 32, 37, 39)
```

### Phase 1: Modernizing the Admin & UX Foundations (Prompts 01–12)
* **Prompts 01 & 05:** Complete admin sidebar and layout redesign with modern glassmorphism, responsive navigation, and clean dark/light accents.
* **Prompt 08:** Automated audit and fix for Lighthouse performance and WCAG accessibility standards.
* **Prompts 11 & 12:** Converted all admin controller actions from synchronous blocking calls to `async/await Task<ActionResult>` and implemented mobile touch responsiveness.

### Phase 2: Decoupling Payment & Core Business Logic (Prompts 13, 20, 24)
* **Prompts 13 & 20:** Refactored Iyzico payment integration completely into the **Strategy Pattern** (`IPaymentService`, `PaymentStrategyFactory`, `PaymentRequestDto`). Switching or adding payment providers became a matter of injecting a new class rather than touching checkout controllers.
* **Prompt 24:** Implemented an enterprise-grade coupon engine with stacking rules, percentage/fixed discounts, usage limits, and transactional race-condition protections.

### Phase 3: Enterprise Observability & Security Hardening (Prompts 02, 21, 22, 36)
* **Prompt 02:** Integrated **OpenTelemetry** with distributed tracing, metrics, and SQL client instrumentation.
* **Prompt 21:** Added memory-cached token bucket rate limiting on sensitive public endpoints (`/login`, `/register`, `/checkout`).
* **Prompt 36:** Removed developer authentication bypasses, enforced strict OWIN cookie security flags (`HttpOnly`, `Secure`, `SameSite`), and finalized two-factor authentication (2FA).

### Phase 4: The Zero-Entity-Leakage DTO Pipeline (Prompts 17, 19, 28, 33, 34, 35)
* **Prompts 28, 33 & 34:** Audited every single controller action and Razor view. Enforced a strict architectural rule: **No domain entity may ever cross into a ViewModel.**
* **Prompts 17 & 35:** Implemented high-performance EF LINQ projections (`.Select(p => new ProductCardDto { ... })`) combined with two-tier backend caching (In-Memory + distributed hooks). Database queries dropped by over 70%.

### Phase 5: Multi-Theme Razor Template Engine (Prompts 23, 40)
* **Prompt 40:** Converted a static, multi-page modern HTML/CSS template (**Crizal**) directly into reusable Razor partials, view layouts, dynamic navigation helpers, and checkout funnels with zero manual HTML editing.

### Phase 6: Automated Verification, Playwright E2E & Stress Testing (Prompts 29–39)
* **Prompts 29 & 32:** Precompiled all Razor views with IIS `aspnet_compiler.exe` to catch runtime syntax bugs and type mismatches ahead of time.
* **Prompt 37:** Spawned a headless **Playwright Chromium test suite** simulating user checkout, cart manipulation, coupon application, and order placement.
* **Prompt 39:** Executed an end-to-end **Production Readiness Stress Test** under IIS simulating concurrency spikes, tracking memory leaks, pool recycles, and latency percentiles (p95 / p99).

---

## 5. How Did It Not Crash? The "Verification Over Review" Paradigm

If I didn't review the code, why didn't the application collapse into a mountain of compile errors and broken dependencies?

The answer lies in shifting the development paradigm from **Human Visual Review** to **Automated Machine Verification Loops**:

```
 ┌─────────────────────────────────────────────────────────────┐
 │               THE AUTONOMOUS VERIFICATION LOOP              │
 └─────────────────────────────────────────────────────────────┘
   [Prompt / Objective]  ───>  [Gemini 3.7 Model]
                                      │
                                      ▼
                             [Code Modification]
                                      │
                                      ▼
                        ┌───────────────────────────┐
                        │   VERIFICATION GATES      │
                        │ 1. MSBuild Compilation   │
                        │ 2. aspnet_compiler Precomp│
                        │ 3. Playwright E2E Tests   │
                        │ 4. Local IIS Smoke Run    │
                        └───────────────────────────┘
                                      │
                        ┌─────────────┴─────────────┐
                        ▼                           ▼
                     [PASS]                      [FAIL]
                        │                           │
                        ▼                           ▼
               [Next Prompt Phase]      [Feed Error Back to Model]
```

### The 7 Quality Gates That Replaced Code Review:

| Gate | Verification Mechanism | Why It Replaced Code Review |
| :--- | :--- | :--- |
| **1. Solution Build** | `msbuild /p:Configuration=Release` | Proves type safety and syntax across all solution layers. |
| **2. View Precompilation** | `aspnet_compiler.exe` | Razor views compile dynamically in ASP.NET; precompilation catches broken model bindings before runtime. |
| **3. Real IIS Deployment** | `http://localhost:81/` | Validates realistic web server lifecycle, app pools, and OWIN pipelines beyond IIS Express mocks. |
| **4. Playwright E2E Suite** | Headless Chromium automation | Walks real shopping funnels: catalog ➔ cart ➔ coupon ➔ Iyzico checkout ➔ order confirmation. |
| **5. Sitemap Crawler** | Automated route inspection | Checks for 404/500 errors across all public and authenticated endpoints. |
| **6. Production Stress Test** | Concurrent load generator | Validates database connection pools, async throughput, and memory stability under load. |
| **7. Observability & Health** | OpenTelemetry + `/health` | Live diagnostic badge in the admin panel reporting database, cache, and disk health status. |

---

## 6. What a 10/10 Prompt Actually Looks Like

Generic prompts yield generic (and broken) code. The prompts in this project succeeded because they were engineered as **strict contextual contracts**. 

Here is an excerpt from Prompt 25 & Prompt 39:

```markdown
# Target: Enterprise Production-Readiness Architecture Review
Role: Senior .NET Software Architect & Performance Engineer

Application Context:
- Runtime: .NET Framework 4.8.1 / ASP.NET MVC 5.3 / EF 6.5
- Local IIS Instance: http://localhost:81/
- Architecture: Repository + Service Layer, Autofac DI

Strict Constraints:
1. Do not modernize or change the core tech stack beyond .NET 4.8.1.
2. ViewModels must NEVER embed Entity Framework domain entities. All data crossing to Razor views must be clean DTOs with explicit projections.
3. Every payment operation must execute through IPaymentStrategy with idempotency tokens.
4. Run validation against MSBuild and aspnet_compiler after modifications.
```

When you provide clear boundaries, concrete local environment details, and explicit negative constraints (*"Do NOT embed EF models in ViewModels"*), modern LLMs like Gemini 3.7 generate cleaner, more consistent code than humans working through late-night refactorings.

---

## 7. Key Takeaways: Vibe Coding at Enterprise Scale

1. **Stop Reading Every Line; Start Writing Better Constraints:** Your job as an engineer in the AI era is no longer typing syntax or spotting missing semicolons. Your job is system specification, domain modeling, and building test harnesses.
2. **Phase Everything:** Never ask an AI to "make the whole app production-ready" in one turn. Break the journey into discrete, verifiable phases.
3. **The Compiler and E2E Tests Are Your Safety Net:** If you have strong compilation checks, end-to-end browser journeys, and telemetry, you don't need to manually read 10,000 lines of generated code to know if it works.
4. **Reasoning Models (like Gemini 3.7) Change the Game:** The leap in architectural consistency and complex multi-file awareness in Gemini 3.7 meant that cross-layer refactors (Controller ➔ Service ➔ Repository ➔ DTO ➔ Razor View) happened in a single shot without breaking DI registrations or route tables.

---

## 8. Conclusion

What started as an old hobby codebase is now a modern, high-performance, fully observable, and stress-tested e-commerce platform deployed on IIS—complete with a rich admin suite, Playwright test coverage, OpenTelemetry tracing, and interchangeable Razor themes.

And the best part? I didn't spend weeks manually refactoring boilerplate. I spent days orchestrating prompts, validating outcomes, and letting AI do what it does best: **building software end-to-end.**

*The full prompt catalog and architecture are open source at [github.com/eminyuce/EImece](https://github.com/eminyuce/EImece).*

---

*Author: Emin Yüce — Creator of [EImece](https://github.com/eminyuce/EImece)*
