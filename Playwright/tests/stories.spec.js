const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

test.describe('Crizal Stories', () => {
  test('stories index loads', async ({ page }) => {
    const res = await page.goto('/stories/', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);
    await page.screenshot({ path: 'screenshots/stories.png', fullPage: true });
  });
});
