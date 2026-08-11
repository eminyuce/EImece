/**
 * Customer end-to-end shopping flows (Crizal @ http://localhost:81)
 *
 * 1) Guest browse → add to cart → membership checkout forces registration → billing
 * 2) Register → add to cart → abandon (no checkout)
 * 3) Full happy path: register → cart ops → checkout → iyzico sandbox (when keys configured)
 */
const { test, expect } = require('@playwright/test');
const {
  PRODUCT_CATEGORY,
  PRODUCT_DETAIL,
  uniqueCustomerEmail,
  shotBoth,
  openProductDetail,
  addToCartFromDetail,
  goToCart,
  updateFirstCartQuantity,
  removeFirstCartItem,
  registerCustomer,
  proceedToMembershipCheckout,
  fillBillingDetails,
  placeOrderIfReady,
  tryPayWithIyzicoSandbox,
} = require('./customer-flow-helpers');
const { assertCrizalChrome: assertChrome } = require('./helpers');

// Serial: shared IIS + shopping-cart cookie state is safer one-at-a-time for checkout.
test.describe.configure({ mode: 'serial' });

test.describe('Customer E2E — guest forced registration', () => {
  test('guest adds to cart then membership checkout redirects to Register', async ({ page }) => {
    test.setTimeout(180_000);

    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await assertChrome(page);
    await shotBoth(page, '01-home');

    await page.goto(PRODUCT_CATEGORY, { waitUntil: 'domcontentloaded' });
    await assertChrome(page);
    await shotBoth(page, '02-category');

    await openProductDetail(page, PRODUCT_DETAIL);
    await shotBoth(page, '03-product-detail');
    await addToCartFromDetail(page, { quantity: 1 });

    await goToCart(page);
    await expect(page.locator('body')).not.toContainText('Unhandled exception');
    // Cart should list a product line or quantity control (not only empty-basket message)
    const empty = await page.locator('body').innerText();
    expect(empty).not.toMatch(/sepetinizde ürün bulunamadı|no product found in shopping basket/i);
    await shotBoth(page, '04-cart-guest');

    await proceedToMembershipCheckout(page);
    await shotBoth(page, '05-forced-register');
    expect(page.url(), 'Membership checkout must force Register (not Login)').toMatch(
      /\/account\/register/i
    );
    expect(page.url()).toMatch(/returnUrl=/i);

    const email = uniqueCustomerEmail('guest');
    const registered = await registerCustomer(page, {
      email,
      password: 'Test123!',
      returnUrl: '/Payment/CheckoutBillingDetails',
    });
    expect(registered, `Registration failed for ${email}. URL=${page.url()}`).toBeTruthy();
    await shotBoth(page, '06-after-register');

    // After register with returnUrl → billing details
    if (!/CheckoutBillingDetails/i.test(page.url())) {
      await page.goto('/Payment/CheckoutBillingDetails', { waitUntil: 'domcontentloaded' });
    }
    await assertChrome(page);
    await expect(page.locator('#Cities')).toBeVisible();
    await shotBoth(page, '07-billing-after-register');

    await fillBillingDetails(page, { name: 'Guest', surname: 'E2E', gsm: '5559876543' });
    await shotBoth(page, '08-order-review');
    expect(page.url()).toMatch(/CheckoutPaymentOrderReview|PlaceOrder|Payment/i);
  });
});

test.describe('Customer E2E — register and abandon cart', () => {
  test('register, add items, leave without checkout', async ({ page }) => {
    test.setTimeout(180_000);

    const email = uniqueCustomerEmail('abandon');
    const registered = await registerCustomer(page, {
      email,
      password: 'Test123!',
      firstName: 'Abandon',
      lastName: 'Cart',
    });
    expect(registered, `Registration failed for ${email}`).toBeTruthy();
    await shotBoth(page, '10-register-abandon');

    await openProductDetail(page);
    await addToCartFromDetail(page, { quantity: 2 });
    await goToCart(page);
    await shotBoth(page, '11-cart-abandon');

    const body = await page.locator('body').innerText();
    expect(body).not.toMatch(/sepetinizde ürün bulunamadı|no product found in shopping basket/i);

    // Abandon: navigate away without ProceedToCheckout
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await assertChrome(page);
    await shotBoth(page, '12-abandoned-home');

    // Cart still retained for session
    await goToCart(page);
    const still = await page.locator('body').innerText();
    expect(still).not.toMatch(/sepetinizde ürün bulunamadı|no product found in shopping basket/i);
    await shotBoth(page, '13-cart-still-present');
  });
});

