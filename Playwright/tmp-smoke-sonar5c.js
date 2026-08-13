const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
const { loginWithPassword } = require('./tests/helpers');

const BASE = 'http://localhost:81';
const CUSTOMER = { email: 'eminyuce1111@gmail.com', password: 'V02y.qcF' };
const SHOT = path.join(__dirname, 'screenshots', 'smoke-sonar5');
fs.mkdirSync(SHOT, { recursive: true });
const rows = [];

function classify(status, body) {
  const text = (body || '').slice(0, 2500);
  const unhandled = /Unhandled exception|Server Error in Application|Stack Trace:/i.test(text);
  let kind = 'ok';
  if (status >= 500 || unhandled) kind = '5xx';
  else if (status === 404) kind = '404';
  else if (status >= 400) kind = '4xx';
  return { kind, unhandled, snippet: text.replace(/\s+/g, ' ').slice(0, 180) };
}

async function probe(request, url, area) {
  try {
    const res = await request.get(url, { timeout: 45_000, maxRedirects: 5 });
    const body = await res.text().catch(() => '');
    const c = classify(res.status(), body);
    rows.push({ area, url, status: res.status(), final: res.url(), ...c });
    console.log(`${c.kind.toUpperCase().padEnd(4)} ${res.status()} ${url}`);
  } catch (e) {
    rows.push({ area, url, status: 0, kind: 'error', snippet: String(e.message).slice(0, 180) });
    console.log(`ERR  ${url} ${e.message}`);
  }
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const page = await context.newPage();
  const request = context.request;

  const loggedIn = await loginWithPassword(page, {
    email: CUSTOMER.email,
    password: CUSTOMER.password,
    loginPath: BASE + '/account/login',
  });
  console.log('customer loggedIn', loggedIn, page.url());
  await page.screenshot({ path: path.join(SHOT, 'customer-after-login.png') }).catch(() => {});
  rows.push({ area: 'customer-login', url: page.url(), kind: loggedIn ? 'ok' : 'warn', status: loggedIn ? 200 : 401 });

  for (const p of [
    '/Customers/Home/Index',
    '/Customers/Home/CustomerOrders',
    '/Customers/Home/Faq',
    '/Customers/Home/ChangePassword',
    '/Customers/Home/SendMessageToSeller',
    '/Customers/Home/WebSiteAddressInfo',
  ]) {
    await page.goto(BASE + p, { waitUntil: 'domcontentloaded', timeout: 45_000 });
    const html = await page.content();
    const kind = /Server Error|Unhandled exception/i.test(html) ? '5xx' : 'ok';
    const title = await page.title();
    rows.push({ area: 'customer', url: page.url(), status: kind === '5xx' ? 500 : 200, kind, snippet: title });
    console.log(`${kind.toUpperCase().padEnd(4)} ${p} ${title} ${page.url()}`);
    await page.screenshot({ path: path.join(SHOT, `cust-${p.replace(/\W+/g, '_')}.png`) }).catch(() => {});
  }

  await page.goto(BASE + '/admin', { waitUntil: 'domcontentloaded', timeout: 60_000 });
  const adminBypass = !/adminlogin/i.test(page.url());
  console.log('admin bypass', adminBypass, page.url(), await page.title());
  await page.screenshot({ path: path.join(SHOT, 'admin-dashboard.png') }).catch(() => {});
  rows.push({ area: 'admin-dashboard', url: page.url(), kind: adminBypass ? 'ok' : 'warn', status: 200, snippet: await page.title() });

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
  const fail = rows.filter((r) => r.kind === '5xx' || r.kind === 'error');
  const warn = rows.filter((r) => r.kind === 'warn' || r.kind === '404' || r.kind === '4xx');
  fs.writeFileSync(path.join(__dirname, 'smoke-sonar5-part3.json'), JSON.stringify({ fail, warn, rows }, null, 2));
  console.log('\nFAIL', fail.length);
  fail.forEach((f) => console.log(` ${f.kind} ${f.status} ${f.url} ${f.snippet || ''}`));
  console.log('WARN', warn.length);
  warn.forEach((w) => console.log(` ${w.kind} ${w.status} ${w.url}`));
  process.exit(fail.length ? 1 : 0);
})().catch((e) => { console.error(e); process.exit(2); });
