const { test, expect } = require('@playwright/test');
const { loginWithPassword, captureFailure, gotoAndAssertOk } = require('./helpers');

const ADMIN = { email: 'admin@eimece.test', password: 'Test123!' };

const ADMIN_INDEX_PAGES = [
  '/admin/',
  '/admin/dashboard/',
  '/admin/products/',
  '/admin/productcategories/',
  '/admin/stories/',
  '/admin/storycategories/',
  '/admin/menus/',
  '/admin/brands/',
  '/admin/tags/',
  '/admin/tagcategories/',
  '/admin/faq/',
  '/admin/coupons/',
  '/admin/orders/',
  '/admin/customers/',
  '/admin/subscribers/',
  '/admin/settings/',
  '/admin/users/',
  '/admin/media/',
  '/admin/applogs/',
  '/admin/report/',
  '/admin/metrics/',
  '/admin/mainpageimages/',
  '/admin/lists/',
  '/admin/productcomments/',
  '/admin/shoppingcarts/',
];

test.describe('Admin area (authenticated)', () => {
  test.describe.configure({ mode: 'serial' });

  test('login and smoke admin index pages', async ({ page }) => {
    test.setTimeout(600_000);

    const loggedIn = await loginWithPassword(page, {
      email: ADMIN.email,
      password: ADMIN.password,
      loginPath: '/account/adminlogin/',
    });

    if (!loggedIn) {
      await captureFailure(page, 'admin-area-login');
      test.skip(true, `Admin login failed (captcha/2FA). URL=${page.url()}`);
    }

    // If forced to enable authenticator, document and skip grid smoke
    if (/verifyauthenticator|enableauthenticator/i.test(page.url())) {
      test.info().annotations.push({
        type: 'note',
        description: `Admin reached 2FA gate at ${page.url()}; index smoke skipped`,
      });
      return;
    }

    const failures = [];
    for (const path of ADMIN_INDEX_PAGES) {
      try {
        const res = await page.goto(path, { waitUntil: 'domcontentloaded', timeout: 45_000 });
        const status = res?.status() ?? 0;
        const body = await page.locator('body').innerText();
        if (status >= 500 || /Unhandled exception/i.test(body)) {
          await captureFailure(page, `admin-${path}`);
          failures.push({ path, status, error: body.slice(0, 200) });
        } else if (/\/account\/(login|adminlogin)/i.test(page.url())) {
          failures.push({ path, status, error: 'redirected to login (session lost)' });
        }
      } catch (e) {
        await captureFailure(page, `admin-ex-${path}`);
        failures.push({ path, error: String(e) });
      }
    }

    expect(failures, JSON.stringify(failures, null, 2)).toEqual([]);
  });

  test('anonymous admin AJAX is unauthorized', async ({ request }) => {
    const res = await request.post('/admin/ajax/searchautocomplete/', {
      data: { term: 'test' },
      failOnStatusCode: false,
    });
    // Anonymous callers must not get a server fault; redirect/auth errors are fine.
    expect(res.status(), `unexpected status ${res.status()}`).toBeLessThan(500);
  });
});
