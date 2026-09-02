# I Didn’t Review the Code. I Prompted My Way From a Hobby Shop to a Production E-Commerce App.

**How I used Cursor, Gemini 3.7, and a library of coding prompts to turn EImece — an old ASP.NET MVC side project — into something I could actually run as a product.**

For years, [EImece](https://github.com/eminyuce/EImece) was a hobby.

It was a real store, not a tutorial. ASP.NET MVC 5, Entity Framework 6, SQL Server, IIS, Iyzico checkout, an admin panel, a storefront. The kind of project you keep because it works, and you never quite finish because finishing a production system is a different job than writing features on weekends.

In about a month I treated it like a product anyway.

I did not sit down and review pull requests line by line. I wrote prompts. I ran them in Cursor with Cursor’s models and Gemini 3.7. I kept going until the thing compiled, deployed to IIS, and I could walk a customer path from home page to paid order. If something broke, I did not open the diff and debate architecture. I wrote the next prompt.

That is the honest version.

---

## The project I started with

EImece is an open-source e-commerce app for catalog, content, cart, checkout, and store operations. Classic Microsoft stack:

- .NET Framework 4.8.1
- ASP.NET MVC 5.3
- Entity Framework 6.5
- IIS + SQL Server
- Razor storefront themes
- Iyzico payments

This matters, because most “I vibe-coded a SaaS” stories start on a greenfield Next.js repo. I started on a living legacy app. The database already had products. The admin already had grids. The payment call already existed, hard-wired in one place. There was no option to throw it away and start over.

The constraint I gave every model was the same: **do not migrate the stack. Make this stack production-shaped.**

---

## The method: one prompt, one job, no code review

I did not ask the model to “improve the project.”

I asked it to do one production job at a time, in a prompt long enough that a senior engineer could execute it without asking me what I meant.

The prompts now live in the repo: [docs/prompts](https://github.com/eminyuce/EImece/tree/master/docs/prompts). There are forty of them. I keep them there so I can paste the same brief into a new Cursor session months later.

A typical session looked like this:

1. Paste a prompt into Cursor.
2. Let Gemini 3.7 or a Cursor model read the repo and implement.
3. Build.
4. Publish to the local IIS site (`http://localhost:81/`).
5. Hit the real pages — admin, storefront, checkout — not a mock.
6. If Playwright, Lighthouse, `aspnet_compiler`, or a stress test screamed, paste the next prompt.

I was not the reviewer. I was the product owner with a compiler, a browser, and a stubborn requirement: **the path has to work end to end.**

People will say that is reckless. For a payment-taking shop, it is. I am not recommending it as a safety philosophy. I am describing what I actually did, because the interesting part is not “AI writes code.” The interesting part is **what you can ship if you replace line-by-line review with a closed loop: prompt → implement → compile → deploy → exercise the real site.**

The code review happened in production-shaped checks, not in my head.

---

## Why the prompts were long

Short prompts produce short thinking.

“Make the admin nicer” gets you a CSS skin. “Add logging” gets you `Console.WriteLine`. “Make it faster” gets you a cache in the wrong layer.

So I wrote briefs the way I would brief a contractor I would not sit next to:

- stack and paths (`Areas/Admin/Views/Shared/_Layout.cshtml`, IIS folder, GitHub URL)
- what must not change (no ASP.NET Core migration, no invented routes, no payment-logic rewrite unless that *was* the task)
- the acceptance test (sidebar on mobile, Griddly AJAX pager, Playwright on Chromium, `aspnet_compiler` clean)
- the production constraint (2FA stays, no hardcoded admin bypass, no secrets in telemetry)

The first prompt, on August 4, was not “add a feature.” It was a full redesign of the admin shell: kill the two top navbars, put navigation in a fixed left sidebar, keep Bootstrap 3, keep Grid.Mvc, keep CKEditor, keep role checks. That set the tone. After that, almost every prompt named a role (“senior observability architect”, “senior QA engineer”, “.NET performance engineer”) and then a measurable job.

I used Cursor as the workshop. I used Gemini 3.7 when I wanted a model that would stay inside a long, picky spec. I switched models when one stalled. I did not switch the method.

---

## What one month of prompts actually changed

The sequence in `docs/prompts` is the real changelog. It is more honest than a polished roadmap.

**Week 1 — make the back office feel like software someone can run**

- Admin sidebar instead of two horizontal navbars
- OpenTelemetry traces, metrics, logs
- Excel/CSV export that looks like an export, not a dump
- Turkish column headers
- A modern admin visual pass on Bootstrap 3
- Hide coupons, orders, carts, and reports when `IsPriceEnabled` is false
- Form alignment on every SaveOrEdit page
- Lighthouse performance and accessibility fixes

**Week 2 — stop treating MVC like a 2014 weekend project**

- One visual language for every admin list, copied from Products
- Admin controllers converted to async without changing business rules
- Admin usable on a phone
- Iyzico ripped out of a hard-coded call and put behind a Strategy
- Full SQL + image seed so IIS had a store to click through
- A QA pass that assumed desktop *and* mobile
- An admin screen to test email templates instead of discovering them in production

**Week 3 — the unsexy production work**

- Storefront queries projected instead of dragging full entities into Razor
- Unused methods deleted only after the agent hunted routes, AJAX, and reflection
- Rate limits on public endpoints
- A green/red health badge in admin
- Multi-design storefront (Crizal / Modern) as a first-class switch
- Coupon engine with real validation, not a discount text box
- An architecture review that asked for logging, secrets, CI, and tests — not a rewrite
- Grid.Mvc replaced with Griddly, on a separate branch, with the old UX kept

**Week 4 — close the loop until a customer can buy**

- ViewModels that no longer smuggle EF entities into views
- Deploy to IIS and walk every sitemap URL
- Playwright visual QA on the real storefront
- Performance pass on EF6, SQL, Razor, IIS
- `aspnet_compiler.exe` as the source of truth for view errors
- DTO audits across Store Front and Customer
- Caching where the pages actually wait
- Auth bypasses removed; 2FA follows system settings
- A Chromium Playwright suite for the full shopping path, including Iyzico sandbox
- A stress test that refused to call the app production-ready just because happy-path tests passed
- A new Crizal Razor theme from an HTML template, judged by screenshots, not by “the CSS compiled”

That last point is the whole method. I did not ask “does the Razor look correct in the editor?” I asked “does the deployed page look like a store?”

---

## Cursor was the factory. The prompts were the spec.

Cursor is very good at *being in the repo*. It can open `_Layout.cshtml`, find every `IndexGrid`, follow a service into a repository, and change the call sites. Gemini 3.7 was very good at *staying inside a long brief* — the kind of prompt that says “do not invent routes” twelve times because models love inventing routes.

I did not use them as a chat for advice. I used them as a build crew.

The artifact I protected was not a perfect commit history. It was the prompt. If the session went off the rails, I started a new one and pasted the same file. That is why those files are in GitHub now. A chat thread dies. A prompt you can rerun is a process.

If you take one practical thing from this article, take this:

**Write the prompt as if you will not be in the room. Then don’t be in the room. Watch the product, not the diff.**

---

## “I didn’t review the code” — what that actually meant

It did not mean I never looked at the site.

I looked at the site constantly. Admin on a phone. Checkout on Chromium. IIS logs. Yellow screens. Lighthouse. Playwright screenshots. Stress-test numbers. `aspnet_compiler` failures. Sitemap URLs that 500’d.

What I did *not* do is the ritual most of us were trained on: open every changed file, argue about naming, reject a pull request because a helper could be an extension method.

I replaced that ritual with gates:

| Gate | Why it existed |
| --- | --- |
| Solution build | If it does not compile, it is not a product. |
| `aspnet_compiler` | Razor can fail in ways MSBuild will not tell you. |
| IIS at `localhost:81` | A passing unit test is not a store. |
| Playwright on the real site | The rendered page is the source of truth. |
| Sitemap crawl | Forgotten routes are how hobby projects stay hobby. |
| Stress test | One user in admin is not production. |
| Health + metrics | If you cannot see it, you cannot run it. |

That is a different kind of review. It is harsher in some ways. A beautiful repository pattern that still N+1s the product list will fail a stress test. A clever 2FA bypass “just for the agent” will fail the security prompt. A theme that only works at 1440px will fail the Playwright viewport list.

It is weaker in other ways. I cannot tell you I personally audited every authorization attribute. I can tell you I wrote a prompt that said: do not introduce authentication bypasses, do not hardcode admin exceptions, 2FA follows System Settings, remove the bypass email list. Then I let the model do it and I kept shipping.

If you run a bank, do not copy that. If you are one person trying to turn a ten-year hobby into a shop you can operate, you need to know the trade you are making.

---

## What “production” meant for this stack

I did not mean “rewrite it in .NET 8.”

I meant the boring list that hobby projects skip:

- **Operations:** `/health`, `/healthz`, `/metrics`, OpenTelemetry, structured logs, a badge in admin that is red when SQL or disk is dead
- **Security:** rate limits on login and public forms, no leftover auth backdoors, headers, secrets out of the prompt files before they hit GitHub
- **Data access:** DTOs at the view boundary, projections for storefront lists, cache where the page waits, async admin grids
- **Payments:** Iyzico behind a Strategy so the next provider is not another copy-paste
- **UX you can hand to a merchant:** sidebar admin, mobile admin, Griddly lists, Turkish labels, coupons that validate, email templates you can preview
- **Proof:** Playwright shopping flow, IIS deploy checklist, compiler-clean views, a stress test that is allowed to say “not ready”

The stack stayed old. The *behavior* stopped being hobby.

That was the point of the architecture prompts. I told the model: I am not looking to modernize the technology. I am looking for the operational gaps that keep this a weekend project. Then I implemented those gaps one file at a time.

---

## What I would repeat, and what I would not romanticize

I would repeat the prompt library. Forty named jobs beat one giant “make it enterprise” chat.

I would repeat the IIS-first loop. Agents are optimistic. IIS is not.

I would repeat switching models instead of arguing with a stuck one. Cursor and Gemini 3.7 were tools. The spec was the constant.

I would not romanticize skipping code review. I got speed because I accepted risk. Some of that risk is still in the product. An agent that can delete unused methods can also delete a method that is only called from a string in JavaScript. I tried to write prompts that said so. I did not personally verify every deletion.

The discipline that saved me was not taste. It was **end-to-end**. If I could not add to cart, check out with Iyzico sandbox, open the order in admin, and see a health badge that was not lying, the session was not done. I did not stop at “the model said it finished.”

---

## If you want to do this to your own hobby project

1. Pick a stack you will not abandon. Production is constraints, not a rewrite.
2. Write prompts as contracts. Role, paths, hard no’s, acceptance test.
3. Keep them in the repo. A chat is a memory. A file is a process.
4. Run the app where it will actually run. For me that was IIS, not only IIS Express.
5. Let browsers and compilers review. You review the product.
6. Do the unsexy prompts on purpose: health, rate limits, DTOs, compiler, stress. Those are what change the category from hobby to software you can operate.

EImece is Apache 2.0. The prompts are public next to the code. If you want the briefs I actually pasted into Cursor, they are here:

[https://github.com/eminyuce/EImece/tree/master/docs/prompts](https://github.com/eminyuce/EImece/tree/master/docs/prompts)

I did not review the code like a staff engineer on a platform team.

I kept building until a customer could buy something, an admin could run the shop, and the machine could tell me when it was sick.

That was enough to take it out of the hobby drawer.

---

*Emin Yüce builds [EImece](https://github.com/eminyuce/EImece), an open-source ASP.NET MVC e-commerce platform. Support the project on [Buy Me a Coffee](https://buymeacoffee.com/eminyuce).*
