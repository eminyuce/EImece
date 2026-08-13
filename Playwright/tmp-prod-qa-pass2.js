/**
 * Pass 2: admin, reports, cart/checkout, auth, media — appends to e2e-qa-report.json
 */
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
const { loginWithPassword } = require('./tests/helpers');
const {
  PRODUCT_DETAIL,
  uniqueCustomerEmail,
  openProductDetail,
  addToCartFromDetail,
  goToCart,
  updateFirstCartQuantity,
  registerCustomer,
  proceedToMembershipCheckout,
  fillBillingDetails,
  placeOrderIfReady,
  tryPayWithIyzicoSandbox,
} = require('./tests/customer-flow-helpers');

const BASE = 'http://localhost:81';
const REPORT_PATH = path.join(__dirname, 'e2e-qa-report.json');
const SHOT_DIR = path.join(__dirname, 'screenshots', 'prod-qa');
const CUSTOMER = { email: 'eminyuce1111@gmail.com', password: 'V02y.qcF' };
fs.mkdirSync(SHOT_DIR, { recursive: true });

const issues = [];
const testedAreas = new Set();

function addIssue(p) {
  issues.push(p);
}

async function shot(page, name) {
  const file = path.join(SHOT_DIR, `${name}.png`);
  await page.screenshot({ path: file, fullPage: false }).catch(() => {});
  return `Playwright/screenshots/prod-qa/${name}.png`;
}

