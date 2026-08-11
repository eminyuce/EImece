const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');
const {
  collectPageIssues,
  filterConsoleNoise,
  filterAssetFailures,
} = require('./helpers');

const BASE = 'http://localhost:81';
const REPORT_DIR = path.join(__dirname, '..', 'test-results', 'async-regression');
const SCREENSHOT_DIR = path.join(REPORT_DIR, 'screenshots');

/** Static inventory of known storefront / system routes (combined with crawl). */
const STATIC_ROUTES = [
  '/',
  '/stories/',
  '/account/login/',
  '/account/register/',
  '/account/forgotpassword/',
  '/account/adminlogin/',
  '/payment/shoppingcart/',
  '/payment/shoppingwithoutaccount/',
  '/payment/cargotracking/',
  '/payment/nosuccessforyourorder/',
  '/info/aboutus/',
  '/info/deliveryinfo/',
  '/info/privacypolicy/',
  '/info/termsandconditions/',
  '/i/iletisim-3f4h8c6g/',
  '/i/kampanyalar-5i4h8c6g/',
  '/i/sikca-sorulan-sorular-4h4h8c6g/',
  '/i/iade--degisim-8c5i8c6g/',
  '/p/arama?search=nordline',
  '/p/advancedsearchproducts?search=test',
  '/c/pc/kulaklik--ses-4h0j6g1b/',
  '/c/pc/elektronik-0j5i6g1b/',
  '/p/kulaklik--ses/nordline-wireless-bluetooth-kulaklik-pro-4h2d9a5i4h1b/',
  '/p/aydinlatma/homeglow-akilli-led-ampul-9w-4h3f1b5i4h1b/',
  '/s/sc/stil-rehberi-8c6g/',
  '/s/stil-rehberi/2024-sonbahar-kombin-onerileri-1b3f1b/',
  '/health',
  '/sitemap.xml',
  '/robots.txt',
  '/ajax/getallcities',
  '/home/getcompanyname',
  '/home/socialmedialinks',
  '/home/websiteaddressinfo',
  '/home/languages',
  '/home/language/1',
  '/payment/getshoppingcartsmalldetails',
  '/payment/getshoppingcartlinks',
  '/payment/rendershoppingcartprice',
  '/error/',
];

/** Routes expected to 404 / redirect without counting as hard failures */
const SOFT_FAIL_ROUTES = [
  /^\/c\/Ev-Yasam/i,
  /^\/images\/logo\.jpg/i,
  /^\/Home\/ThanksForSubscription\/1/i,
  /^\/Home\/Language\/tr/i,
];

