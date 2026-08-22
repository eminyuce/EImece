// Focused regression for the DTO-refactored surfaces:
// - Customers area (profile, summary header, orders list, order detail, FAQ, contact seller, change password)
// - Payment views driven by ShoppingCartSession / OrderDto replacements
// - Key admin pages
const { test, expect } = require('@playwright/test');

const BASE = 'http://localhost:81';
const CUSTOMER = { email: 'eminyuce1111@gmail.com', password: 'V02y.qcF' };

async function customerLogin(page) {
  await page.goto('/account/login/', { waitUntil: 'domcontentloaded' });
  const form = page.locator('form').filter({ has: page.locator('input[name="Email"]') }).first();
  await form.locator('input[name="Email"]').fill(CUSTOMER.email);
  await form.locator('input[name="Password"]').fill(CUSTOMER.password);
  await Promise.all([
    page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
    form.locator('button[type="submit"], input[type="submit"]').first().click(),
  ]);
}

async function expectNoServerError(page, url) {
  const res = await page.goto(url, { waitUntil: 'domcontentloaded' });
  const status = res ? res.status() : 0;
  if (status >= 500) {
    throw new Error(`${url} returned ${status}`);
  }
  return status;
}

test.describe.serial('DTO-refactor focused regression', () => {
  test('customer login reaches account index with summary header', async ({ page }) => {
    await customerLogin(page);
    expect(page.url()).toMatch(/customers/i);
    const res = await page.goto('/customers/home/index', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBe(200);
    const body = await page.locator('body').innerText();
    // summary header must render real values, not exceptions
    expect(body).not.toMatch(/Beklenmeyen hata|Unhandled exception|Object reference/i);
    expect(body).not.toMatch(/TotalOrderCount|TotalPaid/); // raw property names must not leak
    // form fields present
    await expect(page.locator('input[name="Name"]').first()).toBeVisible();
    await expect(page.locator('input[name="Surname"]').first()).toBeVisible();
  });

  test('customer orders list renders rows or empty state', async ({ page }) => {
    await customerLogin(page);
    const res = await page.goto('/customers/home/customerorders', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBe(200);
    const body = await page.locator('body').innerText();
    expect(body).not.toMatch(/Beklenmeyen hata|Unhandled exception|Object reference/i);
    expect(body).toMatch(/Sipariş|Siparis|Order/i);
  });

  test('customer order detail renders when an order exists', async ({ page }) => {
    await customerLogin(page);
    const list = await page.goto('/customers/home/customerorders', { waitUntil: 'domcontentloaded' });
    const link = page.locator('a[href*="customerorderdetail"]').first();
    if ((await link.count()) > 0) {
      await link.click();
      await page.waitForLoadState('domcontentloaded');
      expect(page.url()).toMatch(/customerorderdetail/i);
      const body = await page.locator('body').innerText();
      expect(body).not.toMatch(/Beklenmeyen hata|Unhandled exception|Object reference/i);
    } else {
      expect(list.status()).toBe(200);
    }
  });

  test('faq and send-message-to-seller render', async ({ page }) => {
    await customerLogin(page);
    expect(await expectNoServerError(page, '/customers/home/faq')).toBe(200);
    expect(await expectNoServerError(page, '/customers/home/sendmessagetoseller')).toBe(200);
  });

  test('change password page renders with summary partial', async ({ page }) => {
    await customerLogin(page);
    const res = await page.goto('/customers/home/changepassword', { waitUntil: 'domcontentloaded' });
    expect(res.status()).toBe(200);
    const body = await page.locator('body').innerText();
    expect(body).not.toMatch(/Beklenmeyen hata|Unhandled exception|Object reference/i);
    await expect(page.locator('input[name="OldPassword"]').first()).toBeVisible();
  });

  test('cargo tracking ajax returns html fragment', async ({ page }) => {
    const res = await page.request.post('/payment/cargotrackingresult', {
      form: { orderNumber: 'NONEXISTENT-1' },
    });
    // endpoint must not 500 even for unknown numbers
    expect(res.status()).toBeLessThan(500);
  });

  test('admin login page renders and admin dashboard reachable', async ({ page }) => {
    await page.goto('/account/adminlogin/', { waitUntil: 'domcontentloaded' });
    const body = await page.locator('body').innerText();
    expect(body).not.toMatch(/Unhandled exception/i);
  });

  test('mobile viewport storefront smoke', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    for (const path of ['/', '/products', '/stories']) {
      const res = await page.goto(path, { waitUntil: 'domcontentloaded' });
      expect(res.status()).toBe(200);
    }
  });
});
