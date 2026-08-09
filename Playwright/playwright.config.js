// @ts-check
const { defineConfig, devices } = require('@playwright/test');

/**
 * Crizal theme validation against the IIS-deployed ASP.NET MVC app.
 * Target: http://localhost:81/
 */
module.exports = defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  // IIS Express/local site is sensitive to parallel cold starts after app-pool recycle.
  workers: process.env.CI ? 2 : 2,
  reporter: [['list'], ['html', { open: 'never', outputFolder: 'playwright-report' }]],
  outputDir: 'test-results',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  use: {
    baseURL: 'http://localhost:81',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'off',
    ignoreHTTPSErrors: true,
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
