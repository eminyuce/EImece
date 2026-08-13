const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

const outDir = path.join(__dirname, 'test-results', 'admin-grids');
fs.mkdirSync(outDir, { recursive: true });

const pages = [
  { name: 'products', url: 'http://localhost:81/admin/products/' },
  { name: 'orders', url: 'http://localhost:81/admin/orders/' },
  { name: 'customers', url: 'http://localhost:81/admin/customers/' },
  { name: 'users', url: 'http://localhost:81/admin/users/' },
  { name: 'brands', url: 'http://localhost:81/admin/brands/' },
  { name: 'menus', url: 'http://localhost:81/admin/menus/' },
  { name: 'productcategories', url: 'http://localhost:81/admin/productcategories/' },
  { name: 'media', url: 'http://localhost:81/admin/media/' },
  { name: 'coupons', url: 'http://localhost:81/admin/coupons/' },
  { name: 'shoppingcarts', url: 'http://localhost:81/admin/shoppingcarts/' },
  { name: 'stories', url: 'http://localhost:81/admin/stories/' },
  { name: 'tags', url: 'http://localhost:81/admin/tags/' },
  { name: 'lists', url: 'http://localhost:81/admin/lists/' },
  { name: 'subscribers', url: 'http://localhost:81/admin/subscribers/' },
  { name: 'mailtemplates', url: 'http://localhost:81/admin/mailtemplates/' },
  { name: 'faq', url: 'http://localhost:81/admin/faq/' },
  { name: 'mainpageimages', url: 'http://localhost:81/admin/mainpageimages/' },
  { name: 'productcomments', url: 'http://localhost:81/admin/productcomments/' },
  { name: 'applogs', url: 'http://localhost:81/admin/applogs/' },
  { name: 'templates', url: 'http://localhost:81/admin/templates/' },
];

const widths = [320, 375, 390, 430, 768];

(async () => {
  const browser = await chromium.launch({ headless: true });
  const report = [];

  for (const w of widths) {
    const context = await browser.newContext({
      viewport: { width: w, height: 844 },
      deviceScaleFactor: 2,
      isMobile: w < 768,
      hasTouch: w < 768,
      userAgent:
        'Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1',
    });
    const page = await context.newPage();

    for (const p of pages) {
      let status = 0;
      try {
        const resp = await page.goto(p.url, { waitUntil: 'domcontentloaded', timeout: 60000 });
        status = resp ? resp.status() : 0;
        await page.waitForTimeout(500);
      } catch (e) {
        report.push({ width: w, page: p.name, error: String(e.message || e) });
        continue;
      }

      const metrics = await page.evaluate(() => {
        const body = document.body;
        const wrap = document.querySelector('.eg-grid .grid-wrap, .eg-grid-scroll, .grid-mvc .grid-wrap');
        const table = document.querySelector('.eg-grid table.grid-table, table.grid-table');
        const shell = document.querySelector('.eg-grid-shell, .grid-mvc.eg-grid, .grid-mvc');
        const actions = Array.from(
          document.querySelectorAll('.eg-actions-compact a, .eg-actions-compact .btn, .eg-col-actions a, .eg-col-actions .btn')
        ).slice(0, 6);
        const pager = document.querySelector('.eg-pager a, .pagination a, .grid-footer a');
        const filterBtn = document.querySelector('.grid-filter-btn, .grid-filter');
        const ths = Array.from(document.querySelectorAll('table.grid-table thead th')).map((th) => ({
          t: (th.textContent || '').trim().slice(0, 24),
          cls: th.className,
          visible: getComputedStyle(th).display !== 'none' && th.getBoundingClientRect().width > 0,
          w: Math.round(th.getBoundingClientRect().width),
        }));
        const pageOverflow = Math.max(document.documentElement.scrollWidth, body.scrollWidth) - document.documentElement.clientWidth;
        let wrapScrollOk = false;
        if (wrap && table) {
          const before = wrap.scrollLeft;
          wrap.scrollLeft = 80;
          wrapScrollOk = wrap.scrollLeft >= 40 || wrap.scrollWidth <= wrap.clientWidth + 2;
          wrap.scrollLeft = before;
        }
        const tinyActions = actions
          .map((el) => {
            const r = el.getBoundingClientRect();
            return { t: (el.textContent || el.title || '').trim().slice(0, 20), w: Math.round(r.width), h: Math.round(r.height) };
          })
          .filter((a) => a.w > 0 && a.h > 0 && (a.w < 40 || a.h < 40));

        return {
          title: (document.querySelector('h2, .admin-page-title') || {}).textContent || '',
          hasGrid: !!table,
          pageOverflow,
          wrapOverflowX: wrap ? getComputedStyle(wrap).overflowX : null,
          wrapW: wrap ? Math.round(wrap.clientWidth) : null,
          tableW: table ? Math.round(table.getBoundingClientRect().width) : null,
          canScrollGrid: wrap ? wrap.scrollWidth > wrap.clientWidth + 2 : false,
          wrapScrollOk,
          thVisible: ths.filter((t) => t.visible).length,
          thTotal: ths.length,
          ths: ths.slice(0, 12),
          tinyActions: tinyActions.slice(0, 6),
          pagerH: pager ? Math.round(pager.getBoundingClientRect().height) : null,
          filterH: filterBtn ? Math.round(filterBtn.getBoundingClientRect().height) : null,
          shellCls: shell ? shell.className.slice(0, 80) : null,
        };
      });

      const shotName = `${p.name}-w${w}.png`;
      if (w === 390 || (w === 320 && ['products', 'orders', 'customers', 'users', 'media', 'shoppingcarts'].includes(p.name))) {
        await page.screenshot({ path: path.join(outDir, shotName) });
      }

      const issues = [];
      if (!metrics.hasGrid) issues.push('no-grid');
      if (metrics.pageOverflow > 2) issues.push('page-overflow:' + metrics.pageOverflow);
      if (metrics.hasGrid && metrics.canScrollGrid && !metrics.wrapScrollOk) issues.push('grid-scroll-broken');
      if (metrics.hasGrid && metrics.wrapOverflowX !== 'auto' && metrics.wrapOverflowX !== 'scroll') issues.push('wrap-overflow:' + metrics.wrapOverflowX);
      if (metrics.tinyActions.length) issues.push('tiny-actions');
      if (metrics.pagerH && metrics.pagerH < 40) issues.push('tiny-pager:' + metrics.pagerH);

      report.push({ width: w, page: p.name, status, finalUrl: page.url(), issues, metrics });
      if (issues.length) console.log('ISSUE', w, p.name, issues.join(', '), 'status', status);
      else console.log('ok', w, p.name, 'cols', metrics.thVisible + '/' + metrics.thTotal, 'tableW', metrics.tableW);
    }

    await context.close();
  }

  fs.writeFileSync(path.join(outDir, 'report.json'), JSON.stringify(report, null, 2));
  const bad = report.filter((r) => r.issues && r.issues.length);
  console.log('DONE bad=', bad.length, 'total=', report.length, outDir);
  await browser.close();
})().catch((e) => {
  console.error(e);
  process.exit(1);
});
