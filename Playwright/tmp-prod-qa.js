/**
 * Production-readiness E2E QA for EImece @ http://localhost:81
 * Writes Playwright/e2e-qa-report.json in the required agent-fix format.
 */
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
const {
  loginWithPassword,
  filterConsoleNoise,
} = require('./tests/helpers');
const {
  PRODUCT_CATEGORY,
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
const SHOT_DIR = path.join(__dirname, 'screenshots', 'prod-qa');
const REPORT_PATH = path.join(__dirname, 'e2e-qa-report.json');
const CUSTOMER = { email: 'eminyuce1111@gmail.com', password: 'V02y.qcF' };
const DESKTOP = { width: 1440, height: 900 };
const MOBILE = { width: 390, height: 844 };

fs.mkdirSync(SHOT_DIR, { recursive: true });

const issues = [];
const testedAreas = new Set();
let bugSeq = 1;

function addIssue(partial) {
  const id = `BUG-${String(bugSeq++).padStart(3, '0')}`;
  issues.push({ id, ...partial });
  return id;
}

function classifyBody(status, body, url) {
  const text = (body || '').slice(0, 4000);
  const unhandled = /Unhandled exception|Server Error in|yellow-screen|Stack Trace:/i.test(text);
  const friendly404 = /sayfa bulunamadı|page not found|not found/i.test(text);
  if (status >= 500 || unhandled) return { kind: '5xx', unhandled, snippet: text.slice(0, 240) };
  if (status === 404) return { kind: '404', unhandled, friendly404, snippet: text.slice(0, 240) };
  return { kind: 'ok', unhandled, snippet: '' };
}

async function shot(page, name) {
  const file = path.join(SHOT_DIR, `${name}.png`);
  await page.screenshot({ path: file, fullPage: false }).catch(() => {});
  return `Playwright/screenshots/prod-qa/${name}.png`;
}

async function pageDiagnostics(page) {
  return page.evaluate(() => {
    const brokenImgs = [...document.querySelectorAll('img')]
      .filter((img) => img.offsetParent !== null && img.naturalWidth === 0 && img.src)
      .map((img) => img.src)
      .slice(0, 12);
    const doc = document.documentElement;
    return {
      title: document.title,
      design: document.body.getAttribute('data-design'),
      overflowX: doc.scrollWidth - doc.clientWidth,
      brokenImgs,
      hasMain: !!document.querySelector('main, #main-content, .admin-layout, .content-wrapper'),
      bodyLen: (document.body && document.body.innerText || '').length,
    };
  });
}

async function httpProbe(request, url) {
  const res = await request.get(url, { maxRedirects: 0, timeout: 30_000 }).catch((e) => ({
    status: () => 0,
    headers: () => ({}),
    text: async () => String(e),
    url: () => url,
  }));
  const status = typeof res.status === 'function' ? res.status() : 0;
  const loc = res.headers ? res.headers()['location'] : undefined;
  let body = '';
  try {
    body = await res.text();
  } catch (_) {
    body = '';
  }
  return { status, location: loc, body, finalUrl: typeof res.url === 'function' ? res.url() : url };
}

async function visit(page, url, { viewport = 'desktop', expectCrizal = false } = {}) {
  const consoleErrors = [];
  const pageErrors = [];
  const failed = [];
  const onConsole = (msg) => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  };
  const onPageError = (err) => pageErrors.push(String(err));
  const onResponse = (res) => {
    if (res.status() >= 400) failed.push({ status: res.status(), url: res.url() });
  };
  page.on('console', onConsole);
  page.on('pageerror', onPageError);
  page.on('response', onResponse);
  let status = 0;
  try {
    const res = await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 45_000 });
    status = res ? res.status() : 0;
  } catch (e) {
    page.off('console', onConsole);
    page.off('pageerror', onPageError);
    page.off('response', onResponse);
    return { status: 0, error: String(e), consoleErrors, pageErrors, failed, url: page.url() };
  }
  await page.waitForTimeout(400);
  const body = await page.locator('body').innerText().catch(() => '');
  const diag = await pageDiagnostics(page).catch(() => ({}));
  page.off('console', onConsole);
  page.off('pageerror', onPageError);
  page.off('response', onResponse);
  return {
    status,
    url: page.url(),
    body,
    diag,
    consoleErrors: filterConsoleNoise(consoleErrors.concat(pageErrors)),
    failed: failed.filter((f) => !/favicon|apple-touch|gtag|analytics/i.test(f.url)),
    viewport,
    expectCrizal,
  };
}

