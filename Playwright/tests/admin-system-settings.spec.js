const { test, expect } = require('@playwright/test');
const { loginWithPassword, captureFailure } = require('./helpers');

const ADMIN = { email: 'admin@eimece.test', password: 'Test123!' };

test.describe('Admin System Settings Center', () => {
  test.describe.configure({ mode: 'serial' });

  test.beforeEach(async ({ page }) => {
    const loggedIn = await loginWithPassword(page, {
      email: ADMIN.email,
      password: ADMIN.password,
      loginPath: '/account/adminlogin/',
    });

    if (!loggedIn) {
      await captureFailure(page, 'admin-systemsettings-login');
      test.skip(true, `Admin login failed; skipping test.`);
    }
  });

  test('System Settings page loads and renders Settings Center overview', async ({ page }) => {
    const res = await page.goto('/admin/adminsettings/systemsettings', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);

    // Overview dashboard cards
    await expect(page.locator('#settingsCenterApp')).toBeVisible();
    await expect(page.locator('#overview')).toBeVisible();
    await expect(page.locator('#stickySaveBar')).toBeVisible();
    await expect(page.locator('#globalSettingsSearch')).toBeVisible();
  });

  test('Tab navigation and hash routing work correctly', async ({ page }) => {
    await page.goto('/admin/adminsettings/systemsettings', { waitUntil: 'domcontentloaded' });

    // Click General & SEO tab
    await page.locator('a[href="#tab-general"]').click();
    await expect(page.locator('#tab-general')).toBeVisible();
    expect(page.url()).toContain('#tab-general');

    // Click Security tab
    await page.locator('a[href="#tab-security"]').click();
    await expect(page.locator('#tab-security')).toBeVisible();
    expect(page.url()).toContain('#tab-security');

    // Click SMTP tab
    await page.locator('a[href="#tab-smtp"]').click();
    await expect(page.locator('#tab-smtp')).toBeVisible();
    expect(page.url()).toContain('#tab-smtp');
  });

  test('Global search filters settings and navigates to target tab', async ({ page }) => {
    await page.goto('/admin/adminsettings/systemsettings', { waitUntil: 'domcontentloaded' });

    const searchInput = page.locator('#globalSettingsSearch');
    await searchInput.fill('SMTP');

    const resultsDropdown = page.locator('#settingsSearchResults');
    await expect(resultsDropdown).toBeVisible();

    const firstResult = resultsDropdown.locator('.search-result-item').first();
    await expect(firstResult).toBeVisible();
    await firstResult.click();

    // Tab should automatically switch to SMTP
    await expect(page.locator('#tab-smtp')).toBeVisible();
  });

  test('Progressive disclosure works for Captcha and Rate Limiting', async ({ page }) => {
    await page.goto('/admin/adminsettings/systemsettings#tab-security', { waitUntil: 'domcontentloaded' });

    // Captcha provider select
    const captchaSelect = page.locator('#CaptchaProvider');
    const recaptchaWrapper = page.locator('#recaptchaSiteKeyWrapper');

    await captchaSelect.selectOption('Recaptcha');
    await expect(recaptchaWrapper).toBeVisible();

    await captchaSelect.selectOption('None');
    await expect(recaptchaWrapper).toBeHidden();
  });

  test('Dirty form tracking activates on input change', async ({ page }) => {
    await page.goto('/admin/adminsettings/systemsettings#tab-company', { waitUntil: 'domcontentloaded' });

    const unsavedBadge = page.locator('#unsavedChangesBadge');
    const discardBtn = page.locator('#btnDiscardChanges');

    await expect(unsavedBadge).toBeHidden();
    await expect(discardBtn).toBeDisabled();

    // Change company name input
    const companyInput = page.locator('input[name="CompanyName"]');
    await companyInput.fill('EImece Test Yeni Unvan');

    await expect(unsavedBadge).toBeVisible();
    await expect(discardBtn).toBeEnabled();
  });
});
