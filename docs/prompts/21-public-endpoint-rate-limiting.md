# Rate limiting for public endpoints

- **Captured:** 2026-08-15 6:33:52 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are working on the EImece e-commerce project (ASP.NET MVC 5, .NET Framework 4.8.1, Entity Framework 6, Microsoft.Extensions.DependencyInjection).

Task: Add basic rate limiting / abuse protection for the following public endpoints:

1. Login (Account/Login – POST)
2. Contact form (Contact or equivalent POST action)
3. Checkout (the main order/payment submission action)
4. Search (product/category search GET or POST)

Goals
- Prevent brute-force login attempts
- Prevent spam on the contact form
- Protect checkout from rapid repeated submissions
- Limit abusive search traffic
- Keep the solution simple, in-memory, and compatible with the current stack
- No new NuGet packages, no Redis, no external services

Implementation requirements

1. Create a lightweight in-memory rate limiter
   - Use a static or singleton ConcurrentDictionary to track requests by key
   - Key format examples:
     - "login:{IP}"
     - "login:{IP}:{username}" (optional extra protection)
     - "contact:{IP}"
     - "checkout:{IP}"
     - "search:{IP}"
   - Store request timestamps (or a simple counter + window)
   - Support a sliding or fixed time window (e.g. 1 minute, 5 minutes, 15 minutes)
   - Automatically clean up old entries to avoid memory growth

2. Configuration
   - Put limits in Web.config <appSettings> (or a small static config class) so they can be tuned without recompiling:
     - Login: e.g. 5 attempts / 15 minutes per IP
     - Contact: e.g. 3 submissions / 10 minutes per IP
     - Checkout: e.g. 5 submissions / 5 minutes per IP
     - Search: e.g. 30 requests / 1 minute per IP
   - Make the limits easy to change

3. Integration style
   - Prefer an ActionFilterAttribute (e.g. [RateLimit("login")]) that can be placed on the target actions
   - Alternatively a small helper method that controllers can call at the start of the action
   - The filter/helper must:
     - Read the client IP (handle X-Forwarded-For if present, otherwise UserHostAddress)
     - Check the rate limit
     - If exceeded → return HTTP 429 (Too Many Requests) with a clear message
     - Optionally log the blocked attempt (use the existing NLog/Serilog infrastructure)
   - Do NOT break existing happy-path behaviour when under the limit

4. User experience
   - On 429, return a friendly message (JSON for AJAX endpoints, or a simple view/message for form posts)
   - For login: show a clear “Too many attempts. Please try again later.” message
   - For contact/checkout: same style of message
   - For search: return an empty result or a short message instead of crashing

5. Technical constraints
   - Stay on .NET Framework 4.8.1 / ASP.NET MVC 5
   - No new packages
   - Thread-safe (ConcurrentDictionary or equivalent)
   - Memory-safe (periodic cleanup of expired entries)
   - Works correctly behind a reverse proxy / load balancer (respect X-Forwarded-For when safe)
   - Do not rate-limit authenticated Admin users on admin routes
   - Keep the code in the Web project (Filters or Helpers folder) unless there is a clear existing place for it

6. Deliverables
   - The rate-limiter helper / service class
   - The ActionFilter (or equivalent integration)
   - Web.config keys with sensible default values and comments
   - The exact places where the filter/attribute is applied (Login, Contact, Checkout, Search actions)
   - Short comments explaining the window size and the key strategy
   - A simple way to temporarily disable rate limiting for local development (config flag)

Important
- Keep the implementation minimal and robust
- Prefer clarity over cleverness
- Do not over-engineer (no distributed cache, no complex token-bucket algorithms)
- Make sure existing unit/integration tests still pass
- After implementation, list the actions that are now protected and the default limits used
