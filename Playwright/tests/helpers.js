/**
 * Shared helpers for Crizal theme Playwright validation.
 */

/** @param {import('@playwright/test').Page} page */
async function collectPageIssues(page) {
  const consoleErrors = [];
  const failedRequests = [];

  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      consoleErrors.push(msg.text());
    }
  });

  page.on('response', (res) => {
    const status = res.status();
    const url = res.url();
    if (status >= 400) {
      // Ignore expected auth redirects / intentional error-page probes
      failedRequests.push({ status, url });
    }
  });

  return { consoleErrors, failedRequests };
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
 * Soft-filter noise that is not a Crizal theme defect.
 * @param {string[]} errors
 */
function filterConsoleNoise(errors) {
  const ignore = [
    /favicon/i,
    /Download the React DevTools/i,
    /third-party/i,
    /google-analytics|gtag|googletagmanager/i,
    /whatsapp/i,
    /Failed to load resource: the server responded with a status of 404.*apple-touch/i,
  ];
  return errors.filter((e) => !ignore.some((re) => re.test(e)));
}

/**
 * @param {import('@playwright/test').Page} page
 * @param {{status:number,url:string}[]} failed
 */
function filterAssetFailures(failed) {
  return failed.filter((f) => {
    const u = f.url;
    if (u.includes('google-analytics') || u.includes('googletagmanager') || u.includes('whatsapp')) {
      return false;
    }
    // Crizal / app asset failures we care about
    return (
      u.includes('/Content/designs/crizal/') ||
      u.includes('/bundles/designs/crizal/') ||
      u.includes('/images/') ||
      u.includes('/media/') ||
      /\.(css|js|woff2?|ttf|eot|png|jpe?g|svg|gif)(\?|$)/i.test(u)
    );
  });
}

module.exports = {
  collectPageIssues,
  assertCrizalChrome,
  filterConsoleNoise,
  filterAssetFailures,
};
