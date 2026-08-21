const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

test.describe('Crizal Critical Journeys', () => {
  test('home → product listing → product detail', async ({ page }) => {
    await page.goto('/');
    await assertCrizalChrome(page);

    await page.goto('/c/pc/elektronik-7e7e4h1b/', { waitUntil: 'domcontentloaded' });
    await assertCrizalChrome(page);
    await expect(page.locator('main#main-content')).toBeVisible();

    await page.goto('/p/kosu--fitness/fitlife-yoga-mati-6mm-133-8c0j2d5i4h1b/', { waitUntil: 'domcontentloaded' });
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
