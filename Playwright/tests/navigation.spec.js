const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

test.describe('Crizal Navigation', () => {
  test('header search toggle and primary links work', async ({ page }) => {
    await page.goto('/');
    await assertCrizalChrome(page);

    // Search icon in attr-nav — template toggles .top-search open
    const searchToggle = page.locator('li.search a').first();
    if (await searchToggle.count()) {
      await searchToggle.click();
      await expect(page.locator('.top-search')).toBeVisible({ timeout: 5_000 });
      await expect(page.locator('.top-search input[type="text"], .top-search input[name="search"], .search-form input[type="text"]').first()).toBeVisible();
    }

    // Primary nav home link
    const homeLink = page.locator('nav a, .navbar a, .navbar-nav a').filter({ hasText: /ana\s*sayfa|home/i }).first();
    if (await homeLink.count()) {
      await homeLink.click();
      await page.waitForLoadState('domcontentloaded');
      await assertCrizalChrome(page);
    }

    // Cart icon link should navigate
    const cartLink = page.locator('a[href*="ShoppingCart"], #ShoppingCartLink, .attr-nav a[href*="Payment"]').first();
    if (await cartLink.count()) {
      await cartLink.click();
      await page.waitForLoadState('domcontentloaded');
      await assertCrizalChrome(page);
    }
  });

  test('mobile nav toggler is present at 375px', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto('/');
    await assertCrizalChrome(page);
    await expect(page.locator('.navbar-toggler').first()).toBeVisible();
  });
});
