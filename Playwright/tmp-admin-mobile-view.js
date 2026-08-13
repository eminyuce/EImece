const { chromium, devices } = require('playwright');
const path = require('path');
const fs = require('fs');

const outDir = path.join(__dirname, 'test-results', 'admin-mobile');
fs.mkdirSync(outDir, { recursive: true });

const pages = [
  { name: 'dashboard', url: 'http://localhost:81/admin/dashboard/' },
  { name: 'products', url: 'http://localhost:81/admin/products/' },
  { name: 'orders', url: 'http://localhost:81/admin/orders/' },
  { name: 'customers', url: 'http://localhost:81/admin/customers/' },
  { name: 'settings', url: 'http://localhost:81/admin/adminsettings/systemsettings/' },
  { name: 'media', url: 'http://localhost:81/admin/media/' },
];

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    ...devices['iPhone 14'],
    hasTouch: true,
    isMobile: true,
  });
  const page = await context.newPage();

  for (const p of pages) {
    const resp = await page.goto(p.url, { waitUntil: 'domcontentloaded', timeout: 90000 });
    await page.waitForTimeout(700);
    const finalUrl = page.url();
    await page.screenshot({ path: path.join(outDir, `${p.name}-vp.png`) });

    const info = await page.evaluate(() => {
      const table = document.querySelector('.eg-grid table.grid-table, table.grid-table, .admin-content table');
      const wrap = document.querySelector('.eg-grid .grid-wrap, .eg-grid-scroll, .table-responsive');
      const selects = Array.from(document.querySelectorAll('.admin-content select')).slice(0, 8).map((el) => ({
        id: el.id || el.name || '',
        className: el.className,
        fontSize: getComputedStyle(el).fontSize,
        width: Math.round(el.getBoundingClientRect().width),
      }));
      const tinyBtns = Array.from(document.querySelectorAll('.admin-content .btn, .admin-content a.btn, .admin-content .eg-actions-compact a, .admin-content .eg-actions-compact button'))
        .slice(0, 40)
        .map((el) => {
          const r = el.getBoundingClientRect();
          return { t: (el.textContent || '').trim().slice(0, 24), w: Math.round(r.width), h: Math.round(r.height) };
        })
        .filter((b) => b.w > 0 && b.h > 0 && (b.w < 44 || b.h < 44));
      const cards = document.querySelectorAll('.admin-dash-card, .dashboard-card, .admin-content .thumbnail, .admin-content .panel');
      const tree = document.querySelector('.eg-category-tree, .treeProducts');
      return {
        h1: (document.querySelector('.admin-content h2, .admin-page-title, .admin-content h1') || {}).textContent || '',
        tableW: table ? Math.round(table.getBoundingClientRect().width) : null,
        tableScrollW: table ? table.scrollWidth : null,
        wrapOverflow: wrap ? getComputedStyle(wrap).overflowX : null,
        wrapW: wrap ? Math.round(wrap.getBoundingClientRect().width) : null,
        selects,
        tinyBtns: tinyBtns.slice(0, 12),
        cardCount: cards.length,
        treeH: tree ? Math.round(tree.getBoundingClientRect().height) : null,
        bodyScrollW: document.body.scrollWidth,
        clientW: document.documentElement.clientWidth,
      };
    });
    console.log(p.name, 'status', resp && resp.status(), 'url', finalUrl);
    console.log(JSON.stringify(info, null, 2));
  }

  await browser.close();
})().catch((e) => { console.error(e); process.exit(1); });
