const { test, expect } = require('@playwright/test');
const { assertCrizalChrome, loginWithPassword, loginForm, captureFailure } = require('./helpers');

const CUSTOMER = { email: 'eminyuce1111@gmail.com', password: 'V02y.qcF' };
const ADMIN = { email: 'admin@eimece.test', password: 'Test123!' };

test.describe('Authentication flows', () => {
  test('login / register / forgot-password pages render forms', async ({ page }) => {
    for (const path of ['/account/login/', '/account/register/', '/account/forgotpassword/']) {
      const res = await page.goto(path, { waitUntil: 'domcontentloaded' });
      expect(res?.status()).toBeLessThan(500);
      await assertCrizalChrome(page);
      await expect(loginForm(page)).toBeVisible();
    }

    // Admin login uses a dedicated non-Crizal shell.
    const adminRes = await page.goto('/account/adminlogin/', { waitUntil: 'domcontentloaded' });
    expect(adminRes?.status()).toBeLessThan(500);
    await expect(page.locator('form[action*="adminlogin"], form.needs-validation').first()).toBeVisible();
    await expect(page.locator('input[name="Email"]')).toBeVisible();
  });

  test('customer login succeeds and reaches customers area', async ({ page }) => {
    test.setTimeout(180_000);
    const ok = await loginWithPassword(page, {
      email: CUSTOMER.email,
      password: CUSTOMER.password,
      loginPath: '/account/login/',
    });

    if (!ok) {
      await captureFailure(page, 'customer-login-failed');
    }
    expect(ok, `Customer login failed. URL=${page.url()}`).toBeTruthy();
    expect(page.url()).toMatch(/\/customers/i);
    await expect(page.locator('body')).not.toContainText('Unhandled exception');
  });

  test('admin login reaches admin or authenticator challenge', async ({ page }) => {
    test.setTimeout(180_000);
    const ok = await loginWithPassword(page, {
      email: ADMIN.email,
      password: ADMIN.password,
      loginPath: '/account/adminlogin/',
    });

    if (!ok) {
      await captureFailure(page, 'admin-login-failed');
    }
    expect(ok, `Admin login failed. URL=${page.url()}`).toBeTruthy();
    expect(page.url()).toMatch(/\/admin|verifyauthenticator|enableauthenticator/i);
    await expect(page.locator('body')).not.toContainText('Unhandled exception');
  });

  test('wrong password stays on login with validation', async ({ page }) => {
    test.setTimeout(120_000);
    await page.goto('/account/login/', { waitUntil: 'domcontentloaded' });
    const form = loginForm(page);
    await form.locator('#Email, input[name="Email"]').first().fill(CUSTOMER.email);
    await form.locator('#Password, input[name="Password"]').first().fill('WrongPassword!');
    const captcha = form.locator('input[name="Captcha"], #Captcha').first();
    if (await captcha.isVisible().catch(() => false)) {
      await captcha.fill('4');
    }
    await form.locator('button[type="submit"], input[type="submit"]:visible').first().click();
    await page.waitForLoadState('domcontentloaded');
    expect(page.url()).toMatch(/\/account\/login/i);
    expect(page.url()).not.toMatch(/\/customers/i);
  });
});
