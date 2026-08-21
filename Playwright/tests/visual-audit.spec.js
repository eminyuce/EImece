const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

/**
 * Playwright-driven visual audit of the Crizal theme on IIS.
 * Captures screenshots, overflow, console/network failures, and design markers.
 */

const VIEWPORTS = [
  { width: 320, height: 800 },
  { width: 375, height: 812 },
  { width: 414, height: 896 },
  { width: 768, height: 1024 },
  { width: 1024, height: 768 },
  { width: 1280, height: 800 },
  { width: 1440, height: 900 },
  { width: 1920, height: 1080 },
];

/** Actual routes discovered from the live app / prior audit */
const PAGES = [
  { key: 'home', path: '/', folder: 'home', fullPageDesktop: true },
  { key: 'products', path: '/c/pc/elektronik-7e7e4h1b/', folder: 'products', fullPageDesktop: true },
  { key: 'product-detail', path: '/p/oturma-grubu/nordline-mese-sehpa-90cm-130-4h4h2d5i4h1b/', folder: 'product-detail', fullPageDesktop: true },
  { key: 'cart', path: '/Payment/ShoppingCart', folder: 'cart', fullPageDesktop: true },
  { key: 'checkout-guest', path: '/Payment/ShoppingWithoutAccount', folder: 'checkout', fullPageDesktop: true },
  { key: 'login', path: '/Account/Login', folder: 'account', fullPageDesktop: true },
  { key: 'register', path: '/Account/Register', folder: 'account', fullPageDesktop: true },
  { key: 'forgot-password', path: '/Account/ForgotPassword', folder: 'account', fullPageDesktop: true },
  { key: 'stories', path: '/stories/', folder: 'stories', fullPageDesktop: true },
  { key: 'search', path: '/p/arama?search=kulaklik', folder: 'search', fullPageDesktop: true },
  { key: 'about', path: '/info/aboutus/', folder: 'content', fullPageDesktop: true },
  { key: 'contact', path: '/i/iletisim-1b9a2d6g/', folder: 'content', fullPageDesktop: true },
  { key: 'error-404', path: '/Error/NotFound', folder: 'errors', fullPageDesktop: true },
  { key: 'error-index', path: '/Error/Index', folder: 'errors', fullPageDesktop: true },
];

const report = {
  generatedAt: new Date().toISOString(),
  baseURL: 'http://localhost:81',
  pages: [],
};

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function filterNoiseUrl(url) {
  return /google-analytics|googletagmanager|whatsapp|facebook\.net|hotjar|clarity/i.test(url);
}

function filterConsoleNoise(text) {
  return /favicon|Download the React DevTools|google-analytics|gtag|whatsapp/i.test(text);
}

test.describe.configure({ mode: 'serial' });

