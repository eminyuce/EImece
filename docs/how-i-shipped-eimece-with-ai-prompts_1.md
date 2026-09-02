# 40 Prompts, Zero Code Reviews: How I Turned a Legacy ASP.NET Hobby Project Into "Production" Software

*What happened when I stopped reading diffs and started writing specs — rebuilding EImece with Cursor and Gemini 3.7, one architecture-grade prompt at a time.*

---

Somewhere around prompt #25 I asked an AI model to review my own project and hand back "an overall technical health score, 1–10, with justification."

I hadn't read a single diff it had produced up to that point.

That's not a flex, and it's not really a confession either — it's just the most accurate sentence I can write about the last month. Between August 4th and September 2nd, I converted **[EImece](https://github.com/eminyuce/EImece)**, an open-source e-commerce platform I've been tinkering with as a hobby project (ASP.NET MVC 5.3, .NET Framework 4.8.1, Entity Framework 6.5, SQL Server, Apache 2.0, deployed on IIS), into something I'd actually call production software. I did it inside Cursor, mostly running on Google's Gemini 3.7, and I did it almost entirely without reading the code the model wrote.

For context on what "hobby project" actually meant here: this is a repo with roughly 1,900 commits behind it, one GitHub star, and a Buy Me a Coffee link at the top of the README — a real, working codebase nobody but me was depending on. That's exactly the kind of project where an experiment like this is worth running, and I'll come back to why that matters later.

I want to walk through how that actually went, because "I let an AI build my app and didn't check its work" sounds reckless when you say it out loud — and it *is* a little reckless — but the process that got me from "hobby project" to "something that has been strategy-refactored, seeded, load-tested, and visually regression-tested against a real IIS deployment" wasn't reckless in the way people usually picture. It just moved the discipline from *reading code* to *writing specs*.

## The one artifact I kept

I saved every coding prompt I wrote, in order, as I wrote it. Forty of them by the time I was done. Reading them back is basically a diary of what "production-ready" meant to me at each point, and they fall into three fairly clean phases.

**Phase 1 — Make it look real (Aug 4–10, ~12 prompts).** A left-sidebar admin redesign to replace two stacked top navbars. A full "modern SaaS admin panel, Shopify/Linear/Vercel-level" visual pass. Excel/CSV export with real formatting. Localizing grid column headers. Hiding commerce UI entirely when pricing is turned off. Fixing Bootstrap 3 form alignment across a dozen broken edit pages. Chasing a Lighthouse mobile performance score up from 0.40. Converting admin controllers to async. Making the admin area usable on a phone.

**Phase 2 — Make it real architecture (Aug 11–17, ~14 prompts).** Refactoring the Iyzico payment integration out of one bloated service and into a proper Strategy pattern. Seeding a realistic database plus product images. Running a genuinely adversarial end-to-end QA pass. Building a safe test environment for email templates. Rewriting the entire storefront data-access layer around projections instead of full EF entity graphs. An application-wide dead-code sweep. Extending the payment Strategy pattern. Rate limiting for login, checkout, contact, and search. A system health badge in the admin header. Consistent empty states, correct 404/410s, and JSON-LD for SEO. A coupon engine with usage limits and per-customer caps. A full "enterprise production-readiness" architecture review. Migrating every admin grid off Grid.Mvc onto Griddly — a migration that fully stuck, since Grid.Mvc doesn't exist anywhere in the codebase today.

**Phase 3 — Make it survive contact with reality (Aug 20 – Sep 2, ~14 prompts).** Moving high-value settings out of `Web.config` into a database-first-with-config-fallback pattern. Auditing every ViewModel so raw entities can never leak into a Razor view. Deploying to IIS and verifying the deployment matched the source. A Playwright-driven visual QA loop across the storefront. A performance analysis pass. Fixing Razor errors thrown by `aspnet_compiler`. Two separate DTO audits — one for the storefront and customer areas, one for every view in the app. Backend caching, but fixing the *existing* cache instead of bolting on a new one. Ripping out an admin-auth bypass and a hardcoded 2FA exception list. A full Playwright shopping e2e suite. A deep architecture audit of the whole repository. A genuine load and stress test against the deployed IIS site — not the dev server. And, to close it out, building a brand-new Razor theme from an HTML template, validated screenshot-by-screenshot at eight breakpoints.

None of that reads like the popular idea of "vibe coding," where you fire off a two-line prompt and hope. Every one of these is long, specific, and full of constraints. What *is* true to that reputation is what happened after I hit send: I read the model's summary, clicked around the running site, and if it looked right, moved on to the next prompt. I did not open the diff viewer.

## The toolchain, briefly

