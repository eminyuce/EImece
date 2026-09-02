# Multi-design storefront (Crizal / Modern)

- **Captured:** 2026-08-15 6:40:26 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are working on the EImece e-commerce project (ASP.NET MVC 5, .NET Framework 4.8.1, Entity Framework 6).

The project has a multi-design storefront (Crizal, Modern) using a design-aware Razor view engine, and already has product/category pages, search, sitemap, and robots endpoints.

Task: Improve storefront polish for empty states, SEO status codes, structured data, and accurate sitemap/robots.

Implement the following four areas carefully and consistently.

--------------------------------------------------
1. Consistent empty states and “no results” pages
--------------------------------------------------
- Create a consistent empty-state experience for:
  - Product listing with zero products (category, brand, tag)
  - Search with zero results
  - Empty cart (if not already good)
  - Any other major list that can be empty
- Use a shared partial view (e.g. _EmptyState.cshtml) that accepts:
  - Title
  - Short message
  - Optional primary action (link + text), e.g. “Continue shopping” or “View all products”
- Style must work for both active designs (Crizal and Modern). Prefer shared markup with design-friendly CSS classes already used in the project.
- Do not show broken layouts, raw “null”, or empty white space.
- Keep messages user-friendly and localized if the project already uses resources.

--------------------------------------------------
2. Proper 404 / 410 for deleted or inactive products/categories (SEO-friendly)
--------------------------------------------------
- When a product or category is requested by URL/id/slug:
  - If it does not exist → return HTTP 404
  - If it existed but is now inactive/deleted/soft-deleted and should not be indexed → prefer HTTP 410 Gone when you can reliably detect it; otherwise 404
- Ensure the correct status code is set on the response (not just a “not found” view with 200).
- Show a clean, branded not-found page (reuse or improve existing error/not-found view).
- Do not leak internal IDs or exception details.
- Keep admin routes unaffected.
- Make sure route handling still works with the existing design-aware view engine.

--------------------------------------------------
3. Structured data (JSON-LD) on key pages
--------------------------------------------------
Add JSON-LD structured data (script type="application/ld+json") for:

A. Organization (site-wide, usually in the main layout)
   - name, url, logo (if available), sameAs (optional)

B. BreadcrumbList
   - On product detail, category, and other hierarchical pages
   - Reflect the real breadcrumb trail shown to the user

C. Product
   - On product detail pages
   - Include at least: name, description, image, sku/id if available, offers (price, priceCurrency, availability), url
   - Use only public, active product data
   - Do not emit Product schema for inactive/deleted products

Implementation notes:
- Prefer a small helper or partial that renders JSON-LD safely (HTML-encode where needed, valid JSON).
- Reuse existing page models/DTOs; do not load heavy extra data just for schema.
- Keep it design-agnostic (works for Crizal and Modern).
- No new NuGet packages.

--------------------------------------------------
4. Sitemap and robots that stay accurate when content changes
--------------------------------------------------
- Review the existing sitemap endpoint/generation.
- Ensure the sitemap includes only active, public, indexable content:
  - Active products
  - Active categories
  - Important content pages (as already intended)
- Exclude inactive, deleted, or non-canonical URLs.
- Use correct lastmod when available.
- Ensure the response is valid XML and returns the correct content type.
- robots.txt (or robots endpoint):
  - Must reference the correct sitemap URL
  - Must not accidentally disallow important public pages
  - Should stay consistent with the site’s public URL/domain setting
- When products/categories are deactivated or deleted, they must disappear from the sitemap on the next generation (no stale URLs).
- Keep generation efficient (no N+1 queries, use existing projections/AsNoTracking style where possible).

--------------------------------------------------
Technical constraints
--------------------------------------------------
- No tech upgrade, no new packages
- Stay compatible with the multi-design Razor setup
- Prefer shared partials/helpers over duplicating markup per design
- Do not break existing admin or customer areas
- Prefer small, clear changes over large rewrites
- Use existing logging if you need to log unusual 404/410 cases

--------------------------------------------------
Deliverables
--------------------------------------------------
1. Shared empty-state partial + usage on the main empty list/search views
2. Correct 404/410 handling for product and category detail routes
3. JSON-LD for Organization, BreadcrumbList, and Product on the right pages
4. Updated sitemap + robots behaviour that only exposes active public URLs
5. List of files changed
6. Short summary of how each of the four areas now behaves

Important:
- Correct HTTP status codes matter for SEO.
- Empty states must look intentional, not broken.
- Structured data must be valid and only output for public active entities.
- Sitemap must not keep deleted/inactive items.
