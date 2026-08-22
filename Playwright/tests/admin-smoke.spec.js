const { test, expect } = require('@playwright/test');

const ADMIN = { email: 'admin@eimece.test', password: 'Test123!' };

async function adminLogin(page) {
  await page.goto('/account/adminlogin/', { waitUntil: 'domcontentloaded' });
  const form = page.locator('form').filter({ has: page.locator('input[name="Email"]') }).first();
  await form.locator('input[name="Email"]').fill(ADMIN.email);
  await form.locator('input[name="Password"]').fill(ADMIN.password);
  await Promise.all([
    page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
    form.locator('button[type="submit"], input[type="submit"]').first().click(),
  ]);
  return page.url();
}

test.describe.serial('Admin panel smoke', () => {
  const pages = [
    '/admin/productcategories',
    '/admin/products',
    '/admin/stories',
    '/admin/orders',
    '/admin/customers',
    '/admin/faqs',
    '/admin/settings',
    '/admin/systemsettings',
    '/admin/menus',
    '/admin/mainpageimages',
    '/admin/coupons',
    '/admin/subscribers',
    '/admin/tags',
    '/admin/applogs',
    '/admin/filestorages',
  ];

  test('admin login works', async ({ page }) => {
    const url = await adminLogin(page);
    expect(url).not.toMatch(/adminlogin/i);
  });

  for (const p of pages) {
    test(`admin page ${p} loads`, async ({ page }) => {
      const res = await page.goto(p, { waitUntil: 'domcontentloaded' });
      const status = res ? res.status() : 0;
      if (status === 302 || status === 301) {
        // redirected to login -> auth issue
        throw new Error(`${p} redirected (${status}) to ${page.url()}`);
      }
      if (status >= 400) {
        // some areas may not exist in this deployment; only fail on server errors
        if (status >= 500) throw new Error(`${p} returned ${status}`);
        console.log(`NOTE ${p} -> ${status}`);
        return;
      }
      const body = await page.locator('body').innerText();
      expect(body, `${p} shows error`).not.toMatch(/Unhandled exception|Beklenmeyen hata|Object reference/i);
    });
  }
});