test('Crizal visual audit — capture + metrics', async ({ browser }) => {
  test.setTimeout(600_000);
  const root = path.join(__dirname, '..', 'screenshots');
  ensureDir(root);

  for (const pageDef of PAGES) {
    const pageReport = {
      key: pageDef.key,
      path: pageDef.path,
      status: null,
      crizal: false,
      pendingOldTheme: false,
      consoleErrors: [],
      failedAssets: [],
      viewports: [],
      notes: [],
    };

    const context = await browser.newContext({
      viewport: { width: 1280, height: 800 },
    });
    const page = await context.newPage();

    const consoleErrors = [];
    const failedAssets = [];

    page.on('console', (msg) => {
      if (msg.type() === 'error' && !filterConsoleNoise(msg.text())) {
        consoleErrors.push(msg.text());
      }
    });
    page.on('pageerror', (err) => {
      consoleErrors.push(String(err));
    });
    page.on('response', (res) => {
      const status = res.status();
      const url = res.url();
      if (status >= 400 && !filterNoiseUrl(url)) {
        if (
          url.includes('/Content/designs/crizal/') ||
          url.includes('/bundles/designs/crizal/') ||
          /\.(css|js|woff2?|ttf|eot|png|jpe?g|svg|gif)(\?|$)/i.test(url) ||
          url.includes('/images/') ||
          url.includes('/media/')
        ) {
          failedAssets.push({ status, url });
        }
      }
    });

    const response = await page.goto(pageDef.path, { waitUntil: 'domcontentloaded', timeout: 45_000 });
    pageReport.status = response?.status() ?? null;

    // Wait for preloader / fonts
    await page.waitForTimeout(600);

    const design = await page.locator('body').getAttribute('data-design').catch(() => null);
    pageReport.crizal = design === 'crizal';
    if (!pageReport.crizal) {
      pageReport.pendingOldTheme = true;
      pageReport.notes.push('Crizal implementation pending or old theme loaded');
    }

    // Desktop full-page shot at 1280
    const folder = path.join(root, pageDef.folder);
    ensureDir(folder);
    await page.setViewportSize({ width: 1280, height: 800 });
    await page.waitForTimeout(200);
    await page.screenshot({
      path: path.join(folder, `${pageDef.key}-1280-full.png`),
      fullPage: true,
    });

    // Collect design signals at 1280
    const signals = await page.evaluate(() => {
      const body = document.body;
      const cs = getComputedStyle(body);
      const header = document.querySelector('header');
      const footer = document.querySelector('footer, .footer');
      const main = document.querySelector('main');
      const container = document.querySelector('main .container, main .container-fluid, .container');
      const btn = document.querySelector('.butn-style8, .butn, .crizal-btn, .btn-primary');
      const card = document.querySelector('.card, .product-grid, .shop-product, .crizal-product-card');
      const logo = document.querySelector('#logo, .navbar-brand img');

      function box(el) {
        if (!el) return null;
        const r = el.getBoundingClientRect();
        return { w: Math.round(r.width), h: Math.round(r.height), top: Math.round(r.top), left: Math.round(r.left) };
      }

      // Find elements wider than viewport
      const vw = document.documentElement.clientWidth;
      const overflowEls = [];
      document.querySelectorAll('body *').forEach((el) => {
        const r = el.getBoundingClientRect();
        if (r.width > vw + 2 && r.height > 0) {
          const tag = el.tagName.toLowerCase();
          const cls = (el.className && String(el.className).slice(0, 80)) || '';
          overflowEls.push({ tag, cls, width: Math.round(r.width) });
        }
      });

      return {
        fontFamily: cs.fontFamily,
        bodyBg: cs.backgroundColor,
        hasHeader: !!header,
        hasFooter: !!footer,
        hasMain: !!main,
        headerBox: box(header),
        footerBox: box(footer),
        mainBox: box(main),
        containerBox: box(container),
        buttonBox: box(btn),
        cardBox: box(card),
        logoBox: box(logo),
        logoNatural: logo && logo.naturalWidth ? { nw: logo.naturalWidth, nh: logo.naturalHeight, complete: logo.complete } : null,
        scrollWidth: document.documentElement.scrollWidth,
        clientWidth: document.documentElement.clientWidth,
        overflowEls: overflowEls.slice(0, 12),
        butnCount: document.querySelectorAll('.butn-style8, .butn').length,
        mstoreMarkers: !!document.querySelector('[class*="cz-"], .cz-sidebar, .cz-cart'),
        bootstrapGrid: document.querySelectorAll('.row, .col-md-6, .container').length,
      };
    });
    pageReport.signals1280 = signals;

    // Per-viewport screenshots + overflow check (viewport shot, not always full page for speed)
    for (const vp of VIEWPORTS) {
      await page.setViewportSize(vp);
      await page.waitForTimeout(200);

      const metrics = await page.evaluate(() => {
        const doc = document.documentElement;
        const scrollWidth = doc.scrollWidth;
        const clientWidth = doc.clientWidth;
        const overflowing = [];
        document.querySelectorAll('body *').forEach((el) => {
          const r = el.getBoundingClientRect();
          if (r.right > clientWidth + 2 && r.width > 20 && r.height > 10) {
            overflowing.push({
              tag: el.tagName.toLowerCase(),
              cls: (el.className && String(el.className).slice(0, 60)) || '',
              right: Math.round(r.right),
              width: Math.round(r.width),
            });
          }
        });
        return {
          scrollWidth,
          clientWidth,
          hasOverflow: scrollWidth > clientWidth + 1,
          overflowing: overflowing.slice(0, 8),
          navTogglerVisible: !!(
            document.querySelector('.navbar-toggler') &&
            getComputedStyle(document.querySelector('.navbar-toggler')).display !== 'none'
          ),
        };
      });

      const shotName = `${pageDef.key}-${vp.width}.png`;
      await page.screenshot({
        path: path.join(folder, shotName),
        fullPage: false,
      });

      pageReport.viewports.push({
        ...vp,
        ...metrics,
        screenshot: path.join(pageDef.folder, shotName).replace(/\\/g, '/'),
      });
    }

    pageReport.consoleErrors = [...new Set(consoleErrors)].slice(0, 20);
    pageReport.failedAssets = failedAssets.slice(0, 30);

    report.pages.push(pageReport);
    await context.close();
  }

  const reportPath = path.join(root, 'visual-audit-report.json');
  fs.writeFileSync(reportPath, JSON.stringify(report, null, 2), 'utf8');

  // Soft assertions — collect failures rather than aborting mid-audit
  const nonCrizal = report.pages.filter((p) => !p.crizal && !p.key.startsWith('error-'));
  const overflowPages = report.pages.filter((p) => p.viewports.some((v) => v.hasOverflow));
  const assetFails = report.pages.filter((p) => p.failedAssets.length);
  const consoleFails = report.pages.filter((p) => p.consoleErrors.length);

  console.log('\n===== CRIZAL VISUAL AUDIT SUMMARY =====');
  console.log(`Pages audited: ${report.pages.length}`);
  console.log(`Non-Crizal: ${nonCrizal.map((p) => p.key).join(', ') || 'none'}`);
  console.log(`Overflow: ${overflowPages.map((p) => p.key).join(', ') || 'none'}`);
  console.log(`Asset failures: ${assetFails.map((p) => p.key).join(', ') || 'none'}`);
  console.log(`Console errors: ${consoleFails.map((p) => p.key).join(', ') || 'none'}`);
  console.log(`Report: ${reportPath}`);

  expect(nonCrizal, `Pages not on Crizal: ${JSON.stringify(nonCrizal.map((p) => p.key))}`).toEqual([]);
});
