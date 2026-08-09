/**
 * Shared helpers for EImece Playwright E2E / async-migration regression.
 */

const fs = require('fs');
const path = require('path');

const BASE_ORIGIN = 'http://localhost:81';

/** @param {import('@playwright/test').Page} page */
async function collectPageIssues(page) {
  const consoleErrors = [];
  const pageErrors = [];
  const failedRequests = [];

  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      consoleErrors.push(msg.text());
    }
  });

  page.on('pageerror', (err) => {
    pageErrors.push(String(err));
  });

  page.on('response', (res) => {
    const status = res.status();
    const url = res.url();
    if (status >= 400) {
      failedRequests.push({ status, url });
    }
  });

  return { consoleErrors, pageErrors, failedRequests };
}

/** @param {import('@playwright/test').Page} page */
async function assertCrizalChrome(page) {
  await page.waitForSelector('body[data-design="crizal"]', { timeout: 15_000 });
  const design = await page.locator('body').getAttribute('data-design');
  if (design !== 'crizal') {
    throw new Error(`Expected data-design=crizal, got ${design}`);
  }
}

/**
 * Soft-filter noise that is not an application defect.
 * @param {string[]} errors
 */
function filterConsoleNoise(errors) {
  const ignore = [
    /favicon/i,
    /Download the React DevTools/i,
    /third-party/i,
    /google-analytics|gtag|googletagmanager/i,
    /whatsapp/i,
    /apple-touch/i,
    /manifest\.json/i,
    // Browser console often omits the URL; pair with failedRequests for real asset checks.
    /^Failed to load resource: the server responded with a status of 404/i,
  ];
  return errors.filter((e) => !ignore.some((re) => re.test(e)));
}

/**
 * @param {{status:number,url:string}[]} failed
 */
function filterAssetFailures(failed) {
  return failed.filter((f) => {
    const u = f.url;
    if (filterNoiseUrl(u)) return false;
    return (
      u.includes('/Content/designs/crizal/') ||
      u.includes('/bundles/designs/crizal/') ||
      u.includes('/images/') ||
      u.includes('/media/') ||
      /\.(css|js|woff2?|ttf|eot|png|jpe?g|svg|gif)(\?|$)/i.test(u)
    );
  });
}

function filterNoiseUrl(url) {
  return /google-analytics|googletagmanager|whatsapp|facebook\.net|hotjar|clarity|apple-touch/i.test(url);
}

/**
 * Critical document / XHR failures (not third-party noise).
 * @param {{status:number,url:string}[]} failed
 */
function filterCriticalFailures(failed) {
  return failed.filter((f) => {
    if (filterNoiseUrl(f.url)) return false;
    try {
      const u = new URL(f.url);
      if (u.origin !== BASE_ORIGIN) return false;
    } catch {
      return false;
    }
    // Expected probes / auth redirects are handled by callers
    if (/\/error\//i.test(f.url) && f.status === 404) return false;
    return f.status >= 400;
  });
}

function normalizeInternalUrl(href, base = BASE_ORIGIN) {
  if (!href || href.startsWith('mailto:') || href.startsWith('tel:') || href.startsWith('javascript:')) {
    return null;
  }
  let url;
  try {
    url = new URL(href, base);
  } catch {
    return null;
  }
  if (url.origin !== BASE_ORIGIN) return null;
  // Drop fragments and tracking
  url.hash = '';
  ['utm_source', 'utm_medium', 'utm_campaign', 'fbclid', 'gclid'].forEach((k) => url.searchParams.delete(k));
  // Skip logout / destructive
  const path = url.pathname.toLowerCase();
  if (path.includes('logoff') || path.includes('logout') || path.includes('delete')) return null;
  // Normalize trailing slash preference of the app
  let normalized = url.pathname;
  if (!normalized.endsWith('/') && !/\.[a-z0-9]+$/i.test(normalized)) {
    normalized += '/';
  }
  const qs = url.searchParams.toString();
  return qs ? `${normalized}?${qs}` : normalized;
}

/**
 * @param {import('@playwright/test').Page} page
 * @param {string} urlPath
 */
async function gotoAndAssertOk(page, urlPath, { expectCrizal = true, allowRedirect = true } = {}) {
  const issues = await collectPageIssues(page);
  const response = await page.goto(urlPath, { waitUntil: 'domcontentloaded', timeout: 45_000 });
  const status = response?.status() ?? 0;
  const finalUrl = page.url();

  if (status >= 500) {
    throw new Error(`HTTP ${status} loading ${urlPath} (final: ${finalUrl})`);
  }

  // Blank / error shell (partial/child endpoints may be intentionally tiny)
  const allowTinyBody = /\/home\/(languages|socialmedialinks|websiteaddressinfo|getcompanyname)/i.test(urlPath);
  const bodyText = (await page.locator('body').innerText().catch(() => '')).trim();
  if (!allowTinyBody && (!bodyText || bodyText.length < 10)) {
    throw new Error(`Blank or nearly empty body for ${urlPath}`);
  }
  if (/Unhandled exception/i.test(bodyText)) {
    throw new Error(`Unhandled exception page for ${urlPath}: ${bodyText.slice(0, 300)}`);
  }

  if (expectCrizal && !/\/account\/|\/admin\/|\/error\//i.test(finalUrl)) {
    // Auth/admin shells may still be Crizal; soft check
    const design = await page.locator('body').getAttribute('data-design').catch(() => null);
    if (design && design !== 'crizal') {
      throw new Error(`Unexpected data-design=${design} on ${urlPath}`);
    }
  }

  await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});

  const consoleErrors = filterConsoleNoise([...issues.consoleErrors, ...issues.pageErrors]);
  const criticalNet = filterCriticalFailures(issues.failedRequests).filter((f) => f.status >= 500);

  return {
    status,
    finalUrl,
    consoleErrors,
    failedRequests: issues.failedRequests,
    criticalNet,
    assetFailures: filterAssetFailures(issues.failedRequests),
    allowRedirect,
  };
}

