# EImece.Web — Functional verification checklist

Manual + automated smoke for the Core host. Full commerce parity with MVC5 is still incremental.

## Automated (CI / local)

```bash
./scripts/verify-core.sh
```

Covers: Debug build, xUnit smoke (`/health`, images, captcha, home, checkout shell, email sink, security headers).

## Manual storefront path

| Step | URL / action | Expected |
|------|----------------|----------|
| 1 Health | `GET /health` | `status=UP`; note `database`, `integrations` |
| 2 Home | `GET /` | Hero + mstore CSS (200 on `/Content/mstore/css/theme.min.css`) |
| 3 Search | `GET /p/arama/?q=test` | Search shell |
| 4 Category | `GET /c/pc/1/` | Category shell (DB optional) |
| 5 Product | `GET /p/{slug}/{id}/` | Detail shell |
| 6 Image | `GET /images/defaultImage/w150h150/default.jpg` | JPEG bytes |
| 7 Captcha | `GET /images/getcaptcha` | JPEG + session cookie |
| 8 Cart | `GET /Payment/` | Cart shell |
| 9 Checkout | `GET /Payment/Checkout/` | Demo basket; Place order disabled without Iyzico keys |
| 10 Login | `GET /Account/Login/` | Form + antiforgery |

## Checkout (sandbox)

| Step | Action | Expected |
|------|--------|----------|
| 1 | Set `Iyzico:ApiKey` + `SecretKey` (sandbox) | `/health` → `integrations.iyzico.configured=true` |
| 2 | `POST /Payment/PlaceOrder` | Checkout Form HTML embedded |
| 3 | Complete sandbox payment | Callback `PaymentResult` shows success/failure |
| 4 | Email | Log sink or real SMTP confirmation body |

> Order row persistence / full cart session remain Phase 8 follow-ups — verify against staging DB before go-live.

## Admin / Customers

| Step | Action | Expected |
|------|--------|----------|
| 1 | `GET /Account/AdminLogin/` | Admin auth layout |
| 2 | Sign in as Admin | Access `/Admin/` dashboard shell |
| 3 | Confirm `BypassAdminAuth=false` in staging/prod | Unauthenticated `/Admin/` redirects to login |
| 4 | Customer role | `/Customers/` requires Customer policy |

## Performance spot checks

- [ ] Home HTML compressed when `Accept-Encoding: br` or `gzip` (non-trivial size)
- [ ] Repeated `/images/w150h150/{id}.jpg` served quickly (memory cache)
- [ ] `/health` remains cheap when SQL is down

## Sign-off

| Environment | Build | Smoke tests | Manual checklist | Approver | Date |
|-------------|-------|-------------|------------------|----------|------|
| Dev | ☐ | ☐ | ☐ | | |
| Staging | ☐ | ☐ | ☐ | | |
| Production | ☐ | ☐ | ☐ | | |
