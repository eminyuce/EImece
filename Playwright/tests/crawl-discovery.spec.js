const { test, expect } = require('@playwright/test');
const {
  normalizeInternalUrl,
  gotoAndAssertOk,
  captureFailure,
  filterConsoleNoise,
  writeJsonReport,
  BASE_ORIGIN,
} = require('./helpers');

/**
 * Crawl internal links from home + sitemap, then visit every unique route.
 * Combined with static inventory so unlinked routes are still covered elsewhere.
 */

const SEED_PATHS = [
  '/',
  '/sitemap.xml',
  '/stories/',
  '/products/',
  '/payment/shoppingcart/',
  '/account/login/',
  '/account/register/',
  '/account/adminlogin/',
  '/info/aboutus/',
  '/info/privacypolicy/',
  '/info/termsandconditions/',
  '/info/deliveryinfo/',
  '/p/arama/?search=kulaklik',
  '/p/advancedsearchproducts/',
  '/health',
  '/robots.txt',
  '/rss/products/',
];

const SKIP_PATH_RE =
  /logoff|logout|delete|externallogin|addtocart|placeorder|paymentresult|buynowpayment|removecart|updatequantity|applycoupon|sendcontactus|addsubscriber|review\//i;

test.describe.configure({ mode: 'serial' });

test('crawl discovery — visit all reachable internal pages', async ({ page, request }) => {
  test.setTimeout(900_000);

  const discovered = new Set();
  const queue = [];
  const report = {
    generatedAt: new Date().toISOString(),
    baseURL: BASE_ORIGIN,
    visited: [],
    failed: [],
    skipped: [],
    consoleErrors: [],
    network500: [],
  };

  function enqueue(raw) {
    const n = normalizeInternalUrl(raw);
    if (!n) return;
    if (SKIP_PATH_RE.test(n)) {
      report.skipped.push({ path: n, reason: 'destructive-or-post' });
      return;
    }
    // Cap query variants for search
    if (n.includes('search=') && ![...discovered].some((d) => d.startsWith('/p/arama'))) {
      // keep
    } else if (n.includes('?') && !n.includes('/p/arama') && !n.includes('lang=')) {
      // drop most query-string variants to avoid explosion
      const bare = n.split('?')[0];
      if (discovered.has(bare) || queue.includes(bare)) return;
    }
    if (!discovered.has(n) && !queue.includes(n)) {
      queue.push(n);
    }
  }

  for (const s of SEED_PATHS) enqueue(s);

  // Sitemap URLs
  const sm = await request.get('/sitemap.xml');
  expect(sm.ok()).toBeTruthy();
  const smText = await sm.text();
  for (const m of smText.matchAll(/<loc>([^<]+)<\/loc>/g)) {
    enqueue(m[1]);
  }

  // Harvest category/product links from a few listing pages first
  for (const seed of ['/', '/products/', '/stories/', '/c/pc/elektronik-0j5i6g1b/']) {
    enqueue(seed);
  }

  let safety = 0;
  const MAX = 180;

  while (queue.length && safety < MAX) {
    const urlPath = queue.shift();
    if (discovered.has(urlPath)) continue;
    discovered.add(urlPath);
    safety += 1;

    // Non-HTML endpoints
    if (/sitemap\.xml|robots\.txt|\/health|\/rss\//i.test(urlPath)) {
      const res = await request.get(urlPath);
      const entry = { path: urlPath, status: res.status(), kind: 'raw' };
      if (res.status() >= 500) {
        report.failed.push({ ...entry, error: `HTTP ${res.status()}` });
      } else {
        report.visited.push(entry);
      }
      continue;
    }

    let lastError = null;
    let result = null;
    for (let attempt = 1; attempt <= 2; attempt++) {
      try {
        result = await gotoAndAssertOk(page, urlPath, { expectCrizal: false });
        const body = await page.locator('body').innerText();
        if (result.status >= 500 || /Unhandled exception/i.test(body)) {
          lastError = `server failure status=${result.status}`;
          result = null;
          await page.waitForTimeout(500);
          continue;
        }
        lastError = null;
        break;
      } catch (err) {
        lastError = String(err);
        result = null;
        await page.waitForTimeout(500);
      }
    }

    if (!result) {
      const shot = await captureFailure(page, `crawl-fail-${urlPath}`);
      report.failed.push({ path: urlPath, error: lastError, screenshot: shot });
      continue;
    }

    const entry = {
      path: urlPath,
      status: result.status,
      finalUrl: result.finalUrl,
      consoleErrors: result.consoleErrors,
      criticalNet: result.criticalNet,
    };
    report.visited.push(entry);
    if (result.consoleErrors?.length) {
      report.consoleErrors = report.consoleErrors || [];
      report.consoleErrors.push({ path: urlPath, errors: result.consoleErrors });
    }

    const hrefs = await page.$$eval('a[href]', (as) => as.map((a) => a.getAttribute('href')));
    for (const h of hrefs) enqueue(h);
  }

  report.totals = {
    discovered: discovered.size,
    visited: report.visited.length,
    failed: report.failed.length,
    skipped: report.skipped.length,
    queuedRemaining: queue.length,
  };

  const reportPath = writeJsonReport('crawl-report.json', report);
  console.log(`Crawl report: ${reportPath}`);
  console.log(`Visited=${report.visited.length} Failed=${report.failed.length} Discovered=${discovered.size}`);

  expect(report.failed, `Crawl failures:\n${JSON.stringify(report.failed, null, 2)}`).toEqual([]);
});
