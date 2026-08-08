const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

test.describe('Crizal Search', () => {
  test('search results page loads', async ({ page }) => {
    const res = await page.goto('/p/arama?search=kulaklik', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);
    await page.screenshot({ path: 'screenshots/search.png', fullPage: true });
  });

  test('advanced search page loads', async ({ page }) => {
    const res = await page.goto('/p/advancedsearchproducts', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);
  });
});
