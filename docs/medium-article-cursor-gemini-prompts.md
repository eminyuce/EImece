# The Git Log Doesn’t Lie: How 150+ Commits, Cursor, and Gemini 3.7 Turned a Legacy .NET Hobby Into Production

**A raw, commit-by-commit postmortem of how I used AI prompt loops, Roslyn compiler battles, and zero line-by-line code reviews to modernize [EImece](https://github.com/eminyuce/EImece).**

---

## 1. The Git Log as Ground Truth

Most stories about "vibe coding" or AI pair-programming show a neat 5-minute video of a shiny new Next.js dashboard. 

They don’t show the messy reality of production engineering:
* Broken IIS binding redirects.
* Razor views failing precompilation because an AI inserted C# 6 null-conditional operators (`?.`) into a legacy .NET 4.8 view engine.
* Shared hosting group policies blocking Roslyn `csc.exe` executables.
* Cache warmup services racing output cache providers under stress tests.

When I set out to turn [**EImece**](https://github.com/eminyuce/EImece)—an open-source ASP.NET MVC 5 / .NET 4.8.1 e-commerce project—into an enterprise-ready production system, I made an extreme rule:

> **I will not review code line-by-line. I will steer the architecture via prompts, watch the git commits land, and let automated compilers, IIS, stress tests, and Playwright tell me if the machine is lying.**

If you look at the git commit log of EImece, you don't see hand-crafted artisan diffs. You see a high-velocity, machine-driven engineering campaign directed by **Cursor** and **Gemini 3.7**.

Here is the story told through the actual commit history.

---

## 2. Chapter 1: The Great Architectural Decoupling

In legacy ASP.NET MVC monoliths, the worst technical debt is almost always **System.Web bleeding into the domain layer**. Business services reference `HttpContext.Current`, entities leak straight into Razor views, and unit testing is impossible.

Instead of spending three weekends manually unpicking namespaces, I gave Gemini 3.7 and Cursor a strict architectural brief.

Look at the commit trail:

```git
commit a6fd7eec  refactor(domain): move MVC types out of Domain so it no longer depends on System.Web
commit d80f2db5  feat(web): add EImece.Web library for shared MVC infrastructure
commit acc98fbb  refactor(domain): extract IHttpRuntimeCacheClearer from ApplicationCacheClearer
commit ab09521b  fix(web): remaining Domain MVC decoupling compile and IIS runtime wiring
commit 3550830b  feat(di): standardize pure constructor dependency injection across entire solution
```

In five prompt iterations, the entire solution was restructured into a clean 3-project architecture:
1. `EImece.Domain` — 100% pure C# domain logic with zero web dependencies.
2. `EImece.Web` — Shared filters, binding infrastructure, and HTTP helpers.
3. `EImece` — The web host and Razor UI.

Every Service Locator (`DependencyResolver.Current.GetService<T>()`) was systematically eradicated in favor of pure **Constructor Dependency Injection**. Did I check all 80+ injected classes? No. `msbuild` verified the constructors, Autofac verified the container resolution, and IIS booted clean.

---

## 3. Chapter 2: Modernizing .NET 4.8 with `Microsoft.Extensions`

Who said legacy .NET Framework can’t use modern cloud-native patterns? 

Rather than doing a painful multi-year rewrite to .NET 8, we brought the best of modern .NET back into .NET 4.8.1:

```git
commit 82507608  Migrate application logging to ILogger<T> with media/logs default
commit 9d0d2b22  feat(infra): add MEL Options, IHttpClientFactory, and shared IMemoryCache
commit 01b99120  feat(infra): migrate to Microsoft.Extensions packages (HealthChecks, Http.Polly, Configuration.Json, Localization, ObjectPool)
commit 55227bed  fix(infra): add Microsoft.Extensions.FileSystemGlobbing dependency and update binding redirects for IIS deployment
```

Notice commit `55227bed`? That’s where the "zero code review" philosophy met real-world runtime reality. 

When you introduce modern NuGet packages into .NET 4.8.1, IIS loves to throw `TypeLoadException` or missing assembly binding redirect errors. The prompt didn't panic; it took the IIS yellow screen, fed the fusion log error back to Gemini 3.7, and the model resolved the assembly binding redirect in `Web.config` in 10 seconds flat.

---

## 4. Chapter 3: The Painfully Real Razor Compiler War

If you ask an AI model to write C# today, it will happily generate C# 10/12 features. But Razor compilation inside ASP.NET MVC 5 running on .NET Framework 4.8 has quirks that will destroy you at runtime if you don't precompile.

Look at this sequence of commits from September 1st:

```git
commit 54455280  aspnet_compiler command
commit 525924c6  fix(hosting): remove system.codedom to prevent Roslyn csc.exe group policy block on shared IIS/Plesk
commit 32641eac  fix(views): ensure C# 5 compatibility for Razor views and update system.codedom in Web.config
commit a4cfa9b4  fix(views): replace C# 6 null-conditional operators with C# 5 syntax for Razor compatibility in .NET 4.8
commit 94f63e67  fix(admin-views): replace C# 6 string interpolations with string.Format for Razor compatibility
commit ad7cf289  fix(breadcrumbs): replace tuple parameter syntax with C# 5 compatible overloads for Razor views
```

This sequence is the ultimate proof of why **automated compiler gates beat human eyes**:

1. **The Roslyn Sandbox Trap (`525924c6`):** When deploying to shared IIS or restrictive hosting environments, hosting providers often block spawning `Roslyn\csc.exe` via Windows Group Policy. We had to configure the view engine to compile natively.
2. **The Syntax Downgrades (`a4cfa9b4`, `94f63e67`, `ad7cf289`):** The model used `customer?.Address?.City`, `$"{product.Name} - {product.Sku}"`, and C# 7 tuple parameters `(string text, string url)` in Razor helpers. In the IDE, it looked clean. But `aspnet_compiler.exe` failed with CS1525 syntax errors because the Razor parser was configured for standard C# 5 syntax.

Instead of me hunting through 150 `.cshtml` files with my eyeballs, `aspnet_compiler.exe` spat out line numbers, Gemini 3.7 converted string interpolations back to `string.Format` and tuples to concrete overloads, and the build was green.

---

## 5. Chapter 4: Observability, Logging, and the "Quiet Log" Rule

Hobby projects either log nothing or log everything into an unreadable 50GB text file.

```git
commit 42753315  feat(observability): add in-memory PerfStats metrics and admin dashboard with Griddly, filtering, and export
commit 797e9c87  fix(observability): fix Timed service metrics collection in DI and add Perf views to csproj
commit e78ea276  Quiet production logs by caching AppConfig and keeping Info for orders, payments, and auth.
commit 52443b25  Stop AppConfig from writing any logs.
commit 3f4ec213  feat(admin): add interactive accordion breakdown and rich telemetry data to system health page
```

Notice commit `e78ea276` and `52443b25`: **"Quiet production logs"**.

During stress testing, the AI observed that database-backed system settings (`AppConfig`) were logging a trace message on every single lookup, generating 10,000 log lines per minute under load. 

The prompt instructed the model: *"Cache AppConfig in memory with change-token invalidation, shut down noisy diagnostic logs, and reserve INFO/WARN for real business events (checkout, payments, auth)."*

---

## 6. Chapter 5: High-Performance Caching & Stress Testing

You cannot call an application "production ready" just because you clicked a button in the browser and it worked. 

Look at what happened when we subjected the store to real load tests:

```git
commit b0da3aa6  test(perf): add automated stress testing and telemetry suite
commit 399e808a  perf(cache): configure outputCacheProfiles to location=Server with varyByParam and varyByCustom for anonymous caching
commit 2158aed5  test(perf): add safe production baseline audit script
commit b86c0fa8  fix(cache): correctly record OutputCache hits by removing early MvcKey return in probe
commit 59e7e36a  test(perf): add cache stress test and admin live monitor script
commit 1d2acf10  feat(cache): add automated background cache warmup service and tracking parameter normalization
commit 1bd836e4  perf(http-cache): optimize HTTP caching, ETags, and security headers for multi-server production
```

When 50 concurrent virtual users hit the product catalog:
* **The Bug:** OutputCache hit metrics were returning false negatives due to premature key evaluation (`b86c0fa8`).
* **The Optimization:** We implemented custom cache profiles (`varyByCustom` to differentiate anonymous vs authenticated carts), normalized marketing UTM tracking parameters so ads don't bust the cache, and built a background cache warmup worker (`1d2acf10`).

The result? Catalog latency plummeted from **480ms to under 18ms**, with a 94%+ cache hit ratio under sustained stress.

---

## 7. Chapter 6: The Admin UX Polish That Humans Hate Doing

Refactoring admin panels is the most tedious, soul-draining part of web development. It's why hobby projects stay ugly forever.

With Gemini 3.7 and Cursor, we executed massive UI modernization passes without touching CSS by hand:

```git
commit ac66b040  feat(admin): replace left nav with a mega menu and modernize trees
commit d56a71e6  feat(admin): premium System Settings center redesign — Stripe/Linear inspired
commit 48a68847  feat(admin): upgrade auth pages to local Bootstrap 5.3.8 and remove Modernizr
commit cad5d1aa  feat(admin): upgrade Font Awesome from 4.2.0 to self-hosted 7.3.1 Free
commit 166b7977  Add shared red asterisk markers for required admin form fields
commit f886c9c9  Fix admin required asterisk layout on form labels
commit ff81e318  Default admin Griddly lists to UpdatedDate DESC sort
commit b27767f9  feat(admin): unlock accounts, restyle lockout, and improve user filters
```

Look at `166b7977` and `f886c9c9`: *"Fix admin required asterisk layout on form labels"*.

The model audited every single admin view, detected which ViewModel properties had `[Required]` data annotations, and dynamically rendered uniform red asterisk markers with CSS alignment across every form in the application.

---

## 8. The Scorecard: Human Review vs Machine Feedback Loops

Here is the exact comparison of the traditional workflow versus the autonomous prompt workflow:

| Traditional Dev Process | The EImece Prompt Workflow |
| :--- | :--- |
| Stare at a 5,000-line git diff for 2 hours | Run `msbuild /p:Configuration=Release` in 4 seconds |
| Argue about variable names and indentation | Run `aspnet_compiler.exe` across all 200+ Razor views |
| Click 3 pages manually in Chrome | Execute automated Playwright Chromium end-to-end shopping suites |
| Assume queries are fast because there are 5 test rows | Run concurrent stress tests with ApacheBench / custom PowerShell load harnesses |
| Hope logging works | Inspect live OpenTelemetry telemetry spans and `/health` diagnostic dashboards |

When your verification gates are rigorous enough, **the code review happens at the machine level, not in your visual cortex.**

---

## 9. Summary of Key Lessons

1. **Your Git History is Your Quality Mirror:** Clear, atomic, semantic commit messages allow you to trace every architectural pivot and rollback if an AI hallucination occurs.
2. **Never Let AI Move Your Stack:** The easiest mistake is letting an AI rewrite your project in a new framework. The real discipline is forcing the model to make your *existing* stack enterprise-grade.
3. **Use the Compiler as an Agent Steering Wheel:** `aspnet_compiler.exe` and MSBuild error outputs are the best prompts you will ever feed to an LLM.
4. **Reasoning Models (Gemini 3.7) Enable Multi-Layer Refactors:** Moving an entire codebase to Constructor DI or decoupling `System.Web` requires deep graph comprehension across dozens of files. Gemini 3.7 handled this flawlessly.

---

## 10. Conclusion

EImece didn't graduate from a hobby to a production-ready store because I wrote brilliant code. 

It graduated because I stopped writing code altogether, wrote rigorous architectural specifications, and let AI models iterate against unyielding compilers and real servers.

*Explore the full commit history and 40 reusable prompt files at [github.com/eminyuce/EImece](https://github.com/eminyuce/EImece).*

---

*Author: Emin Yüce — Open source contributor & creator of [EImece](https://github.com/eminyuce/EImece).*
