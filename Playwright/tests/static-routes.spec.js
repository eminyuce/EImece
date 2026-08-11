const { test, expect } = require('@playwright/test');
const { gotoAndAssertOk, captureFailure, writeJsonReport } = require('./helpers');

/**
 * Static inventory of MVC routes that may not all appear in the crawl.
 */

const PUBLIC_GET_ROUTES = [
  { name: 'home', path: '/' },
  { name: 'health', path: '/health', raw: true },
  { name: 'healthz', path: '/healthz', raw: true },
  { name: 'robots', path: '/robots.txt', raw: true },
  { name: 'sitemap', path: '/sitemap.xml', raw: true },
  { name: 'products-index', path: '/products/' },
  { name: 'stories-index', path: '/stories/' },
  { name: 'search', path: '/p/arama/?search=test' },
  { name: 'advanced-search', path: '/p/advancedsearchproducts/' },
  { name: 'cart', path: '/payment/shoppingcart/' },
  { name: 'cargo-tracking', path: '/payment/cargotracking/' },
  { name: 'guest-checkout', path: '/payment/shoppingwithoutaccount/' },
  { name: 'checkout-billing', path: '/payment/checkoutbillingdetails/' },
  { name: 'login', path: '/account/login/' },
  { name: 'admin-login', path: '/account/adminlogin/' },
  { name: 'register', path: '/account/register/' },
  { name: 'forgot-password', path: '/account/forgotpassword/' },
  { name: 'about', path: '/info/aboutus/' },
  { name: 'privacy', path: '/info/privacypolicy/' },
  { name: 'terms', path: '/info/termsandconditions/' },
  { name: 'delivery', path: '/info/deliveryinfo/' },
  { name: 'contact-page', path: '/i/iletisim-3f4h8c6g/' },
  { name: 'faq-page', path: '/i/sikca-sorulan-sorular-4h4h8c6g/' },
  { name: 'category-electronics', path: '/c/pc/elektronik-0j5i6g1b/' },
  { name: 'category-headphones', path: '/c/pc/kulaklik--ses-4h0j6g1b/' },
  { name: 'story-category', path: '/s/sc/stil-rehberi-8c6g/' },
  { name: 'rss-products', path: '/rss/products/', raw: true },
  { name: 'rss-stories', path: '/rss/storycategories/', raw: true },
  { name: 'ajax-cities', path: '/ajax/getallcities/', raw: true },
  { name: 'cart-small', path: '/payment/getshoppingcartsmalldetails/', raw: true },
  { name: 'cart-links', path: '/payment/getshoppingcartlinks/', raw: true },
  { name: 'languages', path: '/home/languages/' },
  { name: 'company-name', path: '/home/getcompanyname/', raw: true },
  { name: 'social-links', path: '/home/socialmedialinks/' },
  { name: 'error-index', path: '/error/index/' },
  { name: 'logo', path: '/images/logo.jpg', raw: true },
  { name: 'captcha', path: '/images/getcaptcha?prefix=CustomerLogin', raw: true },
];

const AUTH_REDIRECT_ROUTES = [
  { name: 'admin-root', path: '/admin/', expectLogin: true },
  { name: 'customers-root', path: '/customers/', expectLogin: true },
  { name: 'manage-root', path: '/manage/', expectLogin: true },
];

test.describe('Static route inventory', () => {
  test('public GET routes load without 5xx / unhandled exceptions', async ({ page, request }) => {
    test.setTimeout(600_000);
    const results = [];

    for (const route of PUBLIC_GET_ROUTES) {
      if (route.raw) {
        const res = await request.get(route.path);
        const status = res.status();
        const ok = status > 0 && status < 500;
        results.push({ ...route, status, ok, error: ok ? null : `HTTP ${status}` });
        expect.soft(ok, `${route.name} ${route.path} => ${status}`).toBeTruthy();
        continue;
      }

      try {
        const r = await gotoAndAssertOk(page, route.path, { expectCrizal: false });
        const ok = r.status < 500 && r.criticalNet.length === 0;
        results.push({
          ...route,
          status: r.status,
          finalUrl: r.finalUrl,
          ok,
          consoleErrors: r.consoleErrors,
          criticalNet: r.criticalNet,
        });
        expect.soft(ok, `${route.name} failed`).toBeTruthy();
        // Console 404 noise is filtered; assert only remaining JS page errors.
        if (r.consoleErrors.length) {
          expect.soft(r.consoleErrors, `${route.name} console`).toEqual([]);
        }
        if (r.assetFailures.some((a) => a.status >= 500)) {
          expect.soft(r.assetFailures.filter((a) => a.status >= 500), `${route.name} asset 5xx`).toEqual([]);
        }
      } catch (e) {
        await captureFailure(page, `static-${route.name}`);
        results.push({ ...route, ok: false, error: String(e) });
        expect.soft(false, `${route.name}: ${e}`).toBeTruthy();
      }
    }

    writeJsonReport('static-routes-report.json', { generatedAt: new Date().toISOString(), results });
  });

  test('protected routes redirect anonymous users to login', async ({ page }) => {
    for (const route of AUTH_REDIRECT_ROUTES) {
      const res = await page.goto(route.path, { waitUntil: 'domcontentloaded' });
      // Followed redirect → login
      expect(page.url(), route.name).toMatch(/\/account\/login/i);
      expect(res?.status() ?? 200).toBeLessThan(500);
    }
  });

  test('product detail pages from category listing (async regression)', async ({ page }) => {
    test.setTimeout(180_000);
    await page.goto('/c/pc/elektronik-0j5i6g1b/', { waitUntil: 'domcontentloaded' });
    const productHrefs = await page.$$eval('a[href*="/p/"]', (as) =>
      [...new Set(as.map((a) => a.getAttribute('href')).filter(Boolean))]
    );
    expect(productHrefs.length).toBeGreaterThan(0);

    const sample = productHrefs.slice(0, 8);
    const failures = [];
    for (const href of sample) {
      try {
        const r = await gotoAndAssertOk(page, href, { expectCrizal: true });
        expect(r.status).toBeLessThan(500);
        await expect(page.locator('#main-content')).toBeVisible();
        const body = await page.locator('body').innerText();
        expect(body).not.toMatch(/Unhandled exception/i);
      } catch (e) {
        await captureFailure(page, `product-detail-${href}`);
        failures.push({ href, error: String(e) });
      }
    }
    expect(failures, JSON.stringify(failures, null, 2)).toEqual([]);
  });
});
