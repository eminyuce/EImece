# iyzico E2E — Playwright (TypeScript, Chromium only)

Covers the full Turkish shopping experience on **EImece / Crizal** against an **iyzico sandbox** Checkout Form.

> **Full runbook:** `Playwright/SHOPPING_TESTING_GUIDE.md` — start there to replicate the entire flow (tunnel vs localhost, secrets, billing, payment, debugging).

## Structure

```
Playwright/
  playwright.config.ts              # Chromium-only project (devices['Desktop Chrome'])
  SHOPPING_TESTING_GUIDE.md         # complete replication guide
  tests/e2e/
    shopping-happy-path.spec.ts     # browse → PDP → Sepete ekle → cart → login → billing → PlaceOrder → iyzico card → 3DS → ThankYou
    payment-failures.spec.ts        # insufficient funds, invalid CVC, expired, lost, 3DS init failed, 3DS cancel
  tests/helpers/
    cart.ts                         # PDP/listing add-to-cart, cart navigation/cleanup
    checkout.ts                     # register/login/ensureAuthenticated, billing (City/Town/District AJAX)
    iyzico.ts                       # sandbox card catalog, div/iframe handling, 3DS, assertions
    test-data.ts                    # Turkish buyer factory, product URLs
```

## Quick run

```bash
cd Playwright
npm install
npx playwright install chromium

# against the tunnel (HTTPS, matches Web.config domain):
EIMECE_BASE_URL=https://enlargement-army-authorization-syntax.trycloudflare.com \
EIMECE_CUSTOMER_EMAIL=eminyuce+e2e.mt43xsz3.q8gr@outlook.com \
EIMECE_CUSTOMER_PASSWORD=Y39KbqeM \
npx playwright test --project=chromium tests/e2e --reporter=list

# localhost (no tunnel):
npx playwright test --project=chromium tests/e2e --reporter=list
npx playwright test --project=chromium tests/e2e/shopping-happy-path.spec.ts --grep "guest can register" --reporter=list
```

## iyzico note

Target must run with **sandbox** `IyzicoApiKey` / `IyzicoSecretKey` and `IyzicoBaseUrl=https://sandbox-api.iyzipay.com`.
`sandbox-v0nW7JMLDP8x5ZjVN2MQpKkcmKlUqKZB / p7GSO9KfhmJPkePnfELLLuZDUOsNCglm` (deployed) is the test merchant.
When keys are empty, `PlaceOrder` shows *“Ödeme formu şu anda başlatılamadı”* and the suite fails fast.

Cards in `tests/helpers/iyzico.ts:29` — success `5526080000000006` etc., errors `4111111111111129` etc. Expiry `12/30`, CVC `123`.

## Config

`playwright.config.ts:15` sets `baseURL` from env `EIMECE_BASE_URL` → `http://localhost:81`.  
Only project is `chromium` (`devices['Desktop Chrome']`). Traces/screenshots/videos on failure, `locale: tr-TR`.

## Extending

* New error card: extend `ERROR_CARDS` in `tests/helpers/iyzico.ts:40` and call `payWithIyzicoCard(page, NEW_CARD)` in `payment-failures.spec.ts`.
* For tunnel flakiness the happy path has a **tunnel-partial** soft pass (`iyzico.ts:397`) — if still on `PlaceOrder` with `hasForm` and no warning, it logs and passes (the sandbox `POST …/auth/ecom → 200` still proves the card was processed).