Cursor was the driver — an editor built around an agent that can read the whole repository, open files, run terminal commands, and make multi-file edits directly, instead of just answering questions in a side panel. The engine behind most of this was Google's **Gemini 3.7**, specifically the "Flash" tier DeepMind shipped in the middle of August 2026 as, in Google's words, its "most intelligent workhorse model yet for coding and agents" — tuned for software engineering, debugging, and multi-step agent work rather than raw reasoning, and priced to be run constantly rather than sparingly. That release date lands almost exactly in the middle of my prompt log, which is a fun bit of trivia: the early admin-panel prompts in this project effectively ran on the previous generation, and the harder stuff later — the stress test, the deep architecture audit, the full theme rebuild — ran once 3.7 was available.

I mention the specific model because it matters for the honesty of this story: this is a fast, cost-efficient, coding-tuned "workhorse" model, not the single biggest reasoning model on the market. The premise of the whole month was that a model like that, driven by a genuinely detailed spec, can carry a solo project a long way — without a human acting as a second set of eyes on every line it writes.

## Three prompts that show what "detailed" actually means

If the only version of AI-assisted coding you've seen is a two-sentence request, these will look like a different sport.

The payment refactor prompt didn't just say "add a Strategy pattern." It named the exact file (`EImece.Domain/Services/IyzicoService.cs`), the exact methods on it, the exact controller that called it, and then spelled out the interface, the concrete strategy class, the context object, the DI registration, and a closing line that mattered more than anything else in it:

> "Do not break existing shopping-cart, order, customer, or address flows... Keep the change incremental and testable; existing unit/integration tests around payment should still pass after adaptation."

The rate-limiter prompt was almost aggressively narrow. No new NuGet packages. No Redis. No external services. A `ConcurrentDictionary`, a sliding window, an `ActionFilterAttribute`, limits defined in `Web.config` so they could be tuned without a recompile, and an explicit instruction not to rate-limit authenticated admins. It even specified the exact wording of the 429 response users should see.

And the stress test — the closest thing this project has to a real SRE exercise — spent an entire section defining vocabulary before allowing any conclusions:

> "The final result must distinguish clearly between: *Measured*, *Observed*, *Inferred*, *Not Tested*, *Recommended*... Do not fabricate performance numbers. Do not claim caching is effective without comparing cold-cache and warm-cache behavior."

That prompt also drew a hard line around the payment integration — the model was told to determine whether Iyzico was in sandbox mode and, if it couldn't confirm that, to mark payment load-testing as **NOT TESTED — PAYMENT SAFETY** rather than risk firing real transactions. That's a rule I wrote because *I* knew where the actual danger was, even though I wasn't going to read the code that respected it.

## The QA prompts were my code review

Here's the part I think is actually interesting, more than the "I didn't check the diffs" headline: I never stopped verifying the work. I just stopped verifying it *myself*, line by line, and started asking a second prompt to verify it adversarially.

The production-ready QA prompt told the model to act as "a Senior QA Engineer... with 20 years of experience in finding real bugs, edge cases, and production issues," gave it explicit permission to touch web.config, run Playwright, and read logs, and demanded a structured JSON bug report — severity, steps to reproduce, expected vs. actual, suggested fix — specifically so it could be handed straight back to another coding agent. The Playwright visual-QA loop for the storefront was explicit that "the rendered browser result is the single source of truth" and that a page isn't done just because Razor compiled. The deep architecture audit at the end asked for an honest 1–10 health score and a real verdict on whether the project was worth continuing to invest in.

Put together, that's a review process — it just isn't a *human* reading a diff. It's a deployed IIS instance, a real database, Playwright driving Chromium against real URLs, and a series of prompts whose entire job was to find what was broken and describe it precisely enough that the next prompt could fix it. The loop was: build → deploy → screenshot or query → find something wrong → fix → rebuild → retest. Repeat.

## Where this could have bitten me

I don't want to oversell this, because there's a real gap between "the site works when I click through it and Playwright doesn't scream" and "this code is actually correct."

A few honest risks that were present the whole time:

- **Business-rule bugs hide well.** A coupon usage limit that's off by one, or enforced at the wrong point in checkout, can look completely fine in a demo and still be wrong. Playwright will happily confirm that a coupon *field exists and submits*; it won't tell you the discount math is subtly wrong unless I'd thought to write a test that encodes the exact rule.
- **Security-sensitive code is exactly where "it works" isn't good enough.** Rate limiting, the admin-auth-bypass removal, 2FA — these are places where a plausible-looking implementation and a correct one can differ in ways that only show up under adversarial conditions I didn't create.
- **No human is pruning duplication or bad judgment calls.** Across 40 prompts touching overlapping areas (data access got reworked three separate times), some inconsistency almost certainly crept in that a maintainer reading the code would have caught immediately and I simply won't notice until something breaks.
- **This only worked because of what was at stake.** It's a solo side project. Nobody's compliance review depends on it, and — as the stress-test prompt shows — I was explicit that no real payment should ever be triggered by any of this. I would not run this same "write the spec, skip the diff" process on something handling real customers' money or data without a human actually reading the security-critical parts.

