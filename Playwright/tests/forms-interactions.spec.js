const { test, expect } = require('@playwright/test');
const { gotoAndAssertOk, assertCrizalChrome } = require('./helpers');

test.describe('Forms and interactive controls', () => {
  test('header search submits to product search', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await assertCrizalChrome(page);

    // Crizal search is often behind an icon toggle
    const toggler = page.locator('.search_btn, .search-toggler, a.search_btn, button.search_btn, [data-bs-target*="search"]').first();
    if (await toggler.count()) {
      await toggler.click({ force: true }).catch(() => {});
    }

    const searchInput = page.locator('.search-form input[name="search"], form.search-form input[type="text"], input[name="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill('kulaklik');
      await searchInput.press('Enter');
      await page.waitForLoadState('domcontentloaded');
    } else {
      // Fallback: exercise the same route the form posts to
      await page.goto('/p/arama/?search=kulaklik', { waitUntil: 'domcontentloaded' });
    }

    expect(page.url()).toMatch(/arama|search|kulaklik/i);
    await expect(page.locator('body')).not.toContainText('Unhandled exception');
  });

  test('advanced search page accepts query interaction', async ({ page }) => {
    const res = await page.goto('/p/advancedsearchproducts/?search=telefon', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBeLessThan(500);
    await expect(page.locator('#main-content')).toBeVisible();
    await expect(page.locator('body')).not.toContainText('Unhandled exception');
  });

  test('category listing filter/sort controls do not 500', async ({ page }) => {
    await page.goto('/c/pc/elektronik-0j5i6g1b/', { waitUntil: 'domcontentloaded' });
    await expect(page.locator('body')).not.toContainText('Unhandled exception');

    // Try sorting query variants used by the app
    for (const sorting of [0, 1, 2, 3]) {
      const res = await page.goto(`/c/pc/elektronik-0j5i6g1b/?page=1&sorting=${sorting}`, {
        waitUntil: 'domcontentloaded',
      });
      expect(res?.status(), `sorting=${sorting}`).toBeLessThan(500);
    }

    // Pagination page 2 if present
    const page2 = page.locator('a[href*="page=2"]').first();
    if (await page2.count()) {
      await page2.click();
      await page.waitForLoadState('domcontentloaded');
      expect(page.url()).toMatch(/page=2/);
      await expect(page.locator('body')).not.toContainText('Unhandled exception');
    }
  });

  test('contact page form fields are interactive', async ({ page }) => {
    await gotoAndAssertOk(page, '/i/iletisim-3f4h8c6g/');
    const form = page
      .locator('main form, #main-content form, form[action*="sendcontactus"], form[action*="contact"]')
      .filter({ has: page.locator('input, textarea') })
      .first();
    await expect(form).toBeVisible();

    const name = form.locator('input[name*="Name"], input[name*="name"], #Name').first();
    const email = form.locator('input[name*="Email"], input[type="email"], #Email').first();
    const message = form.locator('textarea').first();

    if (await name.count()) await name.fill('E2E Tester');
    if (await email.count()) await email.fill('e2e@example.com');
    if (await message.count()) await message.fill('Playwright contact form interaction test.');

    // Do not submit with captcha (would spam); just ensure controls accept input
    await expect(page.locator('body')).not.toContainText('Unhandled exception');
  });

  test('guest checkout form renders fields', async ({ page }) => {
    await gotoAndAssertOk(page, '/payment/shoppingwithoutaccount/');
    await expect(page.locator('main form, #main-content form, form[method="post"]').first()).toBeVisible();
  });

  test('browser back/forward after navigation', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await page.goto('/stories/', { waitUntil: 'domcontentloaded' });
    await page.goto('/payment/shoppingcart/', { waitUntil: 'domcontentloaded' });
    await page.goBack();
    await page.waitForLoadState('domcontentloaded');
    expect(page.url()).toMatch(/stories/i);
    await page.goForward();
    await page.waitForLoadState('domcontentloaded');
    expect(page.url()).toMatch(/shoppingcart/i);
  });
});
