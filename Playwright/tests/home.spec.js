const { test, expect } = require('@playwright/test');
const {
  collectPageIssues,
  assertCrizalChrome,
  filterConsoleNoise,
  filterAssetFailures,
} = require('./helpers');

test.describe('Crizal Home', () => {
  test('loads with Crizal chrome, assets, and key sections', async ({ page }) => {
    const issues = await collectPageIssues(page);
    const response = await page.goto('/', { waitUntil: 'domcontentloaded' });
    expect(response?.status()).toBeLessThan(400);

    await assertCrizalChrome(page);

    // Plugin CSS + theme bundle
    await expect(page.locator('link[href*="designs/crizal/vendor/css/plugins.css"]')).toHaveCount(1);
    await expect(page.locator('link[href*="bundles/designs/crizal/vendor/css"]')).toHaveCount(1);

    // Header / nav / footer landmarks
    await expect(page.locator('header')).toBeVisible();
    await expect(page.locator('main#main-content')).toBeVisible();
    await expect(page.locator('#logo')).toBeVisible();
    await expect(page.locator('img#logo')).toHaveAttribute('src', /logo/i);

    // Logo image should resolve
    const logoSrc = await page.locator('#logo').getAttribute('src');
    const logoRes = await page.request.get(logoSrc.startsWith('http') ? logoSrc : `http://localhost:81${logoSrc}`);
    expect(logoRes.status()).toBe(200);

    // Hero / slider present (template slider-fade3)
    await expect(page.locator('.slider-fade3, .owl-carousel').first()).toBeVisible();

    // Visible Crizal CTA (hero uses butn-style8; header About is xxl-only/hidden)
    await expect(page.locator('main .butn-style8, .slider-fade3 .butn-style8').first()).toBeVisible();

    await page.waitForTimeout(1500);
    const consoleErrors = filterConsoleNoise(issues.consoleErrors);
    const assetFails = filterAssetFailures(issues.failedRequests);
    expect(consoleErrors, `Console errors: ${consoleErrors.join('\n')}`).toEqual([]);
    expect(assetFails, `Failed assets: ${JSON.stringify(assetFails, null, 2)}`).toEqual([]);

    await page.screenshot({ path: 'screenshots/home.png', fullPage: true });
  });
});