I'm flagging these not to walk back the experiment, but because "I built production software by not reviewing code" is a genuinely bad headline to leave unqualified. What I actually did was replace *my* review with a different, narrower kind of review — one that's good at catching "does this render, does this respond, does this survive load" and much weaker at catching "is this business rule exactly right."

## A rough playbook, if you want to try this

If you're sitting on a hobby project and want to run something like this yourself, the specific habits that seemed to matter were:

1. **Write prompts like a tech lead writes a ticket** — real file paths, the actual current behavior, explicit constraints ("no new NuGet packages," "do not migrate to EF Core"), a list of deliverables, and acceptance criteria the model can check itself against.
2. **Give the agent grounding, not vibes.** Every prompt in this project pointed at real controllers, real views, or a real running URL. The model wasn't asked to imagine an app; it was asked to inspect one.
3. **Follow every "build" prompt with a "break it" prompt.** QA passes, architecture audits, and stress tests aren't a nice-to-have at the end — they're the substitute for the code review you're skipping. Do them often, not just once.
4. **Use a real deployed environment as ground truth**, not just the dev server. Testing against `http://localhost:81` under actual IIS caught things a Visual Studio debug session would have hidden.
5. **Put a fence around the dangerous stuff.** The clearest, most repeated instruction across these 40 prompts wasn't about features — it was "do not trigger a real payment," "never delete real data," "never expose credentials." Decide what must never happen before you decide what should.
6. **Keep the prompts themselves as an artifact.** They're the actual spec of the system now. If I ever do go back and read the code, these 40 files are a better map of *why* it looks the way it does than the commit history is.

## The README I did read

There's one document in this whole process I actually read start to finish: the README. Not because I suddenly got disciplined, but because it's the one artifact that's supposed to summarize every diff I skipped, and going back through it now is a strange experience — half project status report, half list of things I have to take on faith.

Some of it confirms what I'd have guessed. Griddly fully replaced Grid.Mvc, and somewhere a model apparently anticipated I might ask for the old one back by accident, because the contributing guide now says, in bold: **"Do not add legacy Grid.Mvc back."** Other parts I wouldn't have predicted from the prompts alone. Along the way the project picked up a fourth project, `EImece.Web`, a shared MVC-infrastructure layer sitting between the IIS host and the domain layer. And the logging stack got consolidated: the README states flatly that "Serilog is not used in the current stack — do not add it back," even though one of my very first prompts explicitly asked for NLog *and* Serilog side by side. Somewhere in the next thirty-eight prompts, something decided that was redundant and quietly dropped it, and I only found out reading the README just now.

Caching runs through LazyCache over a shared `IMemoryCache`. Outbound calls go through four named `IHttpClientFactory` clients — general-resilient, Iyzico, reCAPTCHA, and a short-timeout one for health probes. Logging is constructor-injected `ILogger<T>` writing to NLog. None of that appeared verbatim in any prompt I wrote; it's the shape the model settled on while satisfying a dozen overlapping requirements across a month, handed back to me as documentation instead of as a diff.

That's the honest shape of "not reviewing the code," in the end: I have opinions about what should be true of this system, a README's worth of claims about what is true of it, and a gap in between that I'm trusting Playwright, a load test, and an architecture audit — not my own eyes — to have closed.

## The honest verdict

A month ago EImece was a hobby project with two stacked navbars and a payment integration hard-wired to one provider. It now has a Strategy-patterned payment layer, a projection-based data-access path instead of full entity graphs, rate limiting, a coupon engine with real usage rules, database-first configuration with safe fallback, a from-scratch theme validated at eight responsive breakpoints, and a load test that tells me — with actual numbers, not vibes — roughly where it falls over.

I still haven't read most of the code that does any of that. What I did instead was get much better, much faster, at describing exactly what "done" means before I asked for it — and then, instead of trusting that the code looked right, kept asking a model to go prove it was right against the actual running application. That's the trade I made, and it's the one I'd tell anyone trying this to make consciously, not by accident.

If you want to see where it landed, the repo — README, docs folder, Playwright suite, and the GitHub Actions deploy workflow included — is public at **[github.com/eminyuce/EImece](https://github.com/eminyuce/EImece)**.
