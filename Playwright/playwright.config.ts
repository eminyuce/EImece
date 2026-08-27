import { defineConfig, devices } from '@playwright/test';

/**
 * EImece E2E — iyzico sandbox on Chromium only
 *
 * Base URL precedence:
 *   1. EIMECE_BASE_URL env (e.g. https://enlargement-army-authorization-syntax.trycloudflare.com)
 *   2. PLAYWRIGHT_BASE_URL env (generic Playwright convention)
 *   3. http://localhost:81 (local IIS default for the repo)
 *
 * Run:
 *   npx playwright test --project=chromium
 *   EIMECE_BASE_URL=https://<your-tunnel>.trycloudflare.com npx playwright test --project=chromium
 */
const baseURL =
  process.env.EIMECE_BASE_URL ||
  process.env.PLAYWRIGHT_BASE_URL ||
  process.env.BASE_URL ||
  'http://localhost:81';

export default defineConfig({
  testDir: './tests',
  // Keep legacy JS specs (tests/*.spec.js) plus the new TS e2e suite (tests/e2e/**/*.spec.ts)
  testMatch: ['**/*.spec.ts', '**/*.spec.js'],
  fullyParallel: false, // checkout/3DS + shared cart cookie are safer serially; bump to true once stable
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : 1,
  reporter: [
    ['list'],
    ['html', { open: 'never', outputFolder: 'playwright-report' }],
    ['junit', { outputFile: 'test-results/junit.xml' }],
  ],
  outputDir: 'test-results',
  // Checkout + iyzico initialize + 3DS can be slow; keep generous.
  timeout: 120_000,
  expect: { timeout: 15_000 },
  use: {
    baseURL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    actionTimeout: 15_000,
    navigationTimeout: 45_000,
    ignoreHTTPSErrors: true,
    locale: 'tr-TR',
    timezoneId: 'Europe/Istanbul',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  // No webServer — target is already running (IIS or trycloudflare tunnel).
});
