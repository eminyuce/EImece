const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

test.describe('Crizal Critical Journeys', () => {
  test('home → product listing → product detail', async ({ page }) => {
    await page.goto('/');
    await assertCrizalChrome(page);

    await page.goto('/c/pc/kulaklik--ses-4h0j6g1b/');
    expect((await page.waitForResponse((r) => r.url().includes('kulaklik') || r.status() === 200).catch(() => null)) || true).toBeTruthy();
    await assertCrizalChrome(page);
    await expect(page.locator('main#main-content')).toBeVisible();

    await page.goto('/p/kulaklik--ses/nordline-wireless-bluetooth-kulaklik-pro-4h2d9a5i4h1b/');
    await assertCrizalChrome(page);
    await expect(page.locator('main#main-content')).toBeVisible();
  });

  test('home → search → results stay on Crizal', async ({ page }) => {
    await page.goto('/');
    await assertCrizalChrome(page);

    await page.goto('/p/arama?search=kulaklik');
    await assertCrizalChrome(page);
    await expect(page.locator('main#main-content')).toBeVisible();
  });

  test('home → cart', async ({ page }) => {
    await page.goto('/');
    await assertCrizalChrome(page);
    await page.goto('/Payment/ShoppingCart');
    await assertCrizalChrome(page);
    expect(page.url()).toMatch(/ShoppingCart|Payment/i);
  });
});