async function run() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: DESKTOP,
    ignoreHTTPSErrors: true,
    baseURL: BASE,
  });
  const page = await context.newPage();
  const request = context.request;

  // ---------- 1. Health / static ----------
  testedAreas.add('Health/static');
  for (const p of ['/health', '/healthz', '/robots.txt', '/sitemap.xml']) {
    const r = await httpProbe(request, BASE + p);
    if (r.status >= 500 || r.status === 0) {
      addIssue({
        severity: 'Critical',
        area: 'Other',
        page: p,
        title: `${p} returns HTTP ${r.status}`,
        description: `Core operational endpoint ${p} failed.`,
        stepsToReproduce: [`GET ${BASE}${p}`],
        expected: 'HTTP 200',
        actual: `HTTP ${r.status}`,
        device: 'Both',
        suggestedFix: 'Investigate IIS / routing for this endpoint.',
      });
    }
  }

  // ---------- 2. Sitemap ----------
  testedAreas.add('Sitemap/SEO');
  const smRes = await request.get(BASE + '/sitemap.xml');
  const smXml = await smRes.text();
  const locs = [...smXml.matchAll(/<loc>([^<]+)<\/loc>/g)].map((m) => m[1].trim());
  const sitemapStats = { total: locs.length, status: {}, samples: { 404: [], '5xx': [], 200: 0, redirect: 0 } };
  const uniqueHosts = new Set(locs.map((u) => {
    try { return new URL(u).host; } catch { return u; }
  }));

  if (locs.length === 0) {
    addIssue({
      severity: 'High',
      area: 'Frontend',
      page: '/sitemap.xml',
      title: 'Sitemap is empty',
      description: 'sitemap.xml returned no <loc> entries.',
      stepsToReproduce: ['Open /sitemap.xml'],
      expected: 'Product, category, story, and page URLs',
      actual: 'No loc entries',
      device: 'Both',
      suggestedFix: 'Fix SiteMapService generation / cache.',
    });
  }

  const conventionalCategory = locs.filter((u) => /\/productcategories\/category\//i.test(u));
  const canonicalCategory = locs.filter((u) => /\/c\/pc\//i.test(u));
  const conventionalStoryCat = locs.filter((u) => /\/stories\/categories\//i.test(u));
  const canonicalStoryCat = locs.filter((u) => /\/s\/sc\//i.test(u));

  if (conventionalCategory.length > 0 && canonicalCategory.length === 0) {
    addIssue({
      severity: 'High',
      area: 'Frontend',
      page: '/sitemap.xml',
      title: 'Sitemap emits non-canonical product category URLs',
      description: `All ${conventionalCategory.length} product-category sitemap URLs use /productcategories/category/{slug} instead of canonical /c/pc/{slug}/. SiteMapService.GetDetailPageUrl falls back to conventional MVC paths when HttpContext is null after ConfigureAwait(false). Search engines will index dead or duplicate URLs.`,
      stepsToReproduce: ['GET /sitemap.xml', 'Inspect <loc> for product categories'],
      expected: 'Canonical URLs like http://localhost:81/c/pc/elektronik-.../',
      actual: conventionalCategory.slice(0, 3).join(', '),
      device: 'Both',
      screenshotOrLog: conventionalCategory[0],
      suggestedFix: 'In EntityExtension.BuildDetailRelativePathWithoutHttpContext, map ProductCategories/Category → /c/pc/{seoId} and Stories/Categories → /s/sc/{seoId}. Prefer Url.Action with attribute routes when HttpContext is available.',
    });
  }

  if (conventionalStoryCat.length > 0 && canonicalStoryCat.length === 0) {
    addIssue({
      severity: 'High',
      area: 'Frontend',
      page: '/sitemap.xml',
      title: 'Sitemap emits non-canonical story category URLs',
      description: `Story category sitemap URLs use /stories/categories/{slug} instead of /s/sc/{slug}/.`,
      stepsToReproduce: ['GET /sitemap.xml', 'Inspect story category loc entries'],
      expected: '/s/sc/{seo-hash}/',
      actual: conventionalStoryCat.slice(0, 3).join(', '),
      device: 'Both',
      suggestedFix: 'Add Stories/Categories mapping in BuildDetailRelativePathWithoutHttpContext to /s/sc/{seoId}.',
    });
  }

  // Probe every sitemap URL (HEAD/GET, no browser) — classify real 404/5xx
  for (const loc of locs) {
    const r = await httpProbe(request, loc);
    const bucket = r.status >= 500 ? '5xx' : r.status === 404 ? '404' : r.status >= 300 && r.status < 400 ? 'redirect' : String(r.status);
    sitemapStats.status[bucket] = (sitemapStats.status[bucket] || 0) + 1;
    if (r.status >= 300 && r.status < 400) sitemapStats.samples.redirect++;
    if (r.status >= 200 && r.status < 300) sitemapStats.samples[200]++;
    if (r.status === 404 && sitemapStats.samples[404].length < 8) sitemapStats.samples[404].push(loc);
    if (r.status >= 500 && sitemapStats.samples['5xx'].length < 8) sitemapStats.samples['5xx'].push({ loc, status: r.status });
  }

  if ((sitemapStats.status['404'] || 0) > 0) {
    addIssue({
      severity: 'High',
      area: 'Frontend',
      page: '/sitemap.xml',
      title: `${sitemapStats.status['404']} sitemap URLs return HTTP 404`,
      description: `Sitemap advertises URLs that 404. Sample: ${sitemapStats.samples[404].join(' | ')}. This is typically the conventional /productcategories/category/ and /stories/categories/ paths generated without HttpContext.`,
      stepsToReproduce: ['Open a <loc> from sitemap.xml in the browser'],
      expected: '200 on canonical SEO URL, or sitemap should only list live canonical URLs',
      actual: `HTTP 404 on ${sitemapStats.status['404']} of ${locs.length} sitemap URLs`,
      device: 'Both',
      screenshotOrLog: JSON.stringify(sitemapStats.samples[404]),
      suggestedFix: 'Generate canonical /c/pc, /p, /s/sc, /s URLs in SiteMapService; add 301 from conventional MVC paths if they must remain.',
    });
  }

  if ((sitemapStats.status['5xx'] || 0) > 0) {
    addIssue({
      severity: 'Critical',
      area: 'Frontend',
      page: '/sitemap.xml',
      title: `${sitemapStats.status['5xx']} sitemap URLs return HTTP 5xx`,
      description: JSON.stringify(sitemapStats.samples['5xx']),
      stepsToReproduce: ['GET each failing sitemap loc'],
      expected: 'HTTP 200 or 404',
      actual: 'Server error',
      device: 'Both',
      suggestedFix: 'Fix the controller action throwing for those ids; null-check view models.',
    });
  }

  // Compare conventional vs canonical for first category
  if (conventionalCategory[0]) {
    const conv = await httpProbe(request, conventionalCategory[0]);
    const slug = conventionalCategory[0].split('/').filter(Boolean).pop();
    const canonUrl = `${BASE}/c/pc/${slug}/`;
    const canon = await httpProbe(request, canonUrl);
    if (conv.status === 404 && canon.status >= 200 && canon.status < 400) {
      // already covered by sitemap 404 issue; add redirect-missing as medium if not already
      addIssue({
        severity: 'Medium',
        area: 'Frontend',
        page: conventionalCategory[0],
        title: 'Legacy MVC category URL 404s instead of 301 to /c/pc/',
        description: `${conventionalCategory[0]} returns ${conv.status} while canonical ${canonUrl} returns ${canon.status}. ProductCategoriesController.Category is attribute-routed at /c/pc/{id}; conventional /productcategories/category/{id} is not redirected.`,
        stepsToReproduce: [`Open ${conventionalCategory[0]}`, `Open ${canonUrl}`],
        expected: '301 to canonical /c/pc/{slug}/',
        actual: `HTTP ${conv.status}`,
        device: 'Both',
        suggestedFix: 'Add a conventional-route or legacy action that 301s /productcategories/category/{id} → /c/pc/{id}/ (same pattern as CategoryLegacy).',
      });
    }
  }

  // ---------- 3. Storefront key pages (desktop + mobile) ----------
  testedAreas.add('Frontend/Customer pages');
  const storefront = [
    { name: 'home', path: '/' },
    { name: 'products-index', path: '/products/' },
    { name: 'stories-index', path: '/stories/' },
    { name: 'search', path: '/p/arama/?search=test' },
    { name: 'advanced-search', path: '/p/advancedsearchproducts/' },
    { name: 'cart', path: '/payment/shoppingcart/' },
    { name: 'login', path: '/account/login/' },
    { name: 'register', path: '/account/register/' },
    { name: 'admin-login', path: '/account/adminlogin/' },
    { name: 'forgot', path: '/account/forgotpassword/' },
    { name: 'about', path: '/info/aboutus/' },
    { name: 'privacy', path: '/info/privacypolicy/' },
    { name: 'terms', path: '/info/termsandconditions/' },
    { name: 'delivery', path: '/info/deliveryinfo/' },
    { name: 'contact', path: '/i/iletisim-3f4h8c6g/' },
    { name: 'faq', path: '/i/sikca-sorulan-sorular-4h4h8c6g/' },
    { name: 'category', path: PRODUCT_CATEGORY },
    { name: 'product', path: PRODUCT_DETAIL },
    { name: 'story-cat-stale', path: '/s/sc/stil-rehberi-8c6g/' },
    { name: 'legacy-cat', path: '/c/Ev-Yasam' },
    { name: 'pc-root', path: '/c/pc/' },
    { name: 'error-index', path: '/error/index/' },
    { name: 'error-404', path: '/error/notfound/' },
    { name: 'guest-checkout', path: '/payment/shoppingwithoutaccount/' },
    { name: 'cargo', path: '/payment/cargotracking/' },
  ];

  for (const vp of [
    { name: 'desktop', size: DESKTOP },
    { name: 'mobile', size: MOBILE },
  ]) {
    await page.setViewportSize(vp.size);
    for (const route of storefront) {
      const r = await visit(page, BASE + route.path, { viewport: vp.name, expectCrizal: true });
      const cls = classifyBody(r.status, r.body, r.url);
      const shotName = `${vp.name}-${route.name}`;
      if (cls.kind === '5xx' || cls.unhandled) {
        const ref = await shot(page, shotName);
        addIssue({
          severity: 'Critical',
          area: 'Frontend',
          page: route.path,
          title: `${route.name} throws server error (${vp.name})`,
          description: cls.snippet || r.error || `HTTP ${r.status}`,
          stepsToReproduce: [`Set viewport ${vp.size.width}x${vp.size.height}`, `Open ${route.path}`],
          expected: 'Page renders without unhandled exception',
          actual: `HTTP ${r.status}; ${cls.snippet}`,
          device: vp.name === 'mobile' ? 'Mobile' : 'Desktop',
          screenshotOrLog: ref,
          suggestedFix: 'Null-check the view model; return 404 for missing entities instead of 500.',
        });
      }
      if (r.diag && r.diag.overflowX > 8) {
        const ref = await shot(page, `${shotName}-overflow`);
        addIssue({
          severity: 'Medium',
          area: 'Frontend',
          page: route.path,
          title: `Horizontal overflow on ${route.name} (${vp.name})`,
          description: `document.scrollWidth exceeds clientWidth by ${r.diag.overflowX}px.`,
          stepsToReproduce: [`Viewport ${vp.size.width}x${vp.size.height}`, `Open ${route.path}`],
          expected: 'No horizontal scrollbar',
          actual: `overflowX=${r.diag.overflowX}px`,
          device: vp.name === 'mobile' ? 'Mobile' : 'Desktop',
          screenshotOrLog: ref,
          suggestedFix: 'Find overflowing child (wide table, image, or absolute nav) and constrain with max-width:100% / overflow-x:auto.',
        });
      }
      if (r.diag && r.diag.brokenImgs && r.diag.brokenImgs.length) {
        addIssue({
          severity: 'Medium',
          area: 'Frontend',
          page: route.path,
          title: `Broken images on ${route.name} (${vp.name})`,
          description: r.diag.brokenImgs.slice(0, 5).join(' | '),
          stepsToReproduce: [`Open ${route.path}`],
          expected: 'All visible images load',
          actual: `${r.diag.brokenImgs.length} broken <img>`,
          device: vp.name === 'mobile' ? 'Mobile' : 'Desktop',
          suggestedFix: 'Fix image src / ImagesController resize proxy / missing media files.',
        });
      }
      const jsReal = (r.consoleErrors || []).filter((e) => !/Failed to load resource/i.test(e));
      if (jsReal.length) {
        addIssue({
          severity: 'Medium',
          area: 'Frontend',
          page: route.path,
          title: `JavaScript errors on ${route.name} (${vp.name})`,
          description: jsReal.slice(0, 5).join(' || '),
          stepsToReproduce: [`Open ${route.path}`, 'Inspect browser console'],
          expected: 'No application JS exceptions',
          actual: jsReal[0],
          device: vp.name === 'mobile' ? 'Mobile' : 'Desktop',
          suggestedFix: 'Fix the referenced script; guard null DOM nodes.',
        });
      }
      const net500 = (r.failed || []).filter((f) => f.status >= 500);
      if (net500.length) {
        addIssue({
          severity: 'High',
          area: 'Frontend',
          page: route.path,
          title: `Subresource 5xx on ${route.name}`,
          description: net500.slice(0, 5).map((f) => `${f.status} ${f.url}`).join(' | '),
          stepsToReproduce: [`Open ${route.path}`],
          expected: 'CSS/JS/AJAX return 2xx',
          actual: net500[0].url,
          device: vp.name === 'mobile' ? 'Mobile' : 'Desktop',
          suggestedFix: 'Fix the failing endpoint or bundle.',
        });
      }
    }
  }

  await page.setViewportSize(DESKTOP);

  // Stale story category should be 404 not 500
  const staleStory = await httpProbe(request, BASE + '/s/sc/stil-rehberi-8c6g/?nocache=1');
  if (staleStory.status >= 500) {
    addIssue({
      severity: 'Critical',
      area: 'Frontend',
      page: '/s/sc/stil-rehberi-8c6g/',
      title: 'Unknown story-category SEO hash returns 500',
      description: 'Hash 8c6g does not match a story category; controller should 404.',
      stepsToReproduce: ['Open /s/sc/stil-rehberi-8c6g/?nocache=1'],
      expected: 'HTTP 404',
      actual: `HTTP ${staleStory.status}`,
      device: 'Both',
      suggestedFix: 'StoriesController.Categories: if GetStoryCategoriesViewModel is null, return HttpNotFound / Error/NotFound.',
    });
  }

  // Homepage nav links
  testedAreas.add('Navigation');
  await page.goto(BASE + '/', { waitUntil: 'domcontentloaded' });
  const navHrefs = await page.evaluate(() =>
    [...document.querySelectorAll('header a[href], .navbar a[href], footer a[href]')]
      .map((a) => a.href)
      .filter((h) => h && h.startsWith(location.origin) && !h.includes('javascript:'))
  );
  const uniqueNav = [...new Set(navHrefs)].slice(0, 40);
  for (const href of uniqueNav) {
    const r = await httpProbe(request, href);
    if (r.status >= 500) {
      addIssue({
        severity: 'Critical',
        area: 'Frontend',
        page: href,
        title: 'Header/footer link returns 5xx',
        description: `Nav link ${href} → ${r.status}`,
        stepsToReproduce: ['Open home', `Click or GET ${href}`],
        expected: '200',
        actual: `HTTP ${r.status}`,
        device: 'Both',
        suggestedFix: 'Fix the target action or remove the dead menu item.',
      });
    }
  }

  // Mobile menu
  testedAreas.add('Mobile navigation');
  await page.setViewportSize(MOBILE);
  await page.goto(BASE + '/', { waitUntil: 'domcontentloaded' });
  const burger = page.locator('.navbar-toggle, .navbar-toggler, button.navbar-toggle, #mobile-menu-toggle, .crizal-nav-toggle, .menu-toggle').first();
  if (await burger.count()) {
    await burger.click({ force: true }).catch(() => {});
    await page.waitForTimeout(500);
    const menuVisible = await page.locator('.navbar-collapse.in, .navbar-collapse.show, .navbar-collapse[style*="display: block"], nav.mobile-open, .crizal-mobile-menu').first().isVisible().catch(() => false);
    const anyNavLink = await page.locator('.navbar-collapse a, .mobile-menu a').first().isVisible().catch(() => false);
    if (!menuVisible && !anyNavLink) {
      const ref = await shot(page, 'mobile-menu-closed');
      addIssue({
        severity: 'High',
        area: 'Frontend',
        page: '/',
        title: 'Mobile hamburger does not open navigation',
        description: 'Clicked navbar toggle; collapse menu did not become visible.',
        stepsToReproduce: ['Viewport 390x844', 'Open /', 'Tap hamburger'],
        expected: 'Mobile nav links visible',
        actual: 'Menu stayed closed',
        device: 'Mobile',
        screenshotOrLog: ref,
        suggestedFix: 'Ensure Bootstrap collapse JS is bundled and toggle data-target matches the collapse id.',
      });
    }
  }
  await page.setViewportSize(DESKTOP);

  // ---------- 4. Auth ----------
  testedAreas.add('Authentication');
  const customerLogin = await loginWithPassword(page, {
    email: CUSTOMER.email,
    password: CUSTOMER.password,
    loginPath: '/account/login/',
  });
  if (!customerLogin) {
    const ref = await shot(page, 'customer-login-fail');
    addIssue({
      severity: 'High',
      area: 'Auth',
      page: '/account/login/',
      title: 'Customer login failed with valid credentials (legacy captcha)',
      description: `Could not leave /account/login after filling ${CUSTOMER.email} and brute-forcing captcha 2..8. Final URL=${page.url()}`,
      stepsToReproduce: ['Open /account/login/', `Email ${CUSTOMER.email}`, 'Submit with captcha'],
      expected: 'Redirect to account / home',
      actual: page.url(),
      device: 'Desktop',
      screenshotOrLog: ref,
      suggestedFix: 'Verify captcha session survives POST; ensure login submit button is the form submit (not hidden #searchSubmitButton); consider CaptchaProvider=None for local E2E.',
    });
  } else {
    const accountPages = [
      '/customers/',
      '/customers/index',
      '/manage/',
      '/Manage/ChangePassword',
      '/Manage/ManageLogins',
    ];
    for (const pth of accountPages) {
      const r = await visit(page, BASE + pth);
      if (r.status >= 500 || /Unhandled exception/i.test(r.body || '')) {
        const ref = await shot(page, `account-${pth.replace(/\W+/g, '_')}`);
        addIssue({
          severity: 'Critical',
          area: 'Auth',
          page: pth,
          title: `Authenticated customer page 5xx: ${pth}`,
          description: (r.body || '').slice(0, 240),
          stepsToReproduce: ['Log in as customer', `Open ${pth}`],
          expected: '200',
          actual: `HTTP ${r.status}`,
          device: 'Desktop',
          screenshotOrLog: ref,
          suggestedFix: 'Fix the Customers/Manage action; add null checks.',
        });
      } else if (/\/account\/login/i.test(r.url)) {
        addIssue({
          severity: 'High',
          area: 'Auth',
          page: pth,
          title: `Customer page redirected to login: ${pth}`,
          description: 'Session present but page requires auth and bounced to login.',
          stepsToReproduce: ['Log in', `Open ${pth}`],
          expected: 'Stay authenticated',
          actual: r.url,
          device: 'Desktop',
          suggestedFix: 'Align [Authorize] and cookie auth for Customers vs Manage.',
        });
      }
    }
    await page.goto(BASE + '/account/logoff', { waitUntil: 'domcontentloaded' }).catch(() => {});
  }

  // Admin login page still reachable; bypass should land on dashboard for /admin
  const adminRoot = await visit(page, BASE + '/admin/');
  if (/\/account\/adminlogin/i.test(adminRoot.url) && !/TEMP: Admin auth bypass ON/i.test(adminRoot.body || '')) {
    addIssue({
      severity: 'Medium',
      area: 'Admin',
      page: '/admin/',
      title: 'Admin root redirected to login despite BypassAdminAuth',
      description: `IIS BypassAdminAuth=true and SiteStatus=dev, but /admin/ went to ${adminRoot.url}`,
      stepsToReproduce: ['Open /admin/'],
      expected: 'Dashboard (bypass) or login form',
      actual: adminRoot.url,
      device: 'Desktop',
      suggestedFix: 'Confirm AppConfig.BypassAdminAuth reads the IIS web.config key and SiteStatus is not live.',
    });
  }

  // ---------- 5. Admin pages ----------
  testedAreas.add('Admin panel');
  const adminPages = [
    '/admin/',
    '/admin/dashboard/',
    '/admin/products/',
    '/admin/productcategories/',
    '/admin/stories/',
    '/admin/storycategories/',
    '/admin/menus/',
    '/admin/brands/',
    '/admin/tags/',
    '/admin/tagcategories/',
    '/admin/faq/',
    '/admin/coupons/',
    '/admin/orders/',
    '/admin/customers/',
    '/admin/subscribers/',
    '/admin/settings/',
    '/admin/users/',
    '/admin/media/',
    '/admin/applogs/',
    '/admin/report/',
    '/admin/metrics/',
    '/admin/mainpageimages/',
    '/admin/lists/',
    '/admin/productcomments/',
    '/admin/shoppingcarts/',
    '/admin/templates/',
    '/admin/mailtemplates/',
    '/admin/importdata/',
    '/admin/fileupload/',
    '/admin/adminsettings/',
  ];
  const reportPages = [
    '/admin/report/couponusage/',
    '/admin/report/fraudanalysis/',
    '/admin/report/paymentmethod/',
    '/admin/report/paymentstatus/',
    '/admin/report/getregionalsalesreport/',
    '/admin/report/salesbydaterange/',
    '/admin/report/shipmentcompany/',
    '/admin/report/performancesystemreport/',
    '/admin/report/financialreport/',
    '/admin/report/fraudriskreport/',
    '/admin/report/ordervolumereport/',
    '/admin/report/paymenttransactionreport/',
    '/admin/report/productsummary/',
    '/admin/report/priceanalysis/',
    '/admin/report/productinventory/',
    '/admin/report/productstatsbydaterange/',
  ];

  for (const vp of [
    { name: 'desktop', size: DESKTOP },
    { name: 'mobile', size: MOBILE },
  ]) {
    await page.setViewportSize(vp.size);
    const list = vp.name === 'desktop' ? adminPages.concat(reportPages) : adminPages.slice(0, 12);
    for (const pth of list) {
      const r = await visit(page, BASE + pth, { viewport: vp.name });
      if (r.status >= 500 || /Unhandled exception/i.test(r.body || '')) {
        const ref = await shot(page, `admin-${vp.name}-${pth.replace(/\W+/g, '_')}`);
        addIssue({
          severity: 'Critical',
          area: 'Admin',
          page: pth,
          title: `Admin page 5xx: ${pth} (${vp.name})`,
          description: (r.body || r.error || '').slice(0, 300),
          stepsToReproduce: [`Open ${pth} at ${vp.size.width}px`],
          expected: '200 HTML admin view',
          actual: `HTTP ${r.status}`,
          device: vp.name === 'mobile' ? 'Mobile' : 'Desktop',
          screenshotOrLog: ref,
          suggestedFix: 'Fix the admin action / view; check null models and missing partials.',
        });
      }
      if (r.diag && r.diag.overflowX > 20 && vp.name === 'mobile') {
        const ref = await shot(page, `admin-overflow-${pth.replace(/\W+/g, '_')}`);
        addIssue({
          severity: 'Low',
          area: 'Admin',
          page: pth,
          title: `Admin grid overflows on mobile: ${pth}`,
          description: `overflowX=${r.diag.overflowX}px`,
          stepsToReproduce: ['Viewport 390x844', `Open ${pth}`],
          expected: 'Grid scrolls inside container, page does not overflow',
          actual: `Page scrollWidth exceeds by ${r.diag.overflowX}px`,
          device: 'Mobile',
          screenshotOrLog: ref,
          suggestedFix: 'Wrap jqGrid/table in overflow-x:auto; avoid fixed min-widths on the page shell.',
        });
      }
    }
  }
  await page.setViewportSize(DESKTOP);

  // ---------- 6. Reports / exports ----------
  testedAreas.add('Reports and exports');
  const exports = [
    '/admin/faq/exportexcel?format=csv',
    '/admin/brands/exportexcel?format=csv',
    '/admin/tags/exportexcel?format=csv',
    '/admin/orders/exportexcel?format=csv',
    '/admin/customers/exportexcel?format=csv',
    '/admin/applogs/exportexcel?format=csv',
    '/admin/shoppingcarts/exportexcel?format=csv',
    '/admin/report/export?reportKey=CouponUsage&format=csv',
    '/admin/report/export?reportKey=PaymentMethod&format=csv',
    '/admin/report/export?reportKey=ProductInventory&format=csv',
  ];
  for (const pth of exports) {
    const res = await request.get(BASE + pth, { timeout: 60_000 }).catch((e) => null);
    const status = res ? res.status() : 0;
    const ct = res ? (res.headers()['content-type'] || '') : '';
    if (!res || status >= 500) {
      addIssue({
        severity: 'High',
        area: 'Reports',
        page: pth,
        title: `Export failed: ${pth}`,
        description: `HTTP ${status}`,
        stepsToReproduce: [`GET ${pth} while admin-authenticated (bypass)`],
        expected: '200 with csv/xlsx download',
        actual: `HTTP ${status} ${ct}`,
        device: 'Desktop',
        suggestedFix: 'Fix ExportExcel/Report.Export; handle empty DataTable; do not throw on null reportKey filters.',
      });
    } else if (status === 400) {
      addIssue({
        severity: 'Medium',
        area: 'Reports',
        page: pth,
        title: `Export returned 400: ${pth}`,
        description: await res.text().then((t) => t.slice(0, 200)).catch(() => ''),
        stepsToReproduce: [`GET ${pth}`],
        expected: 'File download',
        actual: 'HTTP 400',
        device: 'Desktop',
        suggestedFix: 'Pass required filter query-string fields (dates, reportKey).',
      });
    }
  }

  // ---------- 7. AJAX admin ----------
  testedAreas.add('Admin AJAX');
  const ajaxGets = [
    '/ajax/getallcities/',
    '/payment/getshoppingcartsmalldetails/',
    '/payment/getshoppingcartlinks/',
    '/home/getcompanyname/',
    '/admin/ajax/getproductcategories?term=a',
  ];
  for (const pth of ajaxGets) {
    const r = await httpProbe(request, BASE + pth);
    if (r.status >= 500) {
      addIssue({
        severity: 'High',
        area: 'Admin',
        page: pth,
        title: `AJAX endpoint 5xx: ${pth}`,
        description: r.body.slice(0, 200),
        stepsToReproduce: [`GET ${pth}`],
        expected: 'JSON 200',
        actual: `HTTP ${r.status}`,
        device: 'Both',
        suggestedFix: 'Fix the AjaxController action.',
      });
    }
  }

  // Grid search AJAX: type into products search if present
  await page.goto(BASE + '/admin/products/', { waitUntil: 'domcontentloaded' });
  const searchBox = page.locator('input[type="search"], input[name="search"], #gs_Name, .ui-search-input input').first();
  if (await searchBox.count()) {
    const respPromise = page.waitForResponse((r) => /grid|jqgrid|products/i.test(r.url()), { timeout: 15_000 }).catch(() => null);
    await searchBox.fill('test');
    await searchBox.press('Enter');
    const resp = await respPromise;
    if (resp && resp.status() >= 500) {
      addIssue({
        severity: 'High',
        area: 'Admin',
        page: '/admin/products/',
        title: 'Products grid search AJAX returns 5xx',
        description: resp.url(),
        stepsToReproduce: ['Open /admin/products/', 'Type test in grid search', 'Enter'],
        expected: 'JSON rows',
        actual: `HTTP ${resp.status()}`,
        device: 'Desktop',
        suggestedFix: 'Fix the jqGrid data URL action.',
      });
    }
  }

  // ---------- 8. Media / uploads ----------
  testedAreas.add('File uploads and media');
  const media = await visit(page, BASE + '/admin/media/');
  if (media.status >= 500) {
    addIssue({
      severity: 'Critical',
      area: 'Admin',
      page: '/admin/media/',
      title: 'Media library 5xx',
      description: (media.body || '').slice(0, 200),
      stepsToReproduce: ['Open /admin/media/'],
      expected: 'Media browser',
      actual: `HTTP ${media.status}`,
      device: 'Desktop',
      suggestedFix: 'Fix MediaController.Index; ensure media folder ACLs.',
    });
  }
  const uploadInput = page.locator('input[type="file"]').first();
  if (await uploadInput.count()) {
    const tmpImg = path.join(SHOT_DIR, 'upload-probe.txt');
    fs.writeFileSync(tmpImg, 'not-an-image');
    await uploadInput.setInputFiles(tmpImg).catch(() => {});
    await page.waitForTimeout(800);
    const body = await page.locator('body').innerText();
    if (/Unhandled exception/i.test(body)) {
      addIssue({
        severity: 'High',
        area: 'Admin',
        page: '/admin/media/',
        title: 'Invalid file upload causes unhandled exception',
        description: 'Uploading a .txt via the media file input crashed the page.',
        stepsToReproduce: ['Open /admin/media/', 'Upload a non-image file'],
        expected: 'Validation error',
        actual: 'Unhandled exception',
        device: 'Desktop',
        suggestedFix: 'Validate content type/extension before Image.FromStream; return ModelState error.',
      });
    }
  }

  // ---------- 9. Cart / checkout / payment ----------
  testedAreas.add('Cart/checkout/payment');
  await context.clearCookies();
  const cartPage = await context.newPage();
  await cartPage.setViewportSize(DESKTOP);
  try {
    await openProductDetail(cartPage, PRODUCT_DETAIL);
    await addToCartFromDetail(cartPage, { quantity: 1 });
    await goToCart(cartPage);
    const cartText = await cartPage.locator('body').innerText();
    if (/Unhandled exception/i.test(cartText)) {
      const ref = await shot(cartPage, 'cart-exception');
      addIssue({
        severity: 'Critical',
        area: 'Cart',
        page: '/Payment/ShoppingCart',
        title: 'Shopping cart unhandled exception after add-to-cart',
        description: cartText.slice(0, 240),
        stepsToReproduce: ['Open in-stock product', 'Add to cart', 'Open /Payment/ShoppingCart'],
        expected: 'Cart lists the SKU',
        actual: 'Exception page',
        device: 'Desktop',
        screenshotOrLog: ref,
        suggestedFix: 'Fix PaymentController.ShoppingCart null refs.',
      });
    } else if (/sepetinizde ürün bulunamadı|no product found in shopping basket/i.test(cartText)) {
      const ref = await shot(cartPage, 'cart-empty-after-add');
      addIssue({
        severity: 'Critical',
        area: 'Cart',
        page: '/Payment/ShoppingCart',
        title: 'Add to cart does not persist items',
        description: 'After #AddToCart, shopping cart shows empty.',
        stepsToReproduce: [`Open ${PRODUCT_DETAIL}`, 'Click AddToCart', 'Open /Payment/ShoppingCart'],
        expected: 'Line item present',
        actual: 'Empty cart message',
        device: 'Desktop',
        screenshotOrLog: ref,
        suggestedFix: 'Check AddToCart AJAX, cookie/session shopping cart, and CORS/path of cart cookie.',
      });
    } else {
      await updateFirstCartQuantity(cartPage, 2);
      await proceedToMembershipCheckout(cartPage);
      if (!/\/account\/register/i.test(cartPage.url())) {
        const ref = await shot(cartPage, 'checkout-not-register');
        addIssue({
          severity: 'High',
          area: 'Cart',
          page: cartPage.url(),
          title: 'Guest membership checkout did not force Register',
          description: `Expected /account/register?returnUrl=... got ${cartPage.url()}`,
          stepsToReproduce: ['Add to cart as guest', 'Click ProceedToCheckout'],
          expected: 'Register with returnUrl to billing',
          actual: cartPage.url(),
          device: 'Desktop',
          screenshotOrLog: ref,
          suggestedFix: 'PaymentController membership checkout should redirect guests to Register, not Login.',
        });
      } else {
        const email = uniqueCustomerEmail('qa');
        const registered = await registerCustomer(cartPage, {
          email,
          password: 'Test123!',
          returnUrl: '/Payment/CheckoutBillingDetails',
        });
        if (!registered) {
          const ref = await shot(cartPage, 'register-fail');
          addIssue({
            severity: 'High',
            area: 'Auth',
            page: '/Account/Register',
            title: 'Checkout registration failed',
            description: `Could not register ${email}. URL=${cartPage.url()}`,
            stepsToReproduce: ['Guest checkout', 'Fill register form', 'Submit'],
            expected: 'Account created; continue to billing',
            actual: cartPage.url(),
            device: 'Desktop',
            screenshotOrLog: ref,
            suggestedFix: 'Fix captcha/validation on Register; ensure unique email and IsPermissionGranted.',
          });
        } else {
          if (!/CheckoutBillingDetails/i.test(cartPage.url())) {
            await cartPage.goto('/Payment/CheckoutBillingDetails', { waitUntil: 'domcontentloaded' });
          }
          const billingBody = await cartPage.locator('body').innerText();
          if (/Unhandled exception/i.test(billingBody) || (await cartPage.locator('#Cities').count()) === 0) {
            const ref = await shot(cartPage, 'billing-fail');
            addIssue({
              severity: 'Critical',
              area: 'Cart',
              page: '/Payment/CheckoutBillingDetails',
              title: 'Billing details page broken after register',
              description: billingBody.slice(0, 240),
              stepsToReproduce: ['Register during checkout', 'Open billing'],
              expected: 'City dropdown visible',
              actual: cartPage.url(),
              device: 'Desktop',
              screenshotOrLog: ref,
              suggestedFix: 'Fix CheckoutBillingDetails; ensure Ajax GetAllCities populates #Cities.',
            });
          } else {
            await fillBillingDetails(cartPage);
            await placeOrderIfReady(cartPage);
            const payBody = await cartPage.locator('body').innerText();
            if (/Unhandled exception/i.test(payBody)) {
              const ref = await shot(cartPage, 'placeorder-500');
              addIssue({
                severity: 'Critical',
                area: 'Payment',
                page: cartPage.url(),
                title: 'PlaceOrder throws unhandled exception',
                description: payBody.slice(0, 240),
                stepsToReproduce: ['Complete billing', 'Place order'],
                expected: 'Iyzico checkout form or friendly error',
                actual: 'Exception page',
                device: 'Desktop',
                screenshotOrLog: ref,
                suggestedFix: 'Guard Iyzico strategy when ApiKey/SecretKey empty; do not throw.',
              });
            } else if (/ödeme formu|payment form/i.test(payBody) && /boş|empty|ayar/i.test(payBody)) {
              const ref = await shot(cartPage, 'iyzico-empty-keys');
              addIssue({
                severity: 'High',
                area: 'Payment',
                page: cartPage.url(),
                title: 'Iyzico checkout form empty (API keys not configured)',
                description: 'PlaceOrder renders a warning instead of the sandbox iframe because IyzicoApiKey/IyzicoSecretKey are empty in IIS web.config.',
                stepsToReproduce: ['Complete checkout to PlaceOrder'],
                expected: 'Sandbox payment iframe when PaymentProvider=Iyzico',
                actual: payBody.slice(0, 200),
                device: 'Desktop',
                screenshotOrLog: ref,
                suggestedFix: 'Set sandbox Iyzico keys in IIS for local QA; keep empty-key UX as a blocking alert with admin link, not a blank iframe.',
              });
            } else {
              const pay = await tryPayWithIyzicoSandbox(cartPage);
              if (!pay.ok) {
                const ref = await shot(cartPage, 'iyzico-pay-fail');
                addIssue({
                  severity: pay.reason && /keys|empty|iframe not found/i.test(pay.reason) ? 'High' : 'Medium',
                  area: 'Payment',
                  page: cartPage.url(),
                  title: 'Iyzico sandbox payment did not complete',
                  description: pay.reason,
                  stepsToReproduce: ['Reach PlaceOrder', 'Fill iyzico sandbox card 5526080000000006'],
                  expected: 'PaymentResult / thank-you',
                  actual: pay.reason,
                  device: 'Desktop',
                  screenshotOrLog: ref,
                  suggestedFix: 'Configure sandbox keys; wait for iframe; map success redirect.',
                });
              }
            }
          }
        }
      }
    }
  } catch (err) {
    const ref = await shot(cartPage, 'cart-flow-ex');
    addIssue({
      severity: 'High',
      area: 'Cart',
      page: cartPage.url(),
      title: 'Cart/checkout flow threw in test harness',
      description: String(err && err.message ? err.message : err),
      stepsToReproduce: ['Run guest add-to-cart → checkout'],
      expected: 'Flow completes to payment',
      actual: String(err),
      device: 'Desktop',
      screenshotOrLog: ref,
      suggestedFix: 'Reproduce manually; fix the failing step.',
    });
  }
  await cartPage.close();

  // Mobile cart smoke
  const mobileCart = await context.newPage();
  await mobileCart.setViewportSize(MOBILE);
  try {
    await openProductDetail(mobileCart, PRODUCT_DETAIL);
    await addToCartFromDetail(mobileCart, { quantity: 1 });
    await goToCart(mobileCart);
    const diag = await pageDiagnostics(mobileCart);
    if (diag.overflowX > 8) {
      const ref = await shot(mobileCart, 'mobile-cart-overflow');
      addIssue({
        severity: 'Medium',
        area: 'Cart',
        page: '/Payment/ShoppingCart',
        title: 'Shopping cart horizontal overflow on mobile',
        description: `overflowX=${diag.overflowX}px`,
        stepsToReproduce: ['390x844', 'Add product', 'Open cart'],
        expected: 'No page-level horizontal scroll',
        actual: `overflow ${diag.overflowX}px`,
        device: 'Mobile',
        screenshotOrLog: ref,
        suggestedFix: 'Make cart table responsive (stack rows / overflow-x auto).',
      });
    }
    const addVisible = await mobileCart.locator('#AddToCart').isVisible().catch(() => false);
    // already left product page
    const proceed = mobileCart.locator('#ProceedToCheckout');
    if ((await proceed.count()) && !(await proceed.first().isVisible())) {
      const ref = await shot(mobileCart, 'mobile-checkout-btn-hidden');
      addIssue({
        severity: 'High',
        area: 'Cart',
        page: '/Payment/ShoppingCart',
        title: 'Proceed to checkout not visible on mobile',
        description: '#ProceedToCheckout exists but is not visible at 390px.',
        stepsToReproduce: ['Mobile viewport', 'Open cart with items'],
        expected: 'Checkout button tappable',
        actual: 'Button not visible',
        device: 'Mobile',
        screenshotOrLog: ref,
        suggestedFix: 'Move checkout CTA into visible mobile footer; avoid desktop-only floats.',
      });
    }
  } catch (err) {
    addIssue({
      severity: 'Medium',
      area: 'Cart',
      page: '/Payment/ShoppingCart',
      title: 'Mobile cart smoke failed',
      description: String(err && err.message ? err.message : err),
      stepsToReproduce: ['Mobile add to cart'],
      expected: 'Cart usable',
      actual: String(err),
      device: 'Mobile',
      suggestedFix: 'Reproduce on 390px viewport.',
    });
  }
  await mobileCart.close();

  // ---------- 10. Error pages ----------
  testedAreas.add('Error pages');
  for (const pth of ['/error/notfound/', '/error/badrequest/', '/error/forbidden/', '/error/internalservererror/', '/this-path-does-not-exist-qa-404/']) {
    const r = await visit(page, BASE + pth);
    if (r.status >= 500 && !/internalservererror/i.test(pth)) {
      addIssue({
        severity: 'High',
        area: 'Other',
        page: pth,
        title: `Error page itself fails: ${pth}`,
        description: (r.body || '').slice(0, 200),
        stepsToReproduce: [`Open ${pth}`],
        expected: 'Friendly error HTML',
        actual: `HTTP ${r.status}`,
        device: 'Both',
        suggestedFix: 'Error views must not depend on layout data that 500s.',
      });
    }
  }

  // ---------- 11. Logs ----------
  testedAreas.add('Logs');
  const logDir = 'C:\\inetpub\\wwwroot\\Eimece\\media\\logs';
  let logFiles = [];
  try {
    logFiles = fs.readdirSync(logDir).filter((f) => /\.(log|json|txt)$/i.test(f));
  } catch (_) {
    logFiles = [];
  }
  if (logFiles.length === 0) {
    addIssue({
      severity: 'Medium',
      area: 'Other',
      page: 'media/logs',
      title: 'NLog file targets write no log files under media/logs',
      description: 'NLog.config fileName is ${basedir}/media/logs/EImeceLog.log but the directory only contains Web.config. File logging may fail due to IIS app-pool ACL or basedir resolution. Database AppLogs target may still work.',
      stepsToReproduce: ['Trigger an error', 'List C:\\inetpub\\wwwroot\\Eimece\\media\\logs'],
      expected: 'EImeceLog.log / .json present',
      actual: 'No log files',
      device: 'Both',
      suggestedFix: 'Grant IIS AppPool write ACL on media/logs; verify ${basedir} is the site root; check NLog internalLogFile.',
    });
  }

  await page.goto(BASE + '/admin/applogs/?eventLevel=ERROR', { waitUntil: 'domcontentloaded' });
  const logBody = await page.locator('body').innerText();
  const refLogs = await shot(page, 'admin-applogs-error');
  if (/Unhandled exception/i.test(logBody)) {
    addIssue({
      severity: 'High',
      area: 'Admin',
      page: '/admin/applogs/',
      title: 'AppLogs admin page throws',
      description: logBody.slice(0, 200),
      stepsToReproduce: ['Open /admin/applogs/?eventLevel=ERROR'],
      expected: 'Grid of log rows',
      actual: 'Exception',
      device: 'Desktop',
      screenshotOrLog: refLogs,
      suggestedFix: 'Fix AppLogsController.Index query.',
    });
  }

  await browser.close();
}

function writeReport() {
  const byTitle = new Map();
  for (const issue of issues) {
    const key = `${issue.title.replace(/ \((desktop|mobile)\)/i, '')}|${issue.page}`;
    if (!byTitle.has(key)) {
      byTitle.set(key, issue);
    } else {
      const prev = byTitle.get(key);
      prev.device = 'Both';
    }
  }
  const deduped = [...byTitle.values()].map((issue, i) => ({
    ...issue,
    id: `BUG-${String(i + 1).padStart(3, '0')}`,
  }));

  const summary = {
    totalIssues: deduped.length,
    critical: deduped.filter((i) => i.severity === 'Critical').length,
    high: deduped.filter((i) => i.severity === 'High').length,
    medium: deduped.filter((i) => i.severity === 'Medium').length,
    low: deduped.filter((i) => i.severity === 'Low').length,
    testedAreas: [...testedAreas],
    generatedAt: new Date().toISOString(),
    baseUrl: BASE,
  };

  const report = { summary, issues: deduped };
  fs.writeFileSync(REPORT_PATH, JSON.stringify(report, null, 2), 'utf8');
  console.log(JSON.stringify(summary, null, 2));
  console.log('Wrote', REPORT_PATH);
}

run().then(writeReport).catch((err) => {
  console.error(err);
  addIssue({
    severity: 'High',
    area: 'Other',
    page: 'QA harness',
    title: 'QA script aborted with an uncaught error',
    description: String(err && err.stack ? err.stack : err),
    stepsToReproduce: ['Run Playwright/tmp-prod-qa.js'],
    expected: 'Harness completes',
    actual: String(err),
    device: 'Both',
    suggestedFix: 'Fix the failing flow; remaining issues above were still captured.',
  });
  writeReport();
  process.exit(1);
});
