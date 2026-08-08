const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

const viewports = [
  { width: 320, height: 800 },
  { width: 375, height: 812 },
  { width: 414, height: 896 },
  { width: 768, height: 1024 },
  { width: 1024, height: 768 },
  { width: 1280, height: 800 },
  { width: 1440, height: 900 },
  { width: 1920, height: 1080 },
];

test.describe('Crizal Responsive', () => {
  for (const vp of viewports) {
    test(`home has no horizontal overflow at ${vp.width}x${vp.height}`, async ({ page }) => {
      await page.setViewportSize(vp);
      await page.goto('/', { waitUntil: 'domcontentloaded' });
      await assertCrizalChrome(page);

      const metrics = await page.evaluate(() => {
        const doc = document.documentElement;
        return {
          scrollWidth: doc.scrollWidth,
          clientWidth: doc.clientWidth,
          bodyScrollWidth: document.body.scrollWidth,
        };
      });

      // Allow 1px tolerance for subpixel rounding
      expect(
        metrics.scrollWidth,
        `Horizontal overflow at ${vp.width}: scrollWidth=${metrics.scrollWidth}`
      ).toBeLessThanOrEqual(metrics.clientWidth + 1);

      await expect(page.locator('header')).toBeVisible();
      await expect(page.locator('main#main-content')).toBeVisible();

      await page.screenshot({
        path: `screenshots/responsive-home-${vp.width}.png`,
        fullPage: false,
      });
    });
  }
});
