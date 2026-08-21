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

  async function assertMobileMenuCoversPageTitle(page, path) {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto(path);
    await assertCrizalChrome(page);

    const title = page.locator('h1.crizal-story-page__title, h1.page-title, main h1').first();
    await expect(title).toBeVisible({ timeout: 15_000 });

    await page.locator('.navbar-toggler').first().click();
    await expect(page.locator('#nav.open, .navbar-nav.open').first()).toBeVisible({ timeout: 5_000 });

    // Stacking guard: when the menu is open, main must sit under the header layer.
    await expect.poll(async () => page.evaluate(() => getComputedStyle(document.querySelector('main')).zIndex)).toBe('0');
    await expect.poll(async () => page.evaluate(() => document.body.classList.contains('crizal-nav-open'))).toBe(true);

    const hit = await page.evaluate(() => {
      const titleEl = document.querySelector('h1.crizal-story-page__title, h1.page-title, main h1');
      const nav = document.querySelector('#nav.open, .navbar-nav.open');
      if (!titleEl || !nav) return { ok: false, reason: 'missing-nodes' };
      const tr = titleEl.getBoundingClientRect();
      const nr = nav.getBoundingClientRect();
      // Only assert when the title geometrically sits inside the open menu panel.
      const overlaps = tr.top < nr.bottom && tr.bottom > nr.top && tr.left < nr.right && tr.right > nr.left;
      if (!overlaps) return { ok: true, skipped: true };
      const el = document.elementFromPoint(tr.left + Math.min(40, tr.width / 2), tr.top + tr.height / 2);
      const inNav = !!(el && (el.closest('#nav') || el.closest('.navbar-nav')));
      return {
        ok: inNav,
        skipped: false,
        hit: el && { tag: el.tagName, cls: String(el.className || '').slice(0, 60) },
      };
    });

    if (!hit.skipped) {
      expect(hit.ok, `page title painted above mobile menu on ${path}: ${JSON.stringify(hit.hit)}`).toBeTruthy();
    }
  }

  test('mobile menu covers page title on home/content pages', async ({ page }) => {
    await page.goto('/');
    await assertCrizalChrome(page);

    // Prefer a story/tag page that renders a large in-flow page title under the header.
    const titleLink = page.locator('a[href*="/s/t/"], a[href*="/s/sc/"], a[href*="/s/"], a[href*="/c/"], a[href*="/stories"]').first();
    if (await titleLink.count()) {
      const href = await titleLink.getAttribute('href');
      await page.goto(href, { waitUntil: 'domcontentloaded' });
      await assertMobileMenuCoversPageTitle(page, page.url());
      return;
    }

    await assertMobileMenuCoversPageTitle(page, '/');
  });

  test('mobile menu covers page title on story category pages', async ({ page }) => {
    await page.goto('/');
    await assertCrizalChrome(page);

    const categoryLink = page.locator('a[href*="/s/sc/"]').first();
    test.skip(!(await categoryLink.count()), 'No story category link available in this environment');
    const href = await categoryLink.getAttribute('href');
    await assertMobileMenuCoversPageTitle(page, href);
  });
});
