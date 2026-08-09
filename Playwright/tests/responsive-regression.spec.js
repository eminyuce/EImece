const { test, expect } = require('@playwright/test');

const PAGES = [
  '/',
  '/c/pc/elektronik-0j5i6g1b/',
  '/stories/',
  '/payment/shoppingcart/',
  '/account/login/',
  '/p/arama/?search=test',
];

const VIEWPORTS = [
  { name: 'mobile', width: 375, height: 812 },
  { name: 'tablet', width: 768, height: 1024 },
  { name: 'desktop', width: 1440, height: 900 },
];

test.describe('Responsive regression', () => {
  for (const vp of VIEWPORTS) {
    test(`no horizontal overflow @ ${vp.name}`, async ({ browser }) => {
      test.setTimeout(180_000);
      const context = await browser.newContext({ viewport: { width: vp.width, height: vp.height } });
      const page = await context.newPage();

      for (const path of PAGES) {
        const res = await page.goto(path, { waitUntil: 'domcontentloaded', timeout: 45_000 });
        expect(res?.status(), path).toBeLessThan(500);
        await expect(page.locator('body')).not.toContainText('Unhandled exception');

        const overflow = await page.evaluate(() => {
          const doc = document.documentElement;
          return {
            scrollWidth: doc.scrollWidth,
            clientWidth: doc.clientWidth,
            overflowX: doc.scrollWidth > doc.clientWidth + 2,
          };
        });

        expect.soft(overflow.overflowX, `${path} @ ${vp.name} overflow ${overflow.scrollWidth}>${overflow.clientWidth}`).toBeFalsy();
      }

      // Mobile nav toggler
      if (vp.width < 992) {
        await page.goto('/', { waitUntil: 'domcontentloaded' });
        const toggler = page.locator('.navbar-toggler, button.navbar-toggler, .menu-toggle').first();
        if (await toggler.isVisible().catch(() => false)) {
          await toggler.click();
          await page.waitForTimeout(300);
          await expect(page.locator('.navbar-collapse.show, .mobile-menu, .offcanvas.show, nav').first()).toBeVisible();
        }
      }

      await context.close();
    });
  }
});