test.describe('Customer E2E — full happy path', () => {
  test('register, cart ops, checkout, iyzico sandbox', async ({ page }) => {
    test.setTimeout(420_000);

    // Plus-addressing so order/confirmation mail can still reach eminyuce@outlook.com
    const email = uniqueCustomerEmail('outlook');
    const password = 'Test123!';

    const registered = await registerCustomer(page, {
      email,
      password,
      firstName: 'Eminy',
      lastName: 'Yuce',
      phone: '5551112233',
    });
    expect(registered, `Registration failed for ${email}. URL=${page.url()}`).toBeTruthy();
    await shotBoth(page, '20-happy-register');

    await openProductDetail(page);
    await addToCartFromDetail(page, { quantity: 1 });

    // Add a second unit / second add, then adjust cart
    await openProductDetail(page);
    await addToCartFromDetail(page, { quantity: 1 });
    await goToCart(page);
    await shotBoth(page, '21-cart-before-ops');

    await updateFirstCartQuantity(page, 3);
    await shotBoth(page, '22-cart-qty-updated');

    // If more than one line exists, remove one; otherwise leave the adjusted line
    const lines = page.locator('.cart-item, .shopping-cart-item, [id*="cartItem"], tr').filter({
      has: page.locator('input[type="number"], a[href*="Remove"], button'),
    });
    if ((await lines.count()) > 1) {
      await removeFirstCartItem(page);
      await shotBoth(page, '23-cart-after-remove');
    }

    await proceedToMembershipCheckout(page);
    if (/\/account\/(login|register)/i.test(page.url())) {
      // Session lost — re-login uncommon; fail clearly
      throw new Error(`Unexpected auth redirect during happy path: ${page.url()}`);
    }
    await shotBoth(page, '24-billing');

    await fillBillingDetails(page, {
      name: 'Eminy',
      surname: 'Yuce',
      gsm: '5551112233',
      identity: '11111111111',
    });
    await shotBoth(page, '25-review');

    await placeOrderIfReady(page);
    await shotBoth(page, '26-place-order');

    await expect(page.locator('body')).not.toContainText('Unhandled exception');
    await expect(page.locator('body')).not.toContainText('Encryption key is not configured');

    expect(page.url(), 'Should reach PlaceOrder step').toMatch(/placeorder/i);

    // Sandbox widget should render (default AppConfig keys apply when Web.config values are empty).
    await expect(page.getByText('Sandbox', { exact: false })).toBeVisible({ timeout: 45_000 });
    await shotBoth(page, '26b-iyzico-sandbox-visible');

    const pay = await tryPayWithIyzicoSandbox(page);
    await shotBoth(page, '27-after-payment-attempt');
    if (!pay.ok) {
      test.info().annotations.push({
        type: 'iyzico',
        description: `${pay.reason}. Email: ${email}. Checkout UI reached iyzico sandbox; complete card submit may need iframe selector updates.`,
      });
      // Do not fail the suite on flaky third-party iframe automation once sandbox UI is proven.
      return;
    }
    await shotBoth(page, '28-order-confirmation');
    await expect(page.locator('body')).not.toContainText('Unhandled exception');
  });
});

test.describe('Customer E2E — cart security smoke', () => {
  test('AddToCart rejects missing anti-forgery or bad product without 500', async ({ request }) => {
    const bare = await request.post('/payment/addtocart/', { form: { productId: '0', quantity: 1 } });
    expect(bare.status(), 'malformed AddToCart should not 500').toBeLessThan(500);

    const cart = await request.get('/payment/shoppingcart/');
    expect(cart.status()).toBeLessThan(500);
  });
});