/**
 * Ensure screenshot directory exists and capture failure artifact.
 * @param {import('@playwright/test').Page} page
 * @param {string} name
 */
async function captureFailure(page, name) {
  const dir = path.join(__dirname, '..', 'screenshots', 'failures');
  fs.mkdirSync(dir, { recursive: true });
  const file = path.join(dir, `${name.replace(/[^\w.-]+/g, '_')}.png`);
  await page.screenshot({ path: file, fullPage: true }).catch(() => {});
  return file;
}

/**
 * Solve Legacy arithmetic captcha by trying sums 2..8 (a,b in 1..4).
 * Reloads captcha between attempts via form redisplay.
 * @param {import('@playwright/test').Page} page
 * @param {() => Promise<void>} fillAndSubmit
 * @param {(page: import('@playwright/test').Page) => Promise<boolean>} isSuccess
 */
async function submitWithLegacyCaptchaBruteForce(page, fillAndSubmit, isSuccess) {
  for (let answer = 2; answer <= 8; answer++) {
    await fillAndSubmit();
    const captcha = page.locator('input[name="Captcha"], #Captcha').first();
    if (await captcha.count()) {
      await captcha.fill(String(answer));
    }
    await Promise.all([
      page.waitForLoadState('domcontentloaded'),
      page.locator('form').first().evaluate((f) => f.requestSubmit()),
    ]).catch(async () => {
      await page.locator('button[type="submit"], input[type="submit"]').first().click();
      await page.waitForLoadState('domcontentloaded');
    });

    if (await isSuccess(page)) {
      return true;
    }
  }
  return false;
}

/**
 * Prefer CaptchaProvider=None in IIS for reliable auth E2E; otherwise brute-force Legacy.
 * Returns whether login navigated away from the login form successfully.
 */
function loginForm(page) {
  return page
    .locator(
      'form.crizal-customer-login__form, form.crizal-admin-login__form, form[action*="/account/login"], form[action*="/account/adminlogin"], .crizal-auth-card form, main form[method="post"]'
    )
    .first();
}

async function loginWithPassword(page, { email, password, loginPath }) {
  await page.goto(loginPath, { waitUntil: 'domcontentloaded' });
  const form = loginForm(page);
  await form.locator('#Email, input[name="Email"]').first().fill(email);
  await form.locator('#Password, input[name="Password"]').first().fill(password);

  const captchaVisible = await form.locator('input[name="Captcha"], #Captcha').first().isVisible().catch(() => false);
  if (!captchaVisible) {
    await Promise.all([
      page.waitForLoadState('domcontentloaded'),
      form.locator('button[type="submit"], input[type="submit"]:visible').first().click(),
    ]);
    return !/\/account\/(login|adminlogin)/i.test(page.url());
  }

  return submitWithLegacyCaptchaBruteForce(
    page,
    async () => {
      await page.goto(loginPath, { waitUntil: 'domcontentloaded' });
      const f = loginForm(page);
      await f.locator('#Email, input[name="Email"]').first().fill(email);
      await f.locator('#Password, input[name="Password"]').first().fill(password);
    },
    async (p) => !/\/account\/(login|adminlogin)/i.test(p.url())
  );
}

function ensureReportDir() {
  const dir = path.join(__dirname, '..', 'test-results');
  fs.mkdirSync(dir, { recursive: true });
  return dir;
}

function writeJsonReport(fileName, data) {
  const dir = ensureReportDir();
  const file = path.join(dir, fileName);
  fs.writeFileSync(file, JSON.stringify(data, null, 2), 'utf8');
  return file;
}

module.exports = {
  BASE_ORIGIN,
  collectPageIssues,
  assertCrizalChrome,
  filterConsoleNoise,
  filterAssetFailures,
  filterCriticalFailures,
  filterNoiseUrl,
  normalizeInternalUrl,
  gotoAndAssertOk,
  captureFailure,
  loginForm,
  loginWithPassword,
  submitWithLegacyCaptchaBruteForce,
  writeJsonReport,
  ensureReportDir,
};