async function safeGoto(page, url) {
  try {
    const res = await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 45_000 });
    await page.waitForTimeout(300);
    const body = await page.locator('body').innerText().catch(() => '');
    return { status: res ? res.status() : 0, url: page.url(), body, error: null };
  } catch (e) {
    return { status: 0, url: page.url(), body: '', error: String(e) };
  }
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    ignoreHTTPSErrors: true,
    baseURL: BASE,
  });
  const page = await context.newPage();
  const request = context.request;

  // Logo GET
  testedAreas.add('Frontend/Customer pages');
  const logo = await request.get(BASE + '/images/logo.jpg');
  if (logo.status() === 404) {
    addIssue({
      severity: 'High',
      area: 'Frontend',
      page: '/images/logo.jpg',
      title: 'Site logo endpoint returns HTTP 404',
      description: 'ImagesController.Logo returns 404 when Settings.WebSiteLogo is missing or the file is not on disk. Header <img id="logo" src="/images/logo.jpg"> is broken on every storefront page. No fallback default image is served.',
      stepsToReproduce: ['GET http://localhost:81/images/logo.jpg', 'Open any storefront page and inspect #logo'],
      expected: 'JPEG 200 of the configured logo, or a bundled default logo',
      actual: `HTTP ${logo.status()} empty body`,
      device: 'Both',
      suggestedFix: 'If WebSiteLogo setting/file is missing, serve Content/images default logo instead of 404; ensure admin Settings has WebSiteLogo pointing at an existing media file.',
    });
  }

  // Duplicate X-Frame-Options
  const homeHeaders = (await request.get(BASE + '/')).headers();
  const raw = await request.get(BASE + '/');
  // Playwright collapses duplicate headers; curl already showed duplicates.

  // Admin pages
  testedAreas.add('Admin panel');
  const adminPages = [
    '/admin/', '/admin/dashboard/', '/admin/products/', '/admin/productcategories/',
    '/admin/stories/', '/admin/storycategories/', '/admin/menus/', '/admin/brands/',
    '/admin/tags/', '/admin/tagcategories/', '/admin/faq/', '/admin/coupons/',
    '/admin/orders/', '/admin/customers/', '/admin/subscribers/', '/admin/settings/',
    '/admin/users/', '/admin/media/', '/admin/applogs/', '/admin/report/',
    '/admin/metrics/', '/admin/mainpageimages/', '/admin/lists/',
    '/admin/productcomments/', '/admin/shoppingcarts/', '/admin/templates/',
    '/admin/mailtemplates/', '/admin/importdata/', '/admin/fileupload/',
    '/admin/faq/saveoredit', '/admin/products/saveoredit',
  ];
  for (const pth of adminPages) {
    const r = await safeGoto(page, pth);
    if (r.status >= 500 || /Unhandled exception/i.test(r.body) || r.error) {
      addIssue({
        severity: 'Critical',
        area: 'Admin',
        page: pth,
        title: `Admin page 5xx: ${pth}`,
        description: (r.error || r.body || '').slice(0, 300),
        stepsToReproduce: [`Open ${pth} with BypassAdminAuth`],
        expected: '200 admin HTML',
        actual: `HTTP ${r.status} ${r.error || ''}`,
        device: 'Desktop',
        screenshotOrLog: await shot(page, `admin-${pth.replace(/\W+/g, '_')}`),
        suggestedFix: 'Fix the admin action/view null refs.',
      });
    }
  }

  await page.setViewportSize({ width: 390, height: 844 });
  for (const pth of ['/admin/', '/admin/products/', '/admin/orders/', '/admin/customers/', '/admin/media/']) {
    const r = await safeGoto(page, pth);
    const overflow = await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
    if (r.status >= 500 || /Unhandled exception/i.test(r.body)) {
      addIssue({
        severity: 'Critical',
        area: 'Admin',
        page: pth,
        title: `Admin page 5xx on mobile: ${pth}`,
        description: (r.body || '').slice(0, 200),
        stepsToReproduce: ['Viewport 390x844', `Open ${pth}`],
        expected: '200',
        actual: `HTTP ${r.status}`,
        device: 'Mobile',
        suggestedFix: 'Fix the admin action.',
      });
    } else if (overflow > 24) {
      addIssue({
        severity: 'Low',
        area: 'Admin',
        page: pth,
        title: `Admin page horizontal overflow on mobile: ${pth}`,
        description: `overflowX=${overflow}px (jqGrid/tables often exceed 390px).`,
        stepsToReproduce: ['390x844', `Open ${pth}`],
        expected: 'Page shell does not force horizontal scroll (grid may scroll internally)',
        actual: `${overflow}px overflow`,
        device: 'Mobile',
        screenshotOrLog: await shot(page, `admin-m-${pth.replace(/\W+/g, '_')}`),
        suggestedFix: 'Wrap grids in overflow-x:auto; keep admin chrome within viewport.',
      });
    }
  }
  await page.setViewportSize({ width: 1440, height: 900 });

  // Reports
  testedAreas.add('Reports and exports');
  const reports = [
    '/admin/report/couponusage/',
    '/admin/report/fraudanalysis/',
    '/admin/report/paymentmethod/',
    '/admin/report/paymentstatus/',
    '/admin/report/getregionalsalesreport/',
    '/admin/report/salesbydaterange/',
    '/admin/report/shipmentcompany/',
    '/admin/report/performancesystemreport/',
    '/admin/report/financialreport/',
    '/admin/report/productsummary/',
    '/admin/report/priceanalysis/',
    '/admin/report/productinventory/',
  ];
  for (const pth of reports) {
    const r = await safeGoto(page, pth);
    if (r.status >= 500 || /Unhandled exception/i.test(r.body)) {
      addIssue({
        severity: 'High',
        area: 'Reports',
        page: pth,
        title: `Report page 5xx: ${pth}`,
        description: (r.body || '').slice(0, 250),
        stepsToReproduce: [`Open ${pth}`],
        expected: 'Report view or empty-state form',
        actual: `HTTP ${r.status}`,
        device: 'Desktop',
        screenshotOrLog: await shot(page, `report-${pth.replace(/\W+/g, '_')}`),
        suggestedFix: 'Fix ReportController action / view model.',
      });
    }
  }
  const exports = [
    '/admin/faq/exportexcel?format=csv',
    '/admin/brands/exportexcel?format=csv',
    '/admin/orders/exportexcel?format=csv',
    '/admin/customers/exportexcel?format=csv',
    '/admin/applogs/exportexcel?format=csv',
    '/admin/shoppingcarts/exportexcel?format=csv',
    '/admin/report/export?reportKey=CouponUsage&format=csv',
    '/admin/report/export?reportKey=PaymentMethod&format=csv',
    '/admin/report/export?reportKey=ProductInventory&format=csv',
    '/admin/report/export?reportKey=FinancialReport&format=csv&startDate=2026-01-01&endDate=2026-12-31',
  ];
  for (const pth of exports) {
    const res = await request.get(pth, { timeout: 60_000 }).catch(() => null);
    const status = res ? res.status() : 0;
    if (!res || status >= 500) {
      addIssue({
        severity: 'High',
        area: 'Reports',
        page: pth,
        title: `Export 5xx: ${pth}`,
        description: `HTTP ${status}`,
        stepsToReproduce: [`GET ${pth}`],
        expected: 'csv/xlsx 200',
        actual: `HTTP ${status}`,
        device: 'Desktop',
        suggestedFix: 'Fix ExportExcel / Report.Export null DataTable.',
      });
    }
  }

  // AJAX
  testedAreas.add('Admin AJAX');
  for (const pth of ['/ajax/getallcities/', '/payment/getshoppingcartsmalldetails/', '/home/getcompanyname/']) {
    const res = await request.get(pth);
    if (res.status() >= 500) {
      addIssue({
        severity: 'High',
        area: 'Admin',
        page: pth,
        title: `AJAX 5xx: ${pth}`,
        description: `HTTP ${res.status()}`,
        stepsToReproduce: [`GET ${pth}`],
        expected: '200 JSON/HTML',
        actual: `HTTP ${res.status()}`,
        device: 'Both',
        suggestedFix: 'Fix the AJAX action.',
      });
    }
  }

  // Media
  testedAreas.add('File uploads and media');
  await safeGoto(page, '/admin/media/');
  const fileInput = page.locator('input[type="file"]').first();
  if (await fileInput.count()) {
    const tmp = path.join(SHOT_DIR, 'not-an-image.txt');
    fs.writeFileSync(tmp, 'hello');
    await fileInput.setInputFiles(tmp).catch(() => {});
    await page.waitForTimeout(1000);
    const body = await page.locator('body').innerText();
    if (/Unhandled exception/i.test(body)) {
      addIssue({
        severity: 'High',
        area: 'Admin',
        page: '/admin/media/',
        title: 'Non-image upload causes unhandled exception',
        description: body.slice(0, 200),
        stepsToReproduce: ['Open /admin/media/', 'Upload a .txt file'],
        expected: 'Validation error',
        actual: 'Unhandled exception',
        device: 'Desktop',
        suggestedFix: 'Validate MIME/extension before Image.FromStream.',
      });
    }
  }

  // Auth — wrap captcha so ERR_ABORTED does not kill the run
  testedAreas.add('Authentication');
  let loggedIn = false;
  try {
    loggedIn = await loginWithPassword(page, {
      email: CUSTOMER.email,
      password: CUSTOMER.password,
      loginPath: '/account/login/',
    });
  } catch (e) {
    addIssue({
      severity: 'Medium',
      area: 'Auth',
      page: '/account/login/',
      title: 'Legacy captcha login is brittle (navigation aborted during retry)',
      description: String(e && e.message ? e.message : e),
      stepsToReproduce: ['POST /account/login with Legacy captcha', 'Wrong answer redisplays form'],
      expected: 'Stable POST-redirect without aborting the document',
      actual: String(e),
      device: 'Desktop',
      suggestedFix: 'Keep captcha in session across POST; avoid full-page GET of a new captcha mid-submit. For E2E, CaptchaProvider=None.',
    });
  }
  if (loggedIn) {
    for (const pth of ['/customers/', '/customers/index', '/Manage/ChangePassword']) {
      const r = await safeGoto(page, pth);
      if (r.status >= 500 || /Unhandled exception/i.test(r.body)) {
        addIssue({
          severity: 'Critical',
          area: 'Auth',
          page: pth,
          title: `Customer account page 5xx: ${pth}`,
          description: (r.body || '').slice(0, 200),
          stepsToReproduce: ['Log in as customer', `Open ${pth}`],
          expected: '200',
          actual: `HTTP ${r.status}`,
          device: 'Desktop',
          screenshotOrLog: await shot(page, `cust-${pth.replace(/\W+/g, '_')}`),
          suggestedFix: 'Fix Customers/Manage action.',
        });
      } else if (/\/account\/login/i.test(r.url)) {
        addIssue({
          severity: 'High',
          area: 'Auth',
          page: pth,
          title: `Customer page requires re-login: ${pth}`,
          description: r.url,
          stepsToReproduce: ['Log in', `Open ${pth}`],
          expected: 'Authenticated view',
          actual: r.url,
          device: 'Desktop',
          suggestedFix: 'Cookie path / [Authorize] mismatch between Account and Customers.',
        });
      }
    }
    await page.goto('/account/logoff').catch(() => {});
  }

  // Cart / checkout
  testedAreas.add('Cart/checkout/payment');
  const cartPage = await context.newPage();
  try {
    await openProductDetail(cartPage, PRODUCT_DETAIL);
    const addCount = await cartPage.locator('#AddToCart').count();
    if (!addCount) {
      addIssue({
        severity: 'High',
        area: 'Cart',
        page: PRODUCT_DETAIL,
        title: 'Add to cart button missing on expected in-stock product',
        description: `No #AddToCart on ${cartPage.url()}`,
        stepsToReproduce: [`Open ${PRODUCT_DETAIL}`],
        expected: 'Add to cart for in-stock SKU',
        actual: 'Button missing (out of stock or template)',
        device: 'Desktop',
        screenshotOrLog: await shot(cartPage, 'product-no-addtocart'),
        suggestedFix: 'Seed an in-stock product or show disabled state instead of omitting the CTA.',
      });
    } else {
      await addToCartFromDetail(cartPage, { quantity: 1 });
      await goToCart(cartPage);
      const cartText = await cartPage.locator('body').innerText();
      if (/Unhandled exception/i.test(cartText)) {
        addIssue({
          severity: 'Critical',
          area: 'Cart',
          page: '/Payment/ShoppingCart',
          title: 'Cart throws after add-to-cart',
          description: cartText.slice(0, 240),
          stepsToReproduce: ['Add to cart', 'Open shopping cart'],
          expected: 'Line items',
          actual: 'Exception',
          device: 'Desktop',
          screenshotOrLog: await shot(cartPage, 'cart-ex'),
          suggestedFix: 'Fix PaymentController.ShoppingCart.',
        });
      } else if (/sepetinizde ürün bulunamadı|no product found in shopping basket/i.test(cartText)) {
        addIssue({
          severity: 'Critical',
          area: 'Cart',
          page: '/Payment/ShoppingCart',
          title: 'Add to cart does not persist the item',
          description: 'Cart empty after #AddToCart.',
          stepsToReproduce: [`Open ${PRODUCT_DETAIL}`, 'Click AddToCart', 'Open /Payment/ShoppingCart'],
          expected: 'SKU in cart',
          actual: 'Empty cart',
          device: 'Desktop',
          screenshotOrLog: await shot(cartPage, 'cart-empty'),
          suggestedFix: 'Debug AddToCart AJAX and shopping-cart cookie.',
        });
      } else {
        await updateFirstCartQuantity(cartPage, 2);
        await proceedToMembershipCheckout(cartPage);
        if (!/\/account\/register/i.test(cartPage.url())) {
          addIssue({
            severity: 'High',
            area: 'Cart',
            page: cartPage.url(),
            title: 'Guest checkout did not redirect to Register',
            description: cartPage.url(),
            stepsToReproduce: ['Guest cart', 'ProceedToCheckout'],
            expected: '/account/register?returnUrl=...',
            actual: cartPage.url(),
            device: 'Desktop',
            screenshotOrLog: await shot(cartPage, 'checkout-dest'),
            suggestedFix: 'Membership checkout should force Register for anonymous users.',
          });
        } else {
          const email = uniqueCustomerEmail('qa');
          let registered = false;
          try {
            registered = await registerCustomer(cartPage, {
              email,
              password: 'Test123!',
              returnUrl: '/Payment/CheckoutBillingDetails',
            });
          } catch (e) {
            addIssue({
              severity: 'High',
              area: 'Auth',
              page: '/Account/Register',
              title: 'Register during checkout threw',
              description: String(e && e.message ? e.message : e),
              stepsToReproduce: ['Guest checkout', 'Submit register'],
              expected: 'Account created',
              actual: String(e),
              device: 'Desktop',
              screenshotOrLog: await shot(cartPage, 'reg-throw'),
              suggestedFix: 'Harden Register POST + captcha.',
            });
          }
          if (registered) {
            if (!/CheckoutBillingDetails/i.test(cartPage.url())) {
              await cartPage.goto('/Payment/CheckoutBillingDetails', { waitUntil: 'domcontentloaded' });
            }
            if ((await cartPage.locator('#Cities').count()) === 0) {
              addIssue({
                severity: 'Critical',
                area: 'Cart',
                page: '/Payment/CheckoutBillingDetails',
                title: 'Billing page missing city dropdown',
                description: (await cartPage.locator('body').innerText()).slice(0, 200),
                stepsToReproduce: ['Register in checkout', 'Open billing'],
                expected: '#Cities populated',
                actual: cartPage.url(),
                device: 'Desktop',
                screenshotOrLog: await shot(cartPage, 'billing-no-cities'),
                suggestedFix: 'Fix GetAllCities AJAX / billing view.',
              });
            } else {
              await fillBillingDetails(cartPage);
              await placeOrderIfReady(cartPage);
              const payBody = await cartPage.locator('body').innerText();
              if (/Unhandled exception/i.test(payBody)) {
                addIssue({
                  severity: 'Critical',
                  area: 'Payment',
                  page: cartPage.url(),
                  title: 'PlaceOrder unhandled exception',
                  description: payBody.slice(0, 240),
                  stepsToReproduce: ['Complete billing', 'Place order'],
                  expected: 'Iyzico form or friendly error',
                  actual: 'Exception page',
                  device: 'Desktop',
                  screenshotOrLog: await shot(cartPage, 'placeorder-ex'),
                  suggestedFix: 'Guard Iyzico when keys are empty.',
                });
              } else {
                const pay = await tryPayWithIyzicoSandbox(cartPage);
                if (!pay.ok) {
                  addIssue({
                    severity: 'High',
                    area: 'Payment',
                    page: cartPage.url(),
                    title: 'Iyzico payment did not complete',
                    description: pay.reason,
                    stepsToReproduce: ['PlaceOrder', 'Sandbox card 5526080000000006'],
                    expected: 'PaymentResult',
                    actual: pay.reason,
                    device: 'Desktop',
                    screenshotOrLog: await shot(cartPage, 'iyzico-fail'),
                    suggestedFix: 'Set IyzicoApiKey/IyzicoSecretKey sandbox values in IIS; handle empty keys with a blocking admin message.',
                  });
                }
              }
            }
          }
        }
      }
    }
  } catch (e) {
    addIssue({
      severity: 'High',
      area: 'Cart',
      page: cartPage.url(),
      title: 'Cart/checkout flow failed',
      description: String(e && e.message ? e.message : e),
      stepsToReproduce: ['Add to cart as guest and checkout'],
      expected: 'Reach payment',
      actual: String(e),
      device: 'Desktop',
      screenshotOrLog: await shot(cartPage, 'cart-flow'),
      suggestedFix: 'Reproduce the failing step.',
    });
  }
  await cartPage.close();

  // Error pages
  testedAreas.add('Error pages');
  const err404 = await request.get('/this-path-does-not-exist-qa-404/');
  if (err404.status() >= 500) {
    addIssue({
      severity: 'High',
      area: 'Other',
      page: '/this-path-does-not-exist-qa-404/',
      title: 'Unknown URL returns 5xx instead of 404',
      description: `HTTP ${err404.status()}`,
      stepsToReproduce: ['Open a nonsense path'],
      expected: '404 friendly page',
      actual: `HTTP ${err404.status()}`,
      device: 'Both',
      suggestedFix: 'customErrors + ErrorController.NotFound.',
    });
  }

  // Logs
  testedAreas.add('Logs');
  const logDir = 'C:\\inetpub\\wwwroot\\Eimece\\media\\logs';
  const logFiles = fs.existsSync(logDir)
    ? fs.readdirSync(logDir).filter((f) => /\.(log|json)$/i.test(f))
    : [];
  if (logFiles.length === 0) {
    addIssue({
      severity: 'Medium',
      area: 'Other',
      page: 'media/logs',
      title: 'NLog file target produces no log files',
      description: 'media/logs has no EImeceLog.log/json. AppLogs DB table only contains 20 repeating seed rows (Fatal payment timeout, Error payment validation, Warn slow ProductRepository) at exact 1-minute offsets — not real request traces.',
      stepsToReproduce: ['List C:\\inetpub\\wwwroot\\Eimece\\media\\logs', 'Open /admin/applogs/'],
      expected: 'File logs + real ERROR rows from requests',
      actual: 'No files; synthetic AppLogs only',
      device: 'Both',
      suggestedFix: 'Grant IIS_IUSRS write on media/logs; verify NLog ${basedir}; stop seeding fake AppLogs or mark them as demo data.',
    });
  }

  await browser.close();

  // Merge with pass-1 report
  let existing = { summary: {}, issues: [] };
  if (fs.existsSync(REPORT_PATH)) {
    existing = JSON.parse(fs.readFileSync(REPORT_PATH, 'utf8'));
  }
  const skipTitles = new Set([
    'QA script aborted with an uncaught error',
    'Mobile hamburger does not open navigation',
  ]);
  const merged = [...(existing.issues || []).filter((i) => !skipTitles.has(i.title) && !/^Broken images on /i.test(i.title)), ...issues];
  const byKey = new Map();
  for (const issue of merged) {
    const key = `${issue.title}|${issue.page}`;
    if (!byKey.has(key)) byKey.set(key, issue);
  }
  const finalIssues = [...byKey.values()].map((issue, i) => ({
    ...issue,
    id: `BUG-${String(i + 1).padStart(3, '0')}`,
  }));
  const areas = new Set([...(existing.summary.testedAreas || []), ...testedAreas]);
  const report = {
    summary: {
      totalIssues: finalIssues.length,
      critical: finalIssues.filter((i) => i.severity === 'Critical').length,
      high: finalIssues.filter((i) => i.severity === 'High').length,
      medium: finalIssues.filter((i) => i.severity === 'Medium').length,
      low: finalIssues.filter((i) => i.severity === 'Low').length,
      testedAreas: [...areas],
      generatedAt: new Date().toISOString(),
      baseUrl: BASE,
    },
    issues: finalIssues,
  };
  fs.writeFileSync(REPORT_PATH, JSON.stringify(report, null, 2), 'utf8');
  console.log(JSON.stringify(report.summary, null, 2));
})().catch((err) => {
  console.error(err);
  process.exit(1);
});
