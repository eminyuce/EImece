const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

test.describe('Crizal Checkout', () => {
  test('checkout billing redirects or loads without 500', async ({ page }) => {
    // Empty cart may redirect — follow redirects and ensure no 500 + Crizal chrome on final page
    const res = await page.goto('/Payment/CheckoutBillingDetails', {
      waitUntil: 'domcontentloaded',
    });
    expect(res?.status() ?? 500).toBeLessThan(500);
    // Land on a Crizal page (checkout form or redirect target such as cart/login)
    await page.waitForSelector('body[data-design="crizal"]', { timeout: 15_000 });
  });

  test('shopping without account page is Crizal', async ({ page }) => {
    const res = await page.goto('/Payment/ShoppingWithoutAccount', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);
    await page.screenshot({ path: 'screenshots/checkout-guest.png', fullPage: true });
  });
});
