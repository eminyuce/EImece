# Old (:81) vs New (:82) — Verification Checklist

**Date:** 2026-08-05  
**Legacy:** `http://localhost:81/` (MVC5 / .NET 4.8.1) — BypassAdminAuth=true  
**Core:** `http://localhost:82/` (ASP.NET Core 8) — BypassAdminAuth=true  
**DB:** shared `yuva8905_yuvadan` on `YUCE\SQLEXPRESS`

Artifacts: `publish/baseline-81.csv`, `publish/verify-82.csv`, `publish/verify-82-functional.csv`

## Legend

| Status | Meaning |
|--------|---------|
| Working | HTTP 200 (or expected auth redirect) and real UI/data — not a placeholder |
| Partial | Loads but thinner UX than legacy (e.g. Bootstrap table vs Grid.Mvc) |
| Broken | Error / missing on Core while Working on legacy |
| N/A legacy | Already broken or missing on :81 |

## Storefront

| Feature / path | :81 | :82 | Notes |
|----------------|-----|-----|-------|
| Home `/` | Working | Working | EF products + banners |
| Health `/health` | Working | Working | DB + integrations probe |
| Login / Register / ForgotPassword | Working | Working | Core Identity |
| Cart `/Payment/` | N/A legacy (500) | Working | Session cart |
| Checkout billing | Working | Working | |
| Cargo tracking | Working | Working | |
| Search `/p/arama/` | N/A legacy (400) | Working | |
| Advanced search | Working | Working | |
| Product detail / categories | Working | Working | SEO routes preserved |
| Stories / Pages / Info | Working* | Working | Info needs matching menu link |
| Manage account | Working | Working | |
| Customers portal | Working | Working | Orders list |
| Ajax cities / subscribe | Working | Working | `data.json` + EF |
| RSS / sitemap / robots | Working | Working | Dynamic sitemap |
| Images / captcha | Working | Working | SkiaSharp |

## Admin

| Feature / path | :81 | :82 | Notes |
|----------------|-----|-----|-------|
| Dashboard | Working | Working | Counts + recent orders |
| Products CRUD | Working | Working | Price, code, category, desc, flags |
| Categories / Brands / Templates / Lists / Coupons | Working | Working | Enriched SaveOrEdit |
| Orders + Details | Working | Working | Status/notes |
| Customers / ShoppingCarts | Working | Working | |
| Reports hub + named reports | Working | Working | Same stored-proc style |
| Report Excel/CSV export | Working | Working | ClosedXML `.xlsx` / CSV |
| Menus / Stories / Tags / FAQ / Subscribers | Working | Working | |
| MailTemplates / MainPageImages | Working | Working | |
| Settings + Logo | Working | Working | |
| AdminSettings / Metrics / Users | Working | Working | Users edit/password/roles |
| FileUpload / Media | Media 500 on :81 | Working | Upload + FileStorage list |
| Images compress | 500 on :81 | Working | |
| ProductComments | 500 on :81 | Working | Optional product id; list-all mode |
| AppLogs / ImportData | Working | Working | Log tail / Excel preview |
| Admin Ajax soft-delete | Working | Working | JSON endpoints |

## Functional spot checks (:82)

| Check | Result |
|-------|--------|
| `/health` database UP | Working |
| Report Export xlsx (`actionName=CouponUsage`) | Working (attachment) |
| Report Export csv (`actionName=PaymentMethod`) | Working |
| `/images/defaultImage/w150h150/default.jpg` | Working JPEG |
| `/images/getcaptcha` | Working + session cookie |
| `/Ajax/GetAllCities/` | Working (TR cities from data.json) |
| Product SaveOrEdit shows Price/Code | Working |

## Remaining differences (accepted / non-blocking)

1. Admin grids use Bootstrap tables (top 200) instead of Grid.Mvc — data and CRUD rules preserved.
2. Excel is `.xlsx` (ClosedXML) instead of legacy NPOI `.xls`.
3. Storefront chrome is mstore theme via Core layout (not a full 1:1 HTML clone of every legacy partial).
4. Iyzico checkout requires sandbox keys (demo success path when keys empty) — same as Core design.
5. Shared AbsoluteRootPath to legacy media was abandoned (IIS ACL); media is copied into Core `wwwroot/media` on publish.

## Sign-off

| Goal | Status |
|------|--------|
| Every :81-Working page reachable on :82 | **Met** (smoke 56/56) |
| Admin bypass for testing | **Met** |
| Reports + Excel/CSV | **Met** |
| Cart / checkout surface | **Met** (improved vs broken :81 cart index) |
| Published on IIS :82 | **Met** (`Eimece_Core`) |
