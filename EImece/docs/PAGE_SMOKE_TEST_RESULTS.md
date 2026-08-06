# Page smoke test results

Generated against `http://127.0.0.1:81` (IIS site **Eimece**).

Config was temporarily toggled for admin tests, then restored to:

- `SiteStatus=live`
- `AdminLoginEnabled=true`
- `BypassAdminAuth=false`
- `customErrors=RemoteOnly`

---

## Summary

| Area | Result |
| --- | --- |
| Health | `/health` → **200** `{"status":"UP"}` |
| Public core pages | Home, cart, account, RSS, sitemap, robots → **OK** |
| Content pages from home | Categories, info pages, 1 product → **OK**; 2 products → **500** |
| Admin login **disabled** (`AdminLoginEnabled=false`) | `/account/adminlogin` and `/admin/*` redirect to **home** (expected) |
| Admin panel (with `SiteStatus=dev` + `BypassAdminAuth=true`) | **27/29** Index pages **200** |
| Language switch | `/home/language/tr` and `/en` → **500** |

---

## Public pages

| Status | Path | Notes |
| ---: | --- | --- |
| 200 | `/` | Home |
| 200 | `/health`, `/healthz` | Health |
| 200 | `/robots.txt`, `/sitemap.xml` | SEO |
| 200 | `/underconstruction` | |
| 200 | `/account/login`, `/register`, `/forgotpassword` | |
| 200 | `/account/adminlogin` | Login form (when enabled) |
| 200 | `/payment/shoppingcart`, `/shoppingwithoutaccount`, `/cargotracking` | |
| 200 | `/rss/products` | |
| 200 | `/products/advancedsearchproducts` | |
| 500 | `/home/language/tr`, `/home/language/en` | `CultureNotFoundException`: culture id `0` in `BaseController.SetLanguage` |
| 404 | `/products/searchproducts?search=test` | Route is `/p/...` style / attribute route — bare path not registered |
| 404 | `/c/`, `/s/`, `/p/` | Empty SEO prefixes (expected without id) |
| 404 | `/images/logo.jpg`, `/error/notfound` | May be intentional / custom error handling |

### Content links scraped from home

| Status | Path |
| ---: | --- |
| 200 | `/c/pc/kudret-nari-viyolu-3f2d7e1b/` |
| 200 | `/c/pc/taze-kudret-nari-7e2d7e1b/` |
| 200 | `/i/galerimiz-7e7e1b6g/` |
| 200 | `/i/iletisim-3f7e1b6g/` |
| 200 | `/i/iptal-ve-iade-sartlari-0j2d1b6g/` |
| 200 | `/i/mesafeli-satis-sozlesmesi-9a7e1b6g/` |
| 200 | `/p/kudret-nari-viyolu/yetistirmeye-hazir-kudret-nari-tohumu-6g2d3f6g4h1b/` |
| 500 | `/p/taze-kudret-nari--15-kg/taze-kudret-nari--15-kilo-2d2d3f6g4h1b/` |
| 500 | `/p/taze-kudret-nari--1-kg/taze-kudret-nari--1-kilo-8c2d3f6g4h1b/` |

---

## Disabled admin panel (`AdminLoginEnabled=false`)

With `BypassAdminAuth=false` and `SiteStatus=live`:

| Request | Outcome |
| --- | --- |
| `/account/adminlogin` | **200** final URL `/` (home) — login panel disabled |
| `/admin` | **200** final URL `/` |
| `/admin/dashboard` | **200** final URL `/` |
| `/admin/products` | **200** final URL `/` |

This matches `AccountController` / `AuthorizeRolesAttribute`: when admin login is off, unauthenticated admin traffic is sent to the storefront home instead of the login page.

---

## Admin panel pages (auth bypass for smoke test)

Temporary settings used only for this pass: `SiteStatus=dev`, `BypassAdminAuth=true`, `AdminLoginEnabled=true`.

| Status | Path | Notes |
| ---: | --- | --- |
| 200 | `/admin`, `/admin/dashboard` | |
| 200 | `/admin/products`, `/productcategories`, `/brands` | |
| 200 | `/admin/orders`, `/customers`, `/shoppingcarts`, `/coupons` | |
| 200 | `/admin/subscribers`, `/stories`, `/storycategories` | |
| 200 | `/admin/menus`, `/faq`, `/mailtemplates`, `/templates`, `/lists` | |
| 200 | `/admin/tags`, `/tagcategories`, `/mainpageimages` | |
| 200 | `/admin/adminsettings`, `/users`, `/applogs`, `/report`, `/metrics` | |
| 200 | `/admin/importdata`, `/fileupload` | |
| 404 | `/admin/settings` | No `Index` action (`SettingsController` is logo/upload helpers only) |
| 500 | `/admin/productcomments` | Requires product `id` (`Index(int id)`); bare URL invalid |

With a product id, `/admin/productcomments?id=1` redirected (**302**) under bypass (likely missing product / auth flow) — treat as needs a real product id from admin Products UI.

---

## Issues to fix (optional follow-up)

1. **Language switch 500** — `SetLanguage` expects an **enum int** (`EImeceLanguage`), not culture codes. `/home/language/tr` → `ToInt()` → `0` → `CultureNotFoundException`. Use `/home/language/1` (etc.) as the UI does, or harden `SetLanguage` against bad ids.
2. **Two product detail pages 500** — URLs with double hyphen (`taze-kudret-nari--1-kg` / `--15-kg`).
3. **Admin ProductComments** — list URL requires product id: `/admin/productcomments?id={productId}` (bare URL is invalid).

---

## How admin disable works

- `AdminLoginEnabled=false` → AdminLogin GET/POST redirect to home; admin authorize filter also blocks unauthenticated `/admin` to home.
- `BypassAdminAuth` only works when `SiteStatus` is **not** live (hard-disabled in production).
