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
];

(async () => {
  const browser = await chromium.launch({ headless: true });
  const iphone = devices['iPhone 14'];
  const context = await browser.newContext({
    ...iphone,
    hasTouch: true,
    isMobile: true,
  });
  const page = await context.newPage();
  const report = [];

  for (const p of pages) {
    await page.goto(p.url, { waitUntil: 'domcontentloaded', timeout: 90000 });
    await page.waitForTimeout(800);
    await page.screenshot({ path: path.join(outDir, `${p.name}-closed.png`), fullPage: true });

    const metrics = await page.evaluate(() => {
      const body = document.body;
      const sidebar = document.getElementById('adminSidebar');
      const toggle = document.getElementById('adminSidebarToggle');
      const content = document.querySelector('.admin-content');
      const gridWrap = document.querySelector('.eg-grid .grid-wrap, .eg-grid-scroll, .table-responsive');
      const scrollWidth = Math.max(document.documentElement.scrollWidth, body.scrollWidth);
      const clientWidth = document.documentElement.clientWidth;
      const toggleRect = toggle ? toggle.getBoundingClientRect() : null;
      const inputs = Array.from(document.querySelectorAll('.admin-content input[type=text], .admin-content select, .admin-content textarea')).slice(0, 5).map((el) => ({
        tag: el.tagName,
        fontSize: getComputedStyle(el).fontSize,
        minHeight: getComputedStyle(el).minHeight,
      }));
      return {
        title: document.title,
        hasAdminApp: body.classList.contains('admin-app'),
        sidebarOpen: body.classList.contains('sidebar-open'),
        sidebarTransform: sidebar ? getComputedStyle(sidebar).transform : null,
        pageOverflowX: scrollWidth - clientWidth,
        toggleSize: toggleRect ? { w: Math.round(toggleRect.width), h: Math.round(toggleRect.height) } : null,
        contentPad: content ? getComputedStyle(content).padding : null,
        hasGridScroll: !!gridWrap,
        gridOverflowX: gridWrap ? getComputedStyle(gridWrap).overflowX : null,
        inputs,
        hScrollPage: scrollWidth > clientWidth + 2,
      };
    });

    // Open sidebar
    await page.click('#adminSidebarToggle');
    await page.waitForTimeout(400);
    await page.screenshot({ path: path.join(outDir, `${p.name}-sidebar-open.png`) });

    const openState = await page.evaluate(() => ({
      open: document.body.classList.contains('sidebar-open'),
      overlayDisplay: getComputedStyle(document.getElementById('adminSidebarOverlay') || document.body).display,
      overlayZ: getComputedStyle(document.getElementById('adminSidebarOverlay') || document.body).zIndex,
      sidebarZ: getComputedStyle(document.getElementById('adminSidebar') || document.body).zIndex,
      sidebarWidth: document.getElementById('adminSidebar')
        ? Math.round(document.getElementById('adminSidebar').getBoundingClientRect().width)
        : null,
      closeSize: (() => {
        const c = document.getElementById('adminSidebarClose');
        if (!c) return null;
        const r = c.getBoundingClientRect();
        return { w: Math.round(r.width), h: Math.round(r.height) };
      })(),
      firstNavH: (() => {
        const n = document.querySelector('.admin-nav-item');
        return n ? Math.round(n.getBoundingClientRect().height) : null;
      })(),
    }));

    // Close via overlay — click right edge (center may sit under the drawer on ~390px)
    const vp = page.viewportSize();
    await page.mouse.click(vp.width - 12, Math.floor(vp.height / 2));
    await page.waitForTimeout(300);
    let closedAgain = await page.evaluate(() => !document.body.classList.contains('sidebar-open'));

    if (!closedAgain) {
      await page.click('#adminSidebarClose');
      await page.waitForTimeout(300);
      closedAgain = await page.evaluate(() => !document.body.classList.contains('sidebar-open'));
    }

    report.push({ page: p.name, metrics, openState, closedAfterOverlay: closedAgain });
    console.log(p.name, JSON.stringify({ metrics, openState, closedAfterOverlay: closedAgain }));
  }

  fs.writeFileSync(path.join(outDir, 'report.json'), JSON.stringify(report, null, 2));
  await browser.close();
  console.log('done', outDir);
})().catch((e) => {
  console.error(e);
  process.exit(1);
});
