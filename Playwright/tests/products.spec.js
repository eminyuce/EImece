const { test, expect } = require('@playwright/test');
const { assertCrizalChrome, collectPageIssues, filterConsoleNoise } = require('./helpers');

test.describe('Crizal Products', () => {
  test('product category listing renders Crizal UI', async ({ page }) => {
    const issues = await collectPageIssues(page);
    const res = await page.goto('/c/pc/elektronik-7e7e4h1b/', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);
    await expect(page.locator('main#main-content')).toBeVisible();
    await page.screenshot({ path: 'screenshots/product-listing.png', fullPage: true });
    await page.waitForTimeout(800);
    expect(filterConsoleNoise(issues.consoleErrors)).toEqual([]);
  });

  test('product detail renders gallery and CTA area', async ({ page }) => {
    const res = await page.goto(
      '/p/kosu--fitness/fitlife-yoga-mati-6mm-133-8c0j2d5i4h1b/',
      { waitUntil: 'domcontentloaded' }
    );
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);
    await expect(page.locator('main#main-content')).toBeVisible();
    await page.screenshot({ path: 'screenshots/product-detail.png', fullPage: true });
  });
});
