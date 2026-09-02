# Playwright Chromium shopping e2e suite

- **Captured:** 2026-08-27 8:06:18 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Create a complete Playwright (TypeScript) end-to-end test suite that runs exclusively on Chromium for testing the full shopping experience on the Turkish e-commerce site http://ledampulburada.com/ with iyzico payment gateway (sandbox mode).

### Goals
1. Navigate the site, select a product, add it to cart, go through checkout, fill billing/shipping details, and complete a successful payment using iyzico test cards.
2. Also cover common failure scenarios (insufficient funds, invalid CVC, expired card, 3DS failure, etc.).
3. Handle possible iyzico integration styles: hosted Checkout Form (redirect / iframe / popup), direct card form, and 3D Secure challenge.
4. Make the tests resilient (auto-waiting, good selectors, network idle, frame handling).
5. Use Chromium only (project config with devices['Desktop Chrome']).
6. Include screenshots/videos on failure, traces, and clear assertions for success/failure states.

### Site details
- Base URL:  https://enlargement-army-authorization-syntax.trycloudflare.com/
- Language: Turkish (selectors should work with Turkish text: "Sepete Ekle", "Sepet", "Ödeme", "Siparişi Tamamla", etc.).
- Example cheap product (good for testing): 
  URL: https://enlargement-army-authorization-syntax.trycloudflare.com/p/mutfak/aquapure-cam-su-sisesi-750ml-106-2d0j7e0j4h1b
 
- Other products also exist (headphones, chargers, clothing, etc.). Prefer products that are in stock.

### iyzico Sandbox Test Cards (from official docs)
Successful cards (expiry any future date, CVV any 3 digits):
- 5526080000000006 (Akbank Mastercard Credit)
- 5890040000000016 (Akbank Mastercard Debit)
- 4543590000000006 (İş Bankası Visa Credit)
- 5400360000000003 (Garanti Mastercard Credit)
- 4603450000000000 (Denizbank Visa Credit)
- Many others listed in https://docs.iyzico.com/en/add-ons/test-cards

Error cards:
- 4111111111111129 → Not sufficient funds
- 4124111111111116 → Invalid cvc2
- 4125111111111115 → Expired card
- 4127111111111113 → Lost card
- 4151111111111112 → 3D Secure initialize failed
- etc.

Important notes:
- Expiry must be future (e.g. 12/30), CVV random valid (e.g. 123).
- LIVE cards are rejected in sandbox.
- 3DS challenge may appear as redirect, iframe, or popup. In sandbox it is often a mock page where you can simulate success/failure.

### Required test structure
Use Playwright Test runner with TypeScript.

Folder structure suggestion:
tests/
e2e/
shopping-happy-path.spec.ts
payment-failures.spec.ts
helpers/
cart.ts
checkout.ts
iyzico.ts
playwright.config.ts
text### Key technical requirements
- Use chromium only (or project named "chromium").
- Prefer page.getByRole(), getByText(), getByLabel() and data-testid if available. Fall back to CSS only when necessary.
- Handle iframes with page.frameLocator() (iyzico often uses iframes for card fields or 3DS).
- Wait for network idle or specific success indicators after payment.
- After successful payment assert order confirmation page / success message / order number appears.
- For 3DS: detect the challenge, interact if needed (sandbox mock often has a simple confirm button), then wait for redirect back to the merchant.
- Make tests independent (each test starts from a clean cart or clears cart).
- Add reasonable timeouts (checkout + 3DS can take 30–60 s).
- Use test.step() for readable reports.
- Environment variables for baseURL and any test credentials if login is required (guest checkout preferred).

### Happy-path test steps (implement this first)
1. Go to homepage or directly to a product page.
2. Select size/color/quantity if required.
3. Click "Sepete Ekle" (Add to Cart).
4. Go to cart / checkout.
5. Fill buyer information (name, surname, email, phone, address, city, etc. – use realistic Turkish test data).
6. Choose payment method that leads to iyzico (credit card / iyzico checkout form).
7. Fill card details with a successful test card (or let iyzico hosted form handle it).
8. Complete 3DS if it appears (sandbox mock).
9. Assert success: order confirmation, thank-you message, order ID visible, cart empty, etc.

### Failure cases to cover
- Insufficient funds
- Invalid CVC
- Expired card
- 3DS initialization failure
- (Optional) Cancel during 3DS

### Extra quality
- Page Object Model or helper functions for cart, checkout, payment.
- Soft assertions where useful.
- Screenshot + video + trace on failure.
- Clear console logs or comments explaining iyzico-specific waits.
- README with how to run: npx playwright test --project=chromium
- Note that the site must be in sandbox/test mode for iyzico; real cards must never be used.

Generate:
1. playwright.config.ts (Chromium only, good defaults)
2. Full happy-path test
3. Payment failure tests
4. Helper modules
5. Any necessary package.json scripts / dependencies note
6. Comments explaining iframe / 3DS handling specific to iyzico

Make the code production-ready, well-commented, and easy to extend.
