# EImece.Web — Security checklist (Phase 9)

Use before production cutover of the ASP.NET Core host.

## Secrets & configuration

- [ ] No real SQL / Iyzico / SMTP / OAuth secrets in git (`appsettings*.json` placeholders only)
- [ ] Production connection string via `EIMECE_DB_CONNECTION_STRING`, user-secrets, or vault
- [ ] `EImece:BypassAdminAuth` is **false**
- [ ] `EImece:AdminLoginEnabled` set intentionally
- [ ] OAuth providers only registered when ClientId+Secret are non-empty (already gated)
- [ ] DataProtection keys persisted under `App_Data/DataProtection-Keys` and **not** committed (gitignored)

## Cookies & CSRF

- [ ] Auth cookie `HttpOnly`, `SameSite=Lax`, `SecurePolicy=Always` outside Development
- [ ] Session cookie `HttpOnly` + `SecurePolicy=SameAsRequest`
- [ ] Antiforgery on Account login/logout and Payment `PlaceOrder`
- [ ] Iyzico `PaymentResult` callback remains antiforgery-exempt (external POST) — validate token via Iyzico retrieve API only

## Headers & transport

- [ ] Reverse proxy forwards `X-Forwarded-Proto` / `X-Forwarded-For`
- [ ] HSTS enabled in non-Development
- [ ] Security headers present: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` (`SecurityHeadersMiddleware`)
- [ ] HTTPS terminated at proxy or Kestrel certificate configured

## AuthZ

- [ ] Admin area requires `Admin` / `NormalUser` policies (`AdminOnly` / `AdminOrEditor`)
- [ ] Customers area requires `CustomerOnly`
- [ ] Dev-only endpoints (`POST /api/integrations/email/test`) return 404 outside Development

## Media & uploads

- [ ] Media paths resolved under media root (path-traversal guard in `MediaFileService`)
- [ ] Uploaded filenames sanitized (`Path.GetFileName`) before write
- [ ] Captcha session not treated as sole spam control for public forms in production (prefer reCAPTCHA when enabled)

## Logging & PII

- [ ] NLog does not log full card data or Iyzico secrets
- [ ] SMTP log-sink does not print passwords
- [ ] Correlation id middleware used for support without exposing stack traces to clients in Production

## Residual risks (tracked)

- Full CSP not yet applied (parity with legacy SecurityHeadersHttpModule)
- Cart/order persistence still partial — do not expose live checkout without order write-path review
- WebPush / Excel admin import not on Core host yet
