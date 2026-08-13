/**
 * Post-deploy smoke: sitemap, storefront, customer, admin @ http://localhost:81
 */
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const BASE = 'http://localhost:81';
const CUSTOMER = { email: 'eminyuce1111@gmail.com', password: 'V02y.qcF' };
const REPORT = path.join(__dirname, 'smoke-sonar5-report.json');
const SHOT = path.join(__dirname, 'screenshots', 'smoke-sonar5');
fs.mkdirSync(SHOT, { recursive: true });

const results = [];

function classify(status, body) {
  const text = (body || '').slice(0, 2500);
  const unhandled = /Unhandled exception|Server Error in Application|yellow-screen|Stack Trace:/i.test(text);
  const friendly404 = /sayfa bulunamadı|page not found/i.test(text);
  let kind = 'ok';
  if (status >= 500 || unhandled) kind = '5xx';
  else if (status === 404) kind = '404';
  else if (status >= 400) kind = '4xx';
  return { kind, unhandled, friendly404, snippet: text.replace(/\s+/g, ' ').slice(0, 220) };
}

async function probe(request, url, area) {
  const started = Date.now();
  try {
    const res = await request.get(url, { timeout: 45_000, maxRedirects: 5 });
    const body = await res.text().catch(() => '');
    const c = classify(res.status(), body);
    const row = { area, url, status: res.status(), ms: Date.now() - started, ...c };
    results.push(row);
    const mark = c.kind === 'ok' ? 'OK ' : c.kind.toUpperCase();
    console.log(`${mark.padEnd(4)} ${res.status()} ${row.ms}ms ${url}`);
    return row;
  } catch (e) {
    const row = { area, url, status: 0, ms: Date.now() - started, kind: 'error', unhandled: true, snippet: String(e.message || e).slice(0, 220) };
    results.push(row);
    console.log(`ERR  ${url} ${row.snippet}`);
    return row;
  }
}

async function shot(page, name) {
  await page.screenshot({ path: path.join(SHOT, `${name}.png`), fullPage: false }).catch(() => {});
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const page = await context.newPage();
  const request = context.request;

  // --- sitemap ---
  const sm = await probe(request, `${BASE}/sitemap.xml`, 'sitemap');
  let sitemapLocs = [];
  if (sm.status === 200) {
    const xml = await request.get(`${BASE}/sitemap.xml`).then((r) => r.text());
    sitemapLocs = [...xml.matchAll(/<loc>([^<]+)<\/loc>/g)].map((m) => m[1].trim());
    console.log(`sitemap urls: ${sitemapLocs.length}`);
  }

  const frontStatic = [
    '/',
    '/health',
    '/robots.txt',
    '/p',
    '/c',
    '/s',
    '/o/shoppingcart',
    '/account/login',
    '/account/adminlogin',
    '/account/register',
  ];
  for (const p of frontStatic) {
    await probe(request, BASE + p, 'frontstore');
  }

  // Sample sitemap URLs (all if <= 80, else first 80 unique paths)
  const uniq = [...new Set(sitemapLocs)];
  const sample = uniq.slice(0, 80);
  for (const loc of sample) {
    await probe(request, loc, 'sitemap-url');
  }

  // --- customer login ---
  await page.goto(`${BASE}/account/login`, { waitUntil: 'domcontentloaded', timeout: 45_000 });
  await shot(page, '01-login');
  const email = page.locator('input[type="email"], input[name="Email"], #Email').first();
  const pass = page.locator('input[type="password"], input[name="Password"], #Password').first();
  if (await email.count()) {
    await email.fill(CUSTOMER.email);
    await pass.fill(CUSTOMER.password);
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 45_000 }).catch(() => {}),
      page.locator('button[type="submit"], input[type="submit"]').first().click(),
    ]);
  }
  await shot(page, '02-after-login');
  results.push({
    area: 'customer-login',
    url: page.url(),
    status: 200,
    kind: /login/i.test(page.url()) && !(await page.content()).includes('LogOff') ? 'warn' : 'ok',
    snippet: `title=${await page.title()} url=${page.url()}`,
  });

  const customerPages = [
    '/Customers/Home/Index',
    '/Customers/Home/CustomerOrders',
    '/Customers/Home/Faq',
    '/Customers/Home/ChangePassword',
    '/Customers/Home/SendMessageToSeller',
    '/Customers/Home/WebSiteAddressInfo',
  ];
  for (const p of customerPages) {
    await probe(request, BASE + p, 'customer');
    await page.goto(BASE + p, { waitUntil: 'domcontentloaded', timeout: 45_000 }).catch(() => {});
    await shot(page, `cust-${p.replace(/\W+/g, '_')}`);
  }

  // --- admin (BypassAdminAuth) ---
  await page.goto(`${BASE}/admin`, { waitUntil: 'domcontentloaded', timeout: 60_000 });
  await shot(page, '03-admin-dashboard');
  results.push({
    area: 'admin-dashboard',
    url: page.url(),
    status: 200,
    kind: /adminlogin/i.test(page.url()) ? 'warn' : 'ok',
    snippet: `title=${await page.title()} url=${page.url()}`,
  });

  const adminControllers = [
    'Dashboard', 'Products', 'ProductCategories', 'Brands', 'Menus', 'Stories',
    'StoryCategories', 'Tags', 'TagCategories', 'Users', 'Customers', 'Orders',
    'ShoppingCarts', 'Coupons', 'Subscribers', 'MailTemplates', 'Settings',
    'AdminSettings', 'Templates', 'Lists', 'Faq', 'MainPageImages',
    'ProductComments', 'AppLogs', 'Media', 'Report', 'Metrics', 'ImportData',
  ];
  for (const c of adminControllers) {
    await probe(request, `${BASE}/Admin/${c}`, 'admin');
  }

  await browser.close();

  const summary = {
    total: results.length,
    ok: results.filter((r) => r.kind === 'ok').length,
    fail5xx: results.filter((r) => r.kind === '5xx'),
    fail404: results.filter((r) => r.kind === '404' && !r.friendly404),
    fail4xx: results.filter((r) => r.kind === '4xx'),
    errors: results.filter((r) => r.kind === 'error'),
    warns: results.filter((r) => r.kind === 'warn'),
  };
  fs.writeFileSync(REPORT, JSON.stringify({ summary: { ...summary, fail5xx: summary.fail5xx.length, fail404: summary.fail404.length, fail4xx: summary.fail4xx.length, errors: summary.errors.length, warns: summary.warns.length }, failures: [...summary.fail5xx, ...summary.errors, ...summary.fail4xx, ...summary.warns], results }, null, 2));
  console.log('\n=== SUMMARY ===');
  console.log(`total=${summary.total} ok=${summary.ok} 5xx=${summary.fail5xx.length} 404=${summary.fail404.length} 4xx=${summary.fail4xx.length} err=${summary.errors.length} warn=${summary.warns.length}`);
  for (const f of [...summary.fail5xx, ...summary.errors, ...summary.fail4xx, ...summary.warns]) {
    console.log(`FAIL ${f.kind} ${f.status} ${f.url} ${f.snippet || ''}`);
  }
  process.exit(summary.fail5xx.length + summary.errors.length > 0 ? 1 : 0);
})().catch((e) => {
  console.error(e);
  process.exit(2);
});
