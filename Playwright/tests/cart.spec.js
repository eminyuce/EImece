const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

test.describe('Crizal Cart', () => {
  test('shopping cart page loads with Crizal layout', async ({ page }) => {
    const res = await page.goto('/Payment/ShoppingCart', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);
    await expect(page.locator('main#main-content')).toBeVisible();
    await page.screenshot({ path: 'screenshots/cart.png', fullPage: true });
  });
});