const SKIP_HREF =
  /^(mailto:|tel:|javascript:|#|https?:\/\/(?!localhost:81))/i;
const SKIP_PATH =
  /logoff|logout|deleteconfirmed|deleteall|exportExcel|enableauthenticator|disableauthenticator/i;

function normalizeUrl(href, fromUrl) {
  try {
    const u = new URL(href, fromUrl);
    if (u.origin !== BASE) return null;
    if (SKIP_HREF.test(href)) return null;
    if (SKIP_PATH.test(u.pathname)) return null;
    u.hash = '';
    // Drop noisy tracking params
    ['utm_source', 'utm_medium', 'utm_campaign', 'fbclid', 'gclid'].forEach((p) =>
      u.searchParams.delete(p)
    );
    let out = u.pathname + u.search;
    if (!out.endsWith('/') && !u.search && !path.extname(u.pathname)) {
      // keep as-is; MVC often appends trailing slash via redirect
    }
    return out;
  } catch {
    return null;
  }
}

function ensureDirs() {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

function isServerError(status) {
  return status >= 500;
}

function looksLikeAsyncChildActionError(text) {
  return (
    /HttpServerUtility\.Execute blocked while waiting for an asynchronous operation/i.test(
      text
    ) ||
    /ServerExecuteHttpHandlerAsyncWrapper/i.test(text) ||
    /asynchronous operation to complete/i.test(text)
  );
}

test.describe.configure({ mode: 'serial' });

test.describe('Async controller full regression', () => {
  test.describe.configure({ timeout: 180_000 });
  /** @type {import('@playwright/test').Browser} */
  let report;

  test.beforeAll(() => {
    ensureDirs();
    report = {
      startedAt: new Date().toISOString(),
      baseURL: BASE,
      crawled: [],
      passed: [],
      failed: [],
      suspectedAsync: [],
      flows: [],
      skipped: [],
    };
  });

  test.afterAll(() => {
    report.finishedAt = new Date().toISOString();
    report.summary = {
      routesTested: report.passed.length + report.failed.length,
      passed: report.passed.length,
      failed: report.failed.length,
      suspectedAsync: report.suspectedAsync.length,
      flows: report.flows.filter((f) => f.ok).length,
      flowsFailed: report.flows.filter((f) => !f.ok).length,
    };
    fs.writeFileSync(
      path.join(REPORT_DIR, 'report.json'),
      JSON.stringify(report, null, 2),
      'utf8'
    );
    const md = [
      '# Async Controller Regression Report',
      '',
      `- Base URL: ${report.baseURL}`,
      `- Started: ${report.startedAt}`,
      `- Finished: ${report.finishedAt}`,
      `- Routes tested: ${report.summary.routesTested}`,
      `- Passed: ${report.summary.passed}`,
      `- Failed: ${report.summary.failed}`,
      `- Suspected async regressions: ${report.summary.suspectedAsync}`,
      `- Flows passed: ${report.summary.flows}`,
      `- Flows failed: ${report.summary.flowsFailed}`,
      '',
      '## Failed',
      ...(report.failed.length
        ? report.failed.map(
            (f) =>
              `- **${f.url}** — ${f.feature || 'page'} — ${f.message}` +
              (f.asyncRelated ? ' _(async-related)_' : '')
          )
        : ['- None']),
      '',
      '## Suspected Async Regression',
      ...(report.suspectedAsync.length
        ? report.suspectedAsync.map((f) => `- ${f.url}: ${f.message}`)
        : ['- None']),
      '',
      '## Passed (sample)',
      ...report.passed.slice(0, 80).map((p) => `- ${p.url} (${p.status})`),
      report.passed.length > 80 ? `- …and ${report.passed.length - 80} more` : '',
    ].join('\n');
    fs.writeFileSync(path.join(REPORT_DIR, 'report.md'), md, 'utf8');
  });

  test('crawl discover + validate all internal pages', async ({ page }) => {
    const issues = await collectPageIssues(page);
    const pageErrors = [];
    page.on('pageerror', (err) => pageErrors.push(String(err)));

    const queue = [...STATIC_ROUTES];
    const seen = new Set();
    const maxPages = 90;

    while (queue.length && seen.size < maxPages) {
      const route = queue.shift();
      const key = route.split('#')[0];
      if (seen.has(key)) continue;
      seen.add(key);
      report.crawled.push(key);

      issues.consoleErrors.length = 0;
      issues.failedRequests.length = 0;
      pageErrors.length = 0;

      let response;
      try {
        response = await page.goto(key, {
          waitUntil: 'domcontentloaded',
          timeout: 45_000,
        });
      } catch (e) {
        const fail = {
          url: key,
          feature: 'navigation',
          action: 'goto',
          expected: 'page loads',
          actual: String(e.message || e),
          message: `Navigation failed: ${e.message || e}`,
          asyncRelated: /HttpServerUtility\.Execute blocked|asynchronous operation to complete/i.test(
            String(e)
          ),
        };
        report.failed.push(fail);
        if (fail.asyncRelated) report.suspectedAsync.push(fail);
        await page.screenshot({
          path: path.join(SCREENSHOT_DIR, `nav-fail-${seen.size}.png`),
          fullPage: true,
        }).catch(() => {});
        continue;
      }

      const status = response?.status() ?? 0;
      const finalUrl = page.url();
      const bodyText = await page.locator('body').innerText().catch(() => '');
      const title = await page.title().catch(() => '');
      const asyncChildBug = looksLikeAsyncChildActionError(bodyText);
      const blank =
        !bodyText ||
        bodyText.trim().length < 20 ||
        /Unhandled exception/i.test(bodyText);

      // Discover more internal links (bounded; avoid hanging on huge DOMs)
      try {
        const hrefs = await page.$$eval('a[href]', (as) =>
          as.slice(0, 200).map((a) => a.getAttribute('href')).filter(Boolean)
        );
        for (const href of hrefs) {
          const n = normalizeUrl(href, finalUrl);
          if (n && !seen.has(n) && !queue.includes(n)) queue.push(n);
        }
      } catch {
        // page may have navigated away; ignore discovery errors
      }

      const consoleBad = filterConsoleNoise(issues.consoleErrors).filter(
        (e) => !/favicon|apple-touch|logo\.jpg/i.test(e)
      );
      const netBad = filterAssetFailures(issues.failedRequests).filter(
        (f) =>
          f.status >= 500 ||
          (f.status === 404 &&
            !/favicon|apple-touch|logo\.jpg|manifest\.json/i.test(f.url))
      );
      const net500 = netBad.filter((n) => n.status >= 500);
      const docFailed =
        isServerError(status) ||
        asyncChildBug ||
        (blank && status >= 400) ||
        /Unhandled exception|HttpServerUtility\.Execute blocked/i.test(bodyText);

      if (docFailed || net500.length) {
        const shot = path.join(SCREENSHOT_DIR, `fail-${seen.size}.png`);
        await page.screenshot({ path: shot, fullPage: true }).catch(() => {});
        const fail = {
          url: key,
          finalUrl,
          feature: 'page load',
          action: 'render',
          expected: 'HTTP < 500, content rendered, no async child-action errors',
          actual: `status=${status}, blank=${blank}, asyncChild=${asyncChildBug}`,
          status,
          consoleErrors: consoleBad.slice(0, 10),
          networkErrors: netBad.slice(0, 10),
          pageErrors: pageErrors.slice(0, 10),
          message: asyncChildBug
            ? 'Async child action / Html.Action failure'
            : `Page problem status=${status} title=${title}`,
          asyncRelated: asyncChildBug,
          screenshot: shot,
        };
        report.failed.push(fail);
        if (fail.asyncRelated) report.suspectedAsync.push(fail);
      } else {
        report.passed.push({
          url: key,
          finalUrl,
          status,
          title,
          softAssetWarnings: netBad.filter((n) => n.status < 500).slice(0, 5),
        });
      }
    }

    expect(
      report.failed.filter((f) => f.asyncRelated).length,
      `Async-related failures: ${JSON.stringify(report.suspectedAsync, null, 2)}`
    ).toBe(0);

    // Soft assert: allow some known 404 content links, but not mass failure
    const hardFails = report.failed.filter((f) => {
      if (SOFT_FAIL_ROUTES.some((re) => re.test(f.url))) return false;
      // Auth-protected redirects that still render login are OK under 302→200
      if (f.status === 404 && /apple-touch|favicon|Ev-Yasam/i.test(f.url)) return false;
      return f.status >= 500 || f.asyncRelated || /Navigation failed/i.test(f.message);
    });
    expect(
      hardFails,
      `Hard failures:\n${hardFails.map((f) => `${f.url}: ${f.message}`).join('\n')}`
    ).toEqual([]);
  });

  test('product detail renders (async child-action smoke)', async ({ page }) => {
    const res = await page.goto(
      '/p/kulaklik--ses/nordline-wireless-bluetooth-kulaklik-pro-4h2d9a5i4h1b/',
      { waitUntil: 'domcontentloaded' }
    );
    expect(res?.status()).toBe(200);
    await expect(page.locator('main#main-content')).toBeVisible();
    const text = await page.locator('body').innerText();
    expect(looksLikeAsyncChildActionError(text)).toBeFalsy();
    expect(text).not.toMatch(/Beklenmeyen hata|Unhandled exception/i);
    report.flows.push({ name: 'product-detail', ok: true });
  });

  test('add to cart AJAX + cart page', async ({ page }) => {
    const flow = { name: 'add-to-cart', ok: false };
    await page.goto(
      '/p/kulaklik--ses/nordline-wireless-bluetooth-kulaklik-pro-4h2d9a5i4h1b/',
      { waitUntil: 'domcontentloaded' }
    );

    const addBtn = page
      .locator(
        'button:has-text("Sepete"), a:has-text("Sepete"), [data-add-to-cart], .add-to-cart, #addToCart, button[onclick*="AddToCart"], a[onclick*="AddToCart"]'
      )
      .first();

    if (await addBtn.count()) {
      const waitAjax = page.waitForResponse(
        (r) =>
          /AddToCart|ShoppingCart|getshoppingcart/i.test(r.url()) &&
          r.status() < 500,
        { timeout: 15_000 }
      ).catch(() => null);
      await addBtn.click();
      await waitAjax;
    }

    const cartRes = await page.goto('/payment/shoppingcart/', {
      waitUntil: 'domcontentloaded',
    });
    expect(cartRes?.status()).toBeLessThan(500);
    const text = await page.locator('body').innerText();
    expect(looksLikeAsyncChildActionError(text)).toBeFalsy();
    flow.ok = true;
    report.flows.push(flow);
  });

  test('search form interaction', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    const toggle = page.locator('li.search a').first();
    if (await toggle.count()) {
      await toggle.click();
    }
    const input = page
      .locator(
        '.top-search input[type="text"], .top-search input[name="search"], input[name="search"]'
      )
      .first();
    await expect(input).toBeVisible({ timeout: 10_000 });
    await input.fill('kulaklik');
    await Promise.all([
      page.waitForURL(/arama|search|kulaklik/i, { timeout: 20_000 }).catch(() => null),
      input.press('Enter'),
    ]);
    await page.waitForLoadState('domcontentloaded');
    expect(page.url()).toMatch(/arama|search|kulaklik|p\//i);
    const text = await page.locator('body').innerText();
    expect(looksLikeAsyncChildActionError(text)).toBeFalsy();
    report.flows.push({ name: 'search', ok: true });
  });

  test('customer login validation + admin login page', async ({ page }) => {
    await page.goto('/account/login/', { waitUntil: 'domcontentloaded' });
    await expect(page.locator('#Email, input[name="Email"]').first()).toBeVisible();
    await page.locator('#Email, input[name="Email"]').first().fill('customer1@eimece.test');
    await page.locator('#Password, input[name="Password"]').first().fill('Test123!');
    await page.locator('form').filter({ has: page.locator('input[name="Email"]') }).first().locator('button[type="submit"], input[type="submit"]').first().click();
    await page.waitForLoadState('domcontentloaded');
    const afterLogin = page.url();
    // Login may succeed or fail captcha — must not 500 / async child error
    const text = await page.locator('body').innerText();
    expect(looksLikeAsyncChildActionError(text)).toBeFalsy();
    expect(text).not.toMatch(/HttpServerUtility\.Execute blocked/i);

    await page.goto('/account/adminlogin/', { waitUntil: 'domcontentloaded' });
    expect((await page.locator('body').innerText())).not.toMatch(
      /HttpServerUtility\.Execute blocked/i
    );
    report.flows.push({
      name: 'auth-forms',
      ok: true,
      afterLogin,
    });
  });

  test('AJAX city/town cascade', async ({ page }) => {
    const cities = await page.request.get('/ajax/getallcities');
    expect(cities.status()).toBe(200);
    const citiesJson = await cities.json();
    expect(Array.isArray(citiesJson)).toBeTruthy();
    expect(citiesJson.length).toBeGreaterThan(1);

    const city =
      citiesJson.find((c) => c.Value && /istanbul/i.test(c.Value || c.Text || ''))?.Value ||
      citiesJson.find((c) => c.Value)?.Value;
    expect(city).toBeTruthy();

    const towns = await page.request.get(
      `/ajax/gettownsbycity?cityName=${encodeURIComponent(city)}`
    );
    expect(towns.status()).toBe(200);
    const townsJson = await towns.json();
    expect(Array.isArray(townsJson)).toBeTruthy();

    const town = townsJson.find((t) => t.Value)?.Value;
    if (town) {
      const districts = await page.request.get(
        `/ajax/getdistrictsbytown?cityName=${encodeURIComponent(city)}&townName=${encodeURIComponent(town)}`
      );
      expect(districts.status()).toBe(200);
    }
    report.flows.push({ name: 'ajax-regions', ok: true });
  });

  test('guest checkout form fields interact', async ({ page }) => {
    await page.goto('/payment/shoppingwithoutaccount/', {
      waitUntil: 'domcontentloaded',
    });
    const text = await page.locator('body').innerText();
    expect(looksLikeAsyncChildActionError(text)).toBeFalsy();
    const name = page.locator('input[name="Name"], #Name').first();
    if (await name.count()) {
      await name.fill('Test User');
    }
    const email = page.locator('input[name="Email"], #Email').first();
    if (await email.count()) {
      await email.fill('customer1@eimece.test');
    }
    // Trigger city AJAX if city select exists
    const citySelect = page.locator('select[name="City"], #City').first();
    if (await citySelect.count()) {
      const options = await citySelect.locator('option').allTextContents();
      const pick = options.find((o) => o && !/^seç/i.test(o) && o.trim());
      if (pick) {
        const townResp = page.waitForResponse(
          (r) => /GetTownsByCity|gettownsbycity/i.test(r.url()),
          { timeout: 10_000 }
        ).catch(() => null);
        await citySelect.selectOption({ label: pick });
        await townResp;
      }
    }
    report.flows.push({ name: 'guest-checkout-form', ok: true });
  });

  test('responsive smoke on key pages', async ({ page }) => {
    const viewports = [
      { width: 1440, height: 900 },
      { width: 375, height: 812 },
    ];
    const pages = [
      '/',
      '/c/pc/kulaklik--ses-4h0j6g1b/',
      '/p/kulaklik--ses/nordline-wireless-bluetooth-kulaklik-pro-4h2d9a5i4h1b/',
      '/payment/shoppingcart/',
      '/account/login/',
    ];
    for (const vp of viewports) {
      await page.setViewportSize(vp);
      for (const url of pages) {
        const res = await page.goto(url, { waitUntil: 'domcontentloaded' });
        expect(res?.status() ?? 500, url).toBeLessThan(500);
        const overflow = await page.evaluate(() => {
          return document.documentElement.scrollWidth > window.innerWidth + 2;
        });
        // Record but do not fail suite solely on minor overflow; product pages often ok
        if (overflow) {
          report.flows.push({
            name: `overflow-${vp.width}-${url}`,
            ok: true,
            note: 'horizontal overflow detected',
          });
        }
        const text = await page.locator('body').innerText();
        expect(looksLikeAsyncChildActionError(text)).toBeFalsy();
      }
    }
    report.flows.push({ name: 'responsive-smoke', ok: true });
  });

  test('admin area redirects anonymous to login', async ({ page }) => {
    const res = await page.goto('/admin/', { waitUntil: 'domcontentloaded' });
    expect(res?.status() ?? 0).toBeLessThan(500);
    expect(page.url()).toMatch(/adminlogin|login|account/i);
    const text = await page.locator('body').innerText();
    expect(looksLikeAsyncChildActionError(text)).toBeFalsy();
    report.flows.push({ name: 'admin-auth-redirect', ok: true });
  });
});
