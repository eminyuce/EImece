/**
 * Post-Sonar deploy QA: sitemap crawl, admin pages, CRUD, customer cart.
 * Writes Playwright/tmp-sonar4-qa-report.json
 */
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const BASE = 'http://localhost:81';
const CUSTOMER_EMAIL = 'eminyuce1111@gmail.com';
const CUSTOMER_PASSWORD = 'V02y.qcF';
const report = {
  startedAt: new Date().toISOString(),
  sitemap: { total: 0, ok: 0, fail: [] },
  admin: { pages: [], crud: null },
  customer: { login: null, cart: null },
  mobile: { pages: [] },
  errors: [],
};

function isAppError(status, body) {
  if (status >= 500) return `HTTP ${status}`;
  if (/Unhandled exception|Server Error in|YelloError|yellow-screen/i.test(body || '')) {
    return 'exception page';
  }
  return null;
}

async function collect(page) {
  const consoleErrors = [];
  const pageErrors = [];
  page.on('console', (m) => {
    if (m.type() === 'error') consoleErrors.push(m.text());
  });
  page.on('pageerror', (e) => pageErrors.push(String(e)));
  return { consoleErrors, pageErrors };
}

async function gotoCheck(page, url, label) {
  const issues = await collect(page);
  let status = 0;
  let err = null;
  try {
    const res = await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 45_000 });
    status = res ? res.status() : 0;
    const body = await page.locator('body').innerText().catch(() => '');
    err = isAppError(status, body);
    if (!err && (!body || body.trim().length < 8) && !url.includes('sitemap')) {
      err = 'empty body';
    }
  } catch (e) {
    err = String(e.message || e).slice(0, 240);
  }
  const noise = /favicon|gtag|google-analytics|whatsapp|Failed to load resource: the server responded with a status of 404/i;
  const js = issues.consoleErrors.filter((t) => !noise.test(t)).slice(0, 5);
  const pe = issues.pageErrors.slice(0, 5);
  return { label, url, status, err, js, pe, title: await page.title().catch(() => '') };
}

async function parseSitemap(page) {
  const res = await page.goto(`${BASE}/sitemap.xml`, { waitUntil: 'domcontentloaded', timeout: 45_000 });
  const status = res ? res.status() : 0;
  const xml = await page.content();
  const locs = [];
  const re = /<loc>\s*([^<]+)\s*<\/loc>/gi;
  let m;
  while ((m = re.exec(xml))) {
    locs.push(m[1].trim());
  }
  return { status, locs, xmlLen: xml.length };
}

