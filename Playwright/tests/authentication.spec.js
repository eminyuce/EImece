const { test, expect } = require('@playwright/test');
const { assertCrizalChrome } = require('./helpers');

test.describe('Crizal Authentication', () => {
  test('login page renders Crizal auth form', async ({ page }) => {
    const res = await page.goto('/Account/Login', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);

    const authForm = page.locator('main form.login, .crizal-auth-card form, form[action*="Login"]').first();
    await expect(authForm).toBeVisible();
    await expect(page.locator('#Email, input[name="Email"]').first()).toBeVisible();
    await expect(page.locator('#Password, input[name="Password"]').first()).toBeVisible();
    await page.screenshot({ path: 'screenshots/login.png', fullPage: true });
  });

  test('register page renders Crizal form', async ({ page }) => {
    const res = await page.goto('/Account/Register', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);

    const authForm = page.locator('main form, .crizal-auth-card form, form[action*="Register"]').first();
    await expect(authForm).toBeVisible();
    await page.screenshot({ path: 'screenshots/register.png', fullPage: true });
  });

  test('forgot password page renders', async ({ page }) => {
    const res = await page.goto('/Account/ForgotPassword', { waitUntil: 'domcontentloaded' });
    expect(res?.status()).toBe(200);
    await assertCrizalChrome(page);
  });
});