(async () => {
  const browser = await chromium.launch({ headless: true, channel: 'chrome' });
  const context = await browser.newContext({ viewport: { width: 1366, height: 900 } });
  const page = await context.newPage();

  // --- sitemap ---
  try {
    const sm = await parseSitemap(page);
    report.sitemap.http = sm.status;
    report.sitemap.total = sm.locs.length;
    const unique = [...new Set(sm.locs)];
    // Cap crawl to keep runtime reasonable; still hit all if modest.
    const toVisit = unique.slice(0, 40);
    report.sitemap.crawled = toVisit.length;
    for (const loc of toVisit) {
      const r = await gotoCheck(page, loc, 'sitemap');
      if (r.err || r.status >= 400) {
        report.sitemap.fail.push({ loc, status: r.status, err: r.err, js: r.js, pe: r.pe });
      } else {
        report.sitemap.ok += 1;
      }
    }
  } catch (e) {
    report.errors.push('sitemap: ' + e.message);
  }

  // --- admin pages ---
  const adminPages = [
    '/admin/',
    '/admin/dashboard/',
    '/admin/products/',
    '/admin/productcategories/',
    '/admin/orders/',
    '/admin/shoppingcarts/',
    '/admin/customers/',
    '/admin/users/',
    '/admin/faq/',
    '/admin/brands/',
    '/admin/tags/',
    '/admin/coupons/',
    '/admin/stories/',
    '/admin/menus/',
    '/admin/settings/',
    '/admin/report/',
    '/admin/applogs/',
    '/admin/media/',
    '/admin/subscribers/',
    '/admin/mailtemplates/',
    '/admin/templates/',
    '/admin/mainpageimages/',
    '/admin/dashboard/oursitefeatures',
    '/admin/dashboard/searchcontent',
  ];
  for (const p of adminPages) {
    const r = await gotoCheck(page, BASE + p, 'admin');
    report.admin.pages.push(r);
  }

  // --- FAQ CRUD ---
  try {
    const stamp = Date.now().toString(36);
    const name = `QA-SONAR4-${stamp}`;
    await page.goto(`${BASE}/admin/faq/saveoredit`, { waitUntil: 'domcontentloaded', timeout: 45_000 });
    await page.waitForSelector('#Name, input[name="Name"]', { timeout: 15_000 });
    await page.fill('#Name, input[name="Name"]', name);
    const q = page.locator('#Question, textarea[name="Question"], input[name="Question"]');
    if (await q.count()) await q.first().fill(name);
    await page.evaluate((html) => {
      const el = document.querySelector('#Answer, textarea[name="Answer"]');
      if (el) el.value = html;
      if (window.tinymce) {
        const ed = window.tinymce.get('Answer') || window.tinymce.editors[0];
        if (ed) ed.setContent(html);
      }
    }, `<p>${name} answer</p>`);
    const save = page.locator('button[type="submit"], input[type="submit"], .btn-primary').first();
    await save.click();
    await page.waitForLoadState('domcontentloaded');
    await page.goto(`${BASE}/admin/faq/`, { waitUntil: 'domcontentloaded' });
    const listed = await page.getByText(name).count();
    report.admin.crud = { name, listed, url: page.url(), status: listed > 0 ? 'created' : 'not-listed' };
  } catch (e) {
    report.admin.crud = { error: String(e.message || e).slice(0, 400) };
  }

  // --- customer login + cart ---
  try {
    await page.goto(`${BASE}/account/login/`, { waitUntil: 'domcontentloaded', timeout: 45_000 });
    await page.fill('#Email, input[name="Email"]', CUSTOMER_EMAIL);
    await page.fill('#Password, input[name="Password"]', CUSTOMER_PASSWORD);
    await page.locator('button[type="submit"], input[type="submit"]').first().click();
    await page.waitForLoadState('domcontentloaded');
    const afterLogin = page.url();
    const body = await page.locator('body').innerText();
    const loginOk = !/login/i.test(afterLogin) || /customers|hesab|sipari/i.test(body + afterLogin);
    report.customer.login = { url: afterLogin, ok: loginOk, title: await page.title() };

    // product from sitemap or home
    await page.goto(BASE + '/', { waitUntil: 'domcontentloaded' });
    const productLink = page.locator('a[href*="/p/"], a[href*="/products/"]').first();
    if (await productLink.count()) {
      await productLink.click();
      await page.waitForLoadState('domcontentloaded');
    }
    const addBtn = page.locator('#addToCart, .add-to-cart, button:has-text("Sepete"), a:has-text("Sepete"), button:has-text("Add")').first();
    if (await addBtn.count()) {
      await addBtn.click();
      await page.waitForTimeout(800);
    }
    await page.goto(`${BASE}/payment/shoppingcart/`, { waitUntil: 'domcontentloaded' });
    const cartBody = await page.locator('body').innerText();
    report.customer.cart = {
      url: page.url(),
      title: await page.title(),
      hasItems: /adet|qty|toplam|sepet/i.test(cartBody) && !/sepetiniz boş|empty cart/i.test(cartBody.toLowerCase()),
      snippet: cartBody.slice(0, 200),
    };
  } catch (e) {
    report.customer.error = String(e.message || e).slice(0, 400);
  }

  // --- mobile key pages ---
  const mobile = await browser.newContext({
    viewport: { width: 390, height: 844 },
    isMobile: true,
    hasTouch: true,
    userAgent:
      'Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1',
  });
  const mpage = await mobile.newPage();
  for (const p of ['/', '/account/login/', '/payment/shoppingcart/', '/admin/products/', '/admin/faq/']) {
    report.mobile.pages.push(await gotoCheck(mpage, BASE + p, 'mobile'));
  }
  await mobile.close();

  await browser.close();
  report.finishedAt = new Date().toISOString();
  const out = path.join(__dirname, 'tmp-sonar4-qa-report.json');
  fs.writeFileSync(out, JSON.stringify(report, null, 2), 'utf8');
  const adminFails = report.admin.pages.filter((p) => p.err || p.status >= 400);
  const mobileFails = report.mobile.pages.filter((p) => p.err || p.status >= 400);
  console.log(
    JSON.stringify(
      {
        sitemap: { total: report.sitemap.total, crawled: report.sitemap.crawled, ok: report.sitemap.ok, fail: report.sitemap.fail.length },
        adminFails: adminFails.map((p) => ({ url: p.url, status: p.status, err: p.err })),
        crud: report.admin.crud,
        customer: report.customer,
        mobileFails: mobileFails.map((p) => ({ url: p.url, status: p.status, err: p.err })),
        errors: report.errors,
      },
      null,
      2
    )
  );
  if (report.sitemap.fail.length || adminFails.length || mobileFails.length || report.errors.length) {
    process.exitCode = 1;
  }
})().catch((e) => {
  console.error(e);
  process.exit(1);
});
