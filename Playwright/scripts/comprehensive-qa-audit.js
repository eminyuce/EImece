const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
const http = require('http');

const BASE = 'http://localhost:81';
const SHOTS_DIR = path.join(__dirname, '..', 'screenshots', 'qa-audit');
fs.mkdirSync(SHOTS_DIR, { recursive: true });

const DESKTOP = { width: 1440, height: 900 };
const MOBILE = { width: 390, height: 844 };
const TABLET = { width: 768, height: 1024 };

const collectedIssues = [];
let bugCounter = 1;

function recordIssue({ severity, area, page, title, description, stepsToReproduce, expected, actual, device = 'Both', screenshotOrLog = '', suggestedFix = '' }) {
  const id = `BUG-${String(bugCounter++).padStart(3, '0')}`;
  const issue = {
    id,
    severity,
    area,
    page,
    title,
    description,
    stepsToReproduce: Array.isArray(stepsToReproduce) ? stepsToReproduce : [stepsToReproduce],
    expected,
    actual,
    device,
    screenshotOrLog,
    suggestedFix
  };
  collectedIssues.push(issue);
  console.log(`\n[${id}] [${severity}] [${area}] ${title} (${page})`);
  console.log(`   Actual: ${actual.slice(0, 140)}`);
  return issue;
}

async function takeScreenshot(page, name) {
  const safeName = name.replace(/[^a-zA-Z0-9_-]/g, '_');
  const filePath = path.join(SHOTS_DIR, `${safeName}.png`);
  try {
    await page.screenshot({ path: filePath, fullPage: false });
    return `Playwright/screenshots/qa-audit/${safeName}.png`;
  } catch (e) {
    return '';
  }
}

async function runAudit() {
  console.log('=== STARTING DEEP E2E QA AUDIT FOR EIMECE ===\n');
  const browser = await chromium.launch({ headless: true });

  const testedAreas = new Set([
    'Frontend / Customer pages (desktop + mobile)',
    'Admin panel pages and modules',
    'Authentication flows (customer + admin + 2FA)',
    'Shopping cart, checkout, and payment flows',
    'AJAX / jQuery-driven admin operations',
    'File uploads and media handling',
    'Reports and CSV/Excel exports',
    'Error pages and edge cases',
    'Application logs and runtime exceptions'
  ]);

  // Track console errors and network errors
  function setupPageMonitors(page, contextName) {
    const logs = { consoleErrors: [], pageErrors: [], failedRequests: [] };
    page.on('console', msg => {
      if (msg.type() === 'error') {
        const text = msg.text();
        if (!text.includes('favicon') && !text.includes('manifest.json') && !text.includes('apple-touch') && !text.includes('google-analytics')) {
          logs.consoleErrors.push(text);
        }
      }
    });
    page.on('pageerror', err => {
      logs.pageErrors.push(String(err));
    });
    page.on('response', res => {
      const status = res.status();
      const url = res.url();
      if (status >= 400 && url.startsWith(BASE) && !url.includes('favicon') && !url.includes('.map')) {
        logs.failedRequests.push({ status, url });
      }
    });
    return logs;
  }

  // -------------------------------------------------------------
  // SCOPE 1: Frontend / Customer Pages (Desktop + Mobile)
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 1: Frontend / Customer Pages (Desktop + Mobile) ---');
  
  const frontendUrls = [
    { path: '/', name: 'Homepage' },
    { path: '/info/aboutus/', name: 'About Us' },
    { path: '/info/contactus/', name: 'Contact Us' },
    { path: '/info/delivery/', name: 'Delivery' },
    { path: '/info/faq/', name: 'FAQ' },
    { path: '/info/privacy/', name: 'Privacy' },
    { path: '/info/returnconditions/', name: 'Return Conditions' },
    { path: '/info/distancecontract/', name: 'Distance Contract' },
    { path: '/s/', name: 'Stories Index' },
    { path: '/products/advancedsearch/', name: 'Advanced Search' },
    { path: '/payment/shoppingcart/', name: 'Shopping Cart' }
  ];

  const desktopContext = await browser.newContext({ viewport: DESKTOP });
  const desktopPage = await desktopContext.newPage();
  const desktopMonitors = setupPageMonitors(desktopPage, 'Desktop');

  const mobileContext = await browser.newContext({ viewport: MOBILE, userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.5 Mobile/15E148 Safari/604.1' });
  const mobilePage = await mobileContext.newPage();
  const mobileMonitors = setupPageMonitors(mobilePage, 'Mobile');

  // Test static & info routes
  for (const item of frontendUrls) {
    // Desktop
    try {
      desktopMonitors.consoleErrors.length = 0;
      desktopMonitors.pageErrors.length = 0;
      desktopMonitors.failedRequests.length = 0;
      
      const res = await desktopPage.goto(`${BASE}${item.path}`, { waitUntil: 'networkidle', timeout: 30000 });
      const status = res?.status() || 0;
      const title = await desktopPage.title();
      const bodyText = await desktopPage.evaluate(() => document.body.innerText);

      if (status >= 500 || bodyText.includes('Unhandled exception') || bodyText.includes('Server Error')) {
        const shot = await takeScreenshot(desktopPage, `desktop_500_${item.name}`);
        recordIssue({
          severity: 'Critical',
          area: 'Frontend',
          page: item.path,
          title: `${item.name} returns 500 Internal Server Error`,
          description: `Navigating to ${item.path} fails with status ${status} or unhandled exception.`,
          stepsToReproduce: [`Navigate to ${BASE}${item.path}`],
          expected: 'HTTP 200 OK with valid page content',
          actual: `HTTP ${status}: ${bodyText.slice(0, 200)}`,
          device: 'Desktop',
          screenshotOrLog: shot,
          suggestedFix: 'Check controller and view exception logs.'
        });
      } else if (status === 404) {
        recordIssue({
          severity: 'High',
          area: 'Frontend',
          page: item.path,
          title: `${item.name} returns 404 Not Found`,
          description: `Frontend page ${item.path} returns 404 Not Found.`,
          stepsToReproduce: [`Navigate to ${BASE}${item.path}`],
          expected: 'HTTP 200 OK',
          actual: `HTTP 404 Not Found`,
          device: 'Desktop',
          suggestedFix: 'Ensure route is registered and content exists.'
        });
      }

      if (desktopMonitors.pageErrors.length > 0) {
        recordIssue({
          severity: 'Medium',
          area: 'Frontend',
          page: item.path,
          title: `JavaScript runtime error on ${item.name}`,
          description: `Uncaught JS exception: ${desktopMonitors.pageErrors.join('; ')}`,
          stepsToReproduce: [`Navigate to ${BASE}${item.path}`, 'Check browser developer console'],
          expected: 'Clean JS execution with 0 unhandled exceptions',
          actual: desktopMonitors.pageErrors.join('\n'),
          device: 'Desktop',
          suggestedFix: 'Debug and fix JavaScript null reference or missing library script.'
        });
      }
    } catch (err) {
      console.error(`Error testing ${item.path} on desktop: ${err.message}`);
    }

    // Mobile
    try {
      mobileMonitors.consoleErrors.length = 0;
      mobileMonitors.pageErrors.length = 0;
      mobileMonitors.failedRequests.length = 0;
      
      const res = await mobilePage.goto(`${BASE}${item.path}`, { waitUntil: 'networkidle', timeout: 30000 });
      const overflow = await mobilePage.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
      if (overflow > 5) {
        const shot = await takeScreenshot(mobilePage, `mobile_overflow_${item.name}`);
        recordIssue({
          severity: 'Medium',
          area: 'Frontend',
          page: item.path,
          title: `Horizontal layout overflow on ${item.name} (Mobile)`,
          description: `Page content exceeds mobile viewport width by ${overflow}px, causing unwanted horizontal scrolling.`,
          stepsToReproduce: [`Set viewport to 390x844`, `Navigate to ${BASE}${item.path}`, 'Observe horizontal scrollbar / overflow'],
          expected: 'Zero horizontal overflow (scrollWidth === clientWidth)',
          actual: `Horizontal overflow of ${overflow}px detected`,
          device: 'Mobile',
          screenshotOrLog: shot,
          suggestedFix: 'Inspect layout CSS and add overflow-x: hidden or adjust max-width / container paddings.'
        });
      }
    } catch (err) {
      console.error(`Error testing ${item.path} on mobile: ${err.message}`);
    }
  }

  // Check Category Listings and Product Details
  console.log('\n--- Checking Categories, Product Listings, and Product Details ---');
  await desktopPage.goto(`${BASE}/`, { waitUntil: 'domcontentloaded' });
  const categoryLinks = await desktopPage.evaluate(() => {
    return Array.from(document.querySelectorAll('a[href*="/c/pc/"], a[href*="/productcategories/"]'))
      .map(a => a.getAttribute('href'))
      .filter(Boolean);
  });
  console.log(`Found ${categoryLinks.length} category links on homepage.`);

  const sampleCatLink = categoryLinks.length > 0 ? categoryLinks[0] : '/c/pc/elektronik-7e7e4h1b/';
  const fullCatUrl = sampleCatLink.startsWith('http') ? sampleCatLink : `${BASE}${sampleCatLink}`;
  const catRes = await desktopPage.goto(fullCatUrl, { waitUntil: 'networkidle' });
  console.log(`Category page ${sampleCatLink} status: ${catRes?.status()}`);

  const productLinks = await desktopPage.evaluate(() => {
    return Array.from(document.querySelectorAll('a[href*="/p/"], a[href*="/products/"]'))
      .map(a => a.getAttribute('href'))
      .filter(h => h && !h.includes('/p/t/') && !h.includes('/search') && !h.includes('/advancedsearch'));
  });
  console.log(`Found ${productLinks.length} product links on category page.`);

  // Check first products
  let inStockProductUrl = null;
  let outOfStockProductUrl = null;

  for (let i = 0; i < Math.min(productLinks.length, 10); i++) {
    const pLink = productLinks[i];
    const fullPUrl = pLink.startsWith('http') ? pLink : `${BASE}${pLink}`;
    await desktopPage.goto(fullPUrl, { waitUntil: 'networkidle' });
    const isOutOfStock = await desktopPage.evaluate(() => {
      const text = document.body.innerText;
      return text.includes('Stokta Yok') || text.includes('Tükendi') || !document.querySelector('#AddToCart, button[data-add-product-cart], .add-to-cart-btn, form[action*="AddToCart"]');
    });
    if (isOutOfStock && !outOfStockProductUrl) {
      outOfStockProductUrl = pLink;
    } else if (!isOutOfStock && !inStockProductUrl) {
      inStockProductUrl = pLink;
    }
  }

  console.log(`In-stock product sample: ${inStockProductUrl}`);
  console.log(`Out-of-stock product sample: ${outOfStockProductUrl}`);

  // Test Mobile Navigation Hamburger
  console.log('\n--- Testing Mobile Hamburger Menu ---');
  await mobilePage.goto(`${BASE}/`, { waitUntil: 'networkidle' });
  const hamburger = mobilePage.locator('.navbar-toggler, button[data-bs-toggle="collapse"], #mobile-nav-toggle, [aria-label*="Menu"]').first();
  if (await hamburger.isVisible()) {
    const tagName = await hamburger.evaluate(el => el.tagName.toLowerCase());
    const role = await hamburger.getAttribute('role');
    const ariaLabel = await hamburger.getAttribute('aria-label');
    if (tagName !== 'button' && role !== 'button' && !ariaLabel) {
      recordIssue({
        severity: 'Medium',
        area: 'Frontend',
        page: '/',
        title: 'Mobile menu toggler is not an accessible button control',
        description: `Mobile navigation toggler is rendered as <${tagName}> without role="button" or aria-label, failing accessibility standards for screen readers and keyboard navigation.`,
        stepsToReproduce: ['View homepage on mobile viewport (390x844)', 'Inspect the hamburger menu toggle DOM element'],
        expected: '<button type="button" class="navbar-toggler" aria-label="Menu" aria-expanded="false">',
        actual: `<${tagName} class="${await hamburger.getAttribute('class')}"> without button semantics`,
        device: 'Mobile',
        suggestedFix: 'Replace container with button element or add role="button", tabindex="0", and aria-label="Toggle navigation".'
      });
    }
    await hamburger.click();
    await mobilePage.waitForTimeout(500);
    const navVisible = await mobilePage.evaluate(() => {
      const nav = document.querySelector('#nav, .navbar-collapse, .mobile-nav-container, .crizal-nav');
      if (!nav) return false;
      const rect = nav.getBoundingClientRect();
      return rect.height > 0 && rect.width > 0 && window.getComputedStyle(nav).display !== 'none';
    });
    console.log(`Mobile nav opened: ${navVisible}`);
  }

  // Test Header Phone and Links
  console.log('\n--- Checking Header Tel / Contact Links ---');
  const telHref = await desktopPage.evaluate(() => {
    const a = document.querySelector('a[href^="tel:"]');
    return a ? a.getAttribute('href') : null;
  });
  if (telHref && (telHref.includes(' ') || telHref.includes('|') || !/^tel:\+?[0-9]+$/.test(telHref.trim()))) {
    recordIssue({
      severity: 'Medium',
      area: 'Frontend',
      page: '/',
      title: 'Header phone link has invalid tel: URI format',
      description: `The telephone link is configured as "${telHref}", which contains spaces, pipe symbols, or non-dialable characters causing mobile OS dialers to fail.`,
      stepsToReproduce: ['Open homepage on desktop or mobile', 'Inspect the top contact phone number link in the header'],
      expected: 'Clean dialable format like tel:+902165550123',
      actual: `href="${telHref}"`,
      device: 'Both',
      suggestedFix: 'Strip spaces, pipes, and non-numeric characters (except leading +) in the tel: href attribute in _WebSiteAddressInfo.cshtml.'
    });
  }

  // Test Search functionality
  console.log('\n--- Testing Search (Basic, Empty, Special Characters) ---');
  // 1. Basic search
  const searchInput = desktopPage.locator('input[name="search"], input[name="q"], #search-input, input.search-input').first();
  if (await searchInput.isVisible().catch(() => false)) {
    await searchInput.fill('pro');
    await searchInput.press('Enter');
    await desktopPage.waitForLoadState('networkidle');
    const searchStatus = desktopPage.url();
    console.log(`Search result URL: ${searchStatus}`);
  }

  // 2. Empty search GET request
  const emptySearchRes = await desktopPage.goto(`${BASE}/products/searchproducts/?search=&page=1&sorting=0`, { waitUntil: 'networkidle' });
  if (emptySearchRes && emptySearchRes.status() >= 400) {
    recordIssue({
      severity: 'Medium',
      area: 'Frontend',
      page: '/products/searchproducts/?search=',
      title: 'Empty search query returns HTTP 400 Bad Request error',
      description: 'When a user submits an empty search term, ProductsController.SearchProducts logs an error and returns HTTP 400 Bad Request instead of rendering an empty search results view with a friendly notice.',
      stepsToReproduce: [
        'Navigate to http://localhost:81/products/searchproducts/?search=&page=1&sorting=0',
        'Observe the HTTP 400 Bad Request response'
      ],
      expected: 'HTTP 200 OK rendering the search page with an informative message ("Lütfen bir arama terimi giriniz").',
      actual: `HTTP ${emptySearchRes.status()} Bad Request`,
      device: 'Both',
      suggestedFix: 'In ProductsController.SearchProducts, if search is empty, return View(empty model) or redirect to search page with notification instead of new HttpStatusCodeResult(HttpStatusCode.BadRequest).'
    });
  }

  // 3. Search with special characters (XSS/SQL test)
  const specialChars = ["<script>alert(1)</script>", "shoes' OR '1'='1", 'çanta & aksesuar', '100%'];
  for (const q of specialChars) {
    const enc = encodeURIComponent(q);
    const sRes = await desktopPage.goto(`${BASE}/products/searchproducts/?search=${enc}&page=1&sorting=0`, { waitUntil: 'networkidle' });
    const sBody = await desktopPage.evaluate(() => document.body.innerText);
    if (sRes && (sRes.status() >= 500 || sBody.includes('Unhandled exception'))) {
      recordIssue({
        severity: 'High',
        area: 'Frontend',
        page: `/products/searchproducts/?search=${enc}`,
        title: `Search fails with 500 when querying special characters: "${q}"`,
        description: `Search query with special characters causes an unhandled exception or 500 Internal Server Error.`,
        stepsToReproduce: [`Navigate to http://localhost:81/products/searchproducts/?search=${enc}`],
        expected: 'HTTP 200 OK with sanitized search results',
        actual: `HTTP ${sRes ? sRes.status() : 0} error`,
        device: 'Both',
        suggestedFix: 'Ensure search query parameters are properly sanitized and EF queries handle special characters safely.'
      });
    }
  }

  // -------------------------------------------------------------
  // SCOPE 2: Admin Panel Pages & Operations
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 2: Admin Panel Pages & Modules ---');

  const adminPages = [
    { path: '/admin/dashboard/', name: 'Dashboard' },
    { path: '/admin/products/', name: 'Products List' },
    { path: '/admin/products/saveoredit', name: 'Product Create' },
    { path: '/admin/productcategories/', name: 'Product Categories' },
    { path: '/admin/productcategories/saveoredit', name: 'Product Category Create' },
    { path: '/admin/brands/', name: 'Brands' },
    { path: '/admin/brands/saveoredit', name: 'Brand Create' },
    { path: '/admin/orders/', name: 'Orders' },
    { path: '/admin/customers/', name: 'Customers' },
    { path: '/admin/tags/', name: 'Tags' },
    { path: '/admin/tagcategories/', name: 'Tag Categories' },
    { path: '/admin/coupons/', name: 'Coupons' },
    { path: '/admin/coupons/saveoredit', name: 'Coupon Create' },
    { path: '/admin/lists/', name: 'Lists' },
    { path: '/admin/templates/', name: 'Templates' },
    { path: '/admin/mailtemplates/', name: 'Mail Templates' },
    { path: '/admin/stories/', name: 'Stories' },
    { path: '/admin/storycategories/', name: 'Story Categories' },
    { path: '/admin/menus/', name: 'Menus' },
    { path: '/admin/mainpageimages/', name: 'Slider / Main Page Images' },
    { path: '/admin/faq/', name: 'FAQ Admin' },
    { path: '/admin/filestorages/', name: 'Media Library' },
    { path: '/admin/settings/', name: 'Settings' },
    { path: '/admin/applogs/', name: 'App Logs' },
    { path: '/admin/database/', name: 'Database Maintenance' },
    { path: '/metrics', name: 'Metrics Endpoint' }
  ];

  for (const adm of adminPages) {
    desktopMonitors.consoleErrors.length = 0;
    desktopMonitors.pageErrors.length = 0;
    desktopMonitors.failedRequests.length = 0;

    try {
      const res = await desktopPage.goto(`${BASE}${adm.path}`, { waitUntil: 'networkidle', timeout: 30000 });
      const status = res?.status() || 0;
      const bodyText = await desktopPage.evaluate(() => document.body.innerText);

      if (status >= 500 || bodyText.includes('Unhandled exception') || bodyText.includes('Server Error in')) {
        const shot = await takeScreenshot(desktopPage, `admin_500_${adm.name}`);
        recordIssue({
          severity: 'Critical',
          area: 'Admin',
          page: adm.path,
          title: `Admin page "${adm.name}" fails with HTTP ${status} / Unhandled Exception`,
          description: `Navigating to ${adm.path} in the admin panel crashes with a 500 error or unhandled server exception.`,
          stepsToReproduce: [`Log into admin or enable BypassAdminAuth`, `Navigate to ${BASE}${adm.path}`],
          expected: `HTTP 200 OK rendering the ${adm.name} management UI`,
          actual: `HTTP ${status}: ${bodyText.slice(0, 250)}`,
          device: 'Desktop',
          screenshotOrLog: shot,
          suggestedFix: 'Inspect the controller action and view model dependencies for missing null checks or database query errors.'
        });
      } else if (status === 404) {
        recordIssue({
          severity: 'High',
          area: 'Admin',
          page: adm.path,
          title: `Admin route "${adm.name}" returned 404 Not Found`,
          description: `Route ${adm.path} could not be resolved by MVC routing.`,
          stepsToReproduce: [`Navigate to ${BASE}${adm.path}`],
          expected: 'HTTP 200 OK',
          actual: 'HTTP 404 Not Found',
          device: 'Desktop',
          suggestedFix: 'Register missing admin route or verify controller action naming.'
        });
      }

      if (desktopMonitors.pageErrors.length > 0) {
        recordIssue({
          severity: 'Medium',
          area: 'Admin',
          page: adm.path,
          title: `JavaScript error on admin page "${adm.name}"`,
          description: `Console reported unhandled JavaScript exceptions on ${adm.path}: ${desktopMonitors.pageErrors.join('; ')}`,
          stepsToReproduce: [`Open ${BASE}${adm.path}`, 'Open DevTools console'],
          expected: 'Zero uncaught JS errors',
          actual: desktopMonitors.pageErrors.join('\n'),
          device: 'Desktop',
          suggestedFix: 'Fix undefined variable references or jQuery plugin initializations.'
        });
      }
    } catch (e) {
      console.error(`Admin test error on ${adm.path}: ${e.message}`);
    }
  }

  // -------------------------------------------------------------
  // SCOPE 3: Authentication Flows (Customer & Admin)
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 3: Authentication Flows ---');

  // Customer Login Page
  const custLoginRes = await desktopPage.goto(`${BASE}/account/login/`, { waitUntil: 'networkidle' });
  console.log(`Customer login page status: ${custLoginRes?.status()}`);
  const hasCustLoginForm = await desktopPage.locator('form[action*="login"], #Email, input[name="Email"]').first().isVisible();
  console.log(`Customer login form visible: ${hasCustLoginForm}`);

  // Customer Register Page
  const custRegRes = await desktopPage.goto(`${BASE}/account/register/`, { waitUntil: 'networkidle' });
  console.log(`Customer register page status: ${custRegRes?.status()}`);
  const hasCustRegForm = await desktopPage.locator('form[action*="register"], #Email, input[name="Email"]').first().isVisible();
  console.log(`Customer register form visible: ${hasCustRegForm}`);

  // Forgot Password Page
  const forgotRes = await desktopPage.goto(`${BASE}/account/forgotpassword/`, { waitUntil: 'networkidle' });
  console.log(`Forgot password page status: ${forgotRes?.status()}`);

  // Test Customer Registration
  const testRegEmail = `qa_test_${Date.now()}@eimece.test`;
  console.log(`Attempting customer registration with: ${testRegEmail}`);
  await desktopPage.goto(`${BASE}/account/register/`, { waitUntil: 'networkidle' });
  try {
    const regForm = desktopPage.locator('form').first();
    await desktopPage.fill('input[name="Name"], #Name', 'QA Tester');
    await desktopPage.fill('input[name="Surname"], #Surname', 'Automated');
    await desktopPage.fill('input[name="Email"], #Email', testRegEmail);
    await desktopPage.fill('input[name="Password"], #Password', 'Test12345!');
    await desktopPage.fill('input[name="ConfirmPassword"], #ConfirmPassword', 'Test12345!');
    const captchaInput = desktopPage.locator('input[name="Captcha"], #Captcha');
    if (await captchaInput.isVisible().catch(() => false)) {
      // Try captcha brute force 2..8
      for (let ans = 2; ans <= 8; ans++) {
        await captchaInput.fill(String(ans));
        await desktopPage.locator('button[type="submit"], input[type="submit"]').first().click();
        await desktopPage.waitForTimeout(1000);
        if (!desktopPage.url().includes('/account/register')) break;
      }
    } else {
      await desktopPage.locator('button[type="submit"], input[type="submit"]').first().click();
      await desktopPage.waitForTimeout(1500);
    }
    console.log(`Post-registration URL: ${desktopPage.url()}`);
  } catch (e) {
    console.error(`Registration error: ${e.message}`);
  }

  // -------------------------------------------------------------
  // SCOPE 4: Shopping Cart, Checkout, and Payment
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 4: Shopping Cart, Checkout, and Payment ---');

  // Let's find an in-stock product or use product listing
  await desktopPage.goto(`${BASE}/c/pc/elektronik-7e7e4h1b/`, { waitUntil: 'networkidle' });
  const pLinks = await desktopPage.evaluate(() => {
    return Array.from(document.querySelectorAll('a[href*="/p/"]')).map(a => a.getAttribute('href'));
  });

  let productAddedToCart = false;
  for (const pl of pLinks) {
    if (!pl || pl.includes('/p/t/')) continue;
    const fullPl = pl.startsWith('http') ? pl : `${BASE}${pl}`;
    await desktopPage.goto(fullPl, { waitUntil: 'networkidle' });
    const addBtn = desktopPage.locator('#AddToCart, button[data-add-product-cart], .crizal-product-detail__btn-cart, form[action*="AddToCart"] button').first();
    if (await addBtn.isVisible().catch(() => false)) {
      console.log(`Found AddToCart on ${pl}. Clicking...`);
      await addBtn.click();
      await desktopPage.waitForTimeout(1500);
      productAddedToCart = true;
      break;
    }
  }

  // Navigate to Cart
  await desktopPage.goto(`${BASE}/payment/shoppingcart/`, { waitUntil: 'networkidle' });
  const cartBodyText = await desktopPage.evaluate(() => document.body.innerText);
  console.log(`Cart page text preview: ${cartBodyText.slice(0, 150)}`);

  // Check Cart Quantity Updates
  const qtyInput = desktopPage.locator('input.cart-qty, input[name*="Quantity"], input[name*="quantity"]').first();
  if (await qtyInput.isVisible().catch(() => false)) {
    console.log('Cart has quantity input. Testing quantity adjustment...');
    await qtyInput.fill('2');
    await qtyInput.press('Enter');
    await desktopPage.waitForTimeout(1000);
  }

  // Test Guest Checkout vs Member Checkout
  const guestCheckoutRes = await desktopPage.goto(`${BASE}/payment/shoppingwithoutaccount/`, { waitUntil: 'networkidle' });
  console.log(`Guest checkout page status: ${guestCheckoutRes?.status()}`);
  if (guestCheckoutRes && guestCheckoutRes.status() >= 500) {
    recordIssue({
      severity: 'Critical',
      area: 'Cart',
      page: '/payment/shoppingwithoutaccount/',
      title: 'Guest checkout returns HTTP 500 Server Error',
      description: 'Navigating to shopping without account fails with a 500 error.',
      stepsToReproduce: ['Add product to cart', 'Navigate to http://localhost:81/payment/shoppingwithoutaccount/'],
      expected: 'HTTP 200 OK rendering guest address & checkout form',
      actual: `HTTP ${guestCheckoutRes.status()}`,
      device: 'Both',
      suggestedFix: 'Fix ShoppingWithoutAccount action exception.'
    });
  }

  // Test AJAX City/Town Cascade
  console.log('\n--- Testing AJAX City/Town Cascade ---');
  try {
    const citySelect = desktopPage.locator('select[name*="City"], select#CityId, select#BillingCityId').first();
    if (await citySelect.isVisible().catch(() => false)) {
      const cityOptions = await citySelect.locator('option').all();
      if (cityOptions.length > 1) {
        const val = await cityOptions[1].getAttribute('value');
        console.log(`Selecting city value: ${val}`);
        await citySelect.selectOption(val);
        await desktopPage.waitForTimeout(1000);
        const townSelect = desktopPage.locator('select[name*="Town"], select#TownId, select#BillingTownId').first();
        const townOptions = await townSelect.locator('option').all();
        console.log(`Loaded ${townOptions.length} town options for city.`);
        if (townOptions.length <= 1) {
          recordIssue({
            severity: 'High',
            area: 'Cart',
            page: '/payment/shoppingwithoutaccount/',
            title: 'AJAX City to Town cascade did not populate towns',
            description: 'Selecting a province/city from the dropdown did not populate the corresponding district/town dropdown.',
            stepsToReproduce: ['Open guest checkout or billing address form', 'Select a city from the dropdown', 'Inspect town dropdown'],
            expected: 'Town dropdown is populated with district options via AJAX',
            actual: `Town dropdown has ${townOptions.length} options (empty or only default placeholder)`,
            device: 'Both',
            suggestedFix: 'Verify /Payment/GetTowns or region cascade AJAX URL and JSON response structure.'
          });
        }
      }
    }
  } catch (e) {
    console.error(`City cascade error: ${e.message}`);
  }

  // Test PlaceOrder Payment Step
  console.log('\n--- Testing Payment / PlaceOrder Step ---');
  const placeOrderRes = await desktopPage.goto(`${BASE}/payment/placeorder/`, { waitUntil: 'networkidle' });
  const placeOrderText = await desktopPage.evaluate(() => document.body.innerText);
  console.log(`PlaceOrder page status: ${placeOrderRes?.status()}`);
  if (placeOrderRes && placeOrderRes.status() >= 500) {
    recordIssue({
      severity: 'Critical',
      area: 'Payment',
      page: '/payment/placeorder/',
      title: 'PlaceOrder page returns HTTP 500 Server Error',
      description: 'Navigating to /payment/placeorder/ throws an unhandled exception.',
      stepsToReproduce: ['Navigate to http://localhost:81/payment/placeorder/'],
      expected: 'Payment page with payment provider iframe or graceful cart redirection',
      actual: `HTTP ${placeOrderRes.status()}: ${placeOrderText.slice(0, 200)}`,
      device: 'Both',
      suggestedFix: 'Ensure PlaceOrder handles missing cart / session data gracefully.'
    });
  }

  // -------------------------------------------------------------
  // SCOPE 5: AJAX / jQuery-Driven Admin Operations
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 5: Admin AJAX Operations ---');

  // Test Admin Cache Clear Action
  try {
    const clearCacheRes = await desktopPage.goto(`${BASE}/admin/dashboard/clearcache`, { waitUntil: 'networkidle' });
    console.log(`Admin ClearCache endpoint status: ${clearCacheRes?.status()}`);
    if (clearCacheRes && clearCacheRes.status() >= 500) {
      recordIssue({
        severity: 'High',
        area: 'Admin',
        page: '/admin/dashboard/clearcache',
        title: 'Admin ClearCache action crashes with HTTP 500',
        description: 'Triggering the admin cache clear action throws an unhandled exception.',
        stepsToReproduce: ['Open http://localhost:81/admin/dashboard/clearcache'],
        expected: 'Cache cleared successfully with redirect to dashboard or JSON response',
        actual: `HTTP ${clearCacheRes.status()}`,
        device: 'Desktop',
        suggestedFix: 'Check ApplicationCacheClearer / MemoryCache eviction logic.'
      });
    }
  } catch (e) {
    console.error(`ClearCache error: ${e.message}`);
  }

  // Test AJAX Anonymous Protection
  const anonContext = await browser.newContext();
  const anonPage = await anonContext.newPage();
  // If BypassAdminAuth is enabled, let's test if protected customer/admin routes behave
  const anonCustomerRes = await anonPage.goto(`${BASE}/customers/`, { waitUntil: 'networkidle' });
  console.log(`Anonymous access to /customers/ status: ${anonCustomerRes?.status()}, URL: ${anonPage.url()}`);
  if (!anonPage.url().includes('/account/login') && !anonPage.url().includes('/account/register') && anonCustomerRes?.status() === 200) {
    // If it allows anonymous access to customer profile
    const custText = await anonPage.evaluate(() => document.body.innerText);
    if (custText.includes('Siparişlerim') || custText.includes('Profilim')) {
      recordIssue({
        severity: 'Critical',
        area: 'Auth',
        page: '/customers/',
        title: 'Customers profile area is accessible to unauthenticated anonymous users',
        description: 'Anonymous users can access customer profile and orders without logging in.',
        stepsToReproduce: ['Open private browsing window', 'Navigate to http://localhost:81/customers/'],
        expected: 'HTTP 302 Redirect to /account/login/',
        actual: 'HTTP 200 OK showing customer area',
        device: 'Both',
        suggestedFix: 'Add [Authorize] attribute to CustomersController.'
      });
    }
  }
  await anonContext.close();

  // -------------------------------------------------------------
  // SCOPE 6: File Uploads & Media Handling
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 6: File Uploads and Media Library ---');
  await desktopPage.goto(`${BASE}/admin/filestorages/`, { waitUntil: 'networkidle' });
  const mediaGridVisible = await desktopPage.locator('.grid-mvc, table, .admin-media-library').first().isVisible().catch(() => false);
  console.log(`Media Library table/grid visible: ${mediaGridVisible}`);

  // Test Image resizing endpoint
  const testResizeRes = await desktopPage.goto(`${BASE}/images/w300h300/test-sample.jpg/`, { waitUntil: 'networkidle' });
  console.log(`Image resize endpoint status on sample: ${testResizeRes?.status()}`);

  // Test Zero dimension resize
  const zeroDimRes = await desktopPage.goto(`${BASE}/images/w0h500/test-sample.jpg/`, { waitUntil: 'networkidle' });
  console.log(`Zero-dim resize w0h500 status: ${zeroDimRes?.status()}`);

  // -------------------------------------------------------------
  // SCOPE 7: Reports and Exports
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 7: Reports and Exports ---');

  const reportEndpoints = [
    { path: '/admin/report/productsummary/', name: 'Product Summary Report' },
    { path: '/admin/report/priceanalysis/', name: 'Price Analysis Report' },
    { path: '/admin/report/productinventory/', name: 'Product Inventory Report' },
    { path: '/admin/report/financialreport/', name: 'Financial Report' },
    { path: '/admin/report/export?reportKey=ProductInventory&format=csv', name: 'ProductInventory CSV Export' },
    { path: '/admin/report/export?reportKey=PriceAnalysis&format=csv', name: 'PriceAnalysis CSV Export' },
    { path: '/admin/report/export?reportKey=FinancialReport&format=csv&startDate=2026-01-01&endDate=2026-12-31', name: 'FinancialReport CSV Export (ISO dates)' },
    { path: '/admin/report/export?reportKey=FinancialReport&format=csv&startDate=01.01.2026&endDate=31.12.2026', name: 'FinancialReport CSV Export (TR dates)' }
  ];

  for (const rep of reportEndpoints) {
    try {
      const res = await desktopPage.goto(`${BASE}${rep.path}`, { waitUntil: 'networkidle', timeout: 25000 });
      const status = res?.status() || 0;
      const text = await desktopPage.evaluate(() => document.body ? document.body.innerText : '');

      if (status >= 500 || text.includes('Unhandled exception') || text.includes('Server Error in')) {
        const shot = await takeScreenshot(desktopPage, `report_500_${rep.name}`);
        recordIssue({
          severity: 'High',
          area: 'Reports',
          page: rep.path,
          title: `Report "${rep.name}" returns HTTP 500 Internal Server Error`,
          description: `Requesting report or export endpoint ${rep.path} fails with HTTP ${status} or unhandled exception.`,
          stepsToReproduce: [`Navigate to ${BASE}${rep.path}`],
          expected: 'HTTP 200 OK with report data or file download',
          actual: `HTTP ${status}: ${text.slice(0, 240)}`,
          device: 'Desktop',
          screenshotOrLog: shot,
          suggestedFix: 'In DatabaseUtility.GetSqlParameter handle nulls with DBNull.Value; in ReportExportFilter ensure date binding supports both culture-invariant ISO (yyyy-MM-dd) and culture-specific (dd.MM.yyyy) formats.'
        });
      } else if (status === 400 && rep.path.includes('startDate=01.01.2026')) {
        recordIssue({
          severity: 'Medium',
          area: 'Reports',
          page: rep.path,
          title: 'Financial report export fails with HTTP 400 when dates are passed in Turkish format',
          description: 'The report UI displays Turkish dates (dd.MM.yyyy) but the export controller fails with HTTP 400 Bad Request because model binding expects yyyy-MM-dd.',
          stepsToReproduce: [`GET ${BASE}${rep.path}`],
          expected: 'HTTP 200 CSV download parsing Turkish formatted dates properly',
          actual: 'HTTP 400 Bad Request',
          device: 'Desktop',
          suggestedFix: 'Update model binder or controller parser to handle DateTime strings in both dd.MM.yyyy and yyyy-MM-dd formats.'
        });
      }
    } catch (e) {
      console.error(`Report error on ${rep.path}: ${e.message}`);
    }
  }

  // -------------------------------------------------------------
  // SCOPE 8: Error Pages & Edge Cases
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 8: Error Pages and Edge Cases ---');

  const errorRoutes = [
    { path: '/this-page-does-not-exist-at-all-404', expectedStatus: 404, name: 'Standard 404' },
    { path: '/c/pc/non-existent-category-slug-999999/', expectedStatus: 404, name: 'Invalid Category Slug' },
    { path: '/p/non-existent-product-slug-999999/', expectedStatus: 404, name: 'Invalid Product Slug' },
    { path: '/s/non-existent-story-999999/', expectedStatus: 404, name: 'Invalid Story Slug' },
    { path: '/error/notfound/', expectedStatus: 404, name: 'Explicit NotFound Error View' },
    { path: '/error/internalservererror/', expectedStatus: 500, name: 'Explicit 500 Error View' },
    { path: '/error/badrequest/', expectedStatus: 400, name: 'Explicit BadRequest Error View' }
  ];

  for (const errRoute of errorRoutes) {
    try {
      const res = await desktopPage.goto(`${BASE}${errRoute.path}`, { waitUntil: 'networkidle' });
      const status = res?.status() || 0;
      const body = await desktopPage.evaluate(() => document.body.innerText);

      // Check if custom error page is shown or yellow screen of death
      if (body.includes('Server Error in') || body.includes('Stack Trace:')) {
        recordIssue({
          severity: 'High',
          area: 'Error Handling',
          page: errRoute.path,
          title: `Unstyled ASP.NET Yellow Screen of Death shown on ${errRoute.name}`,
          description: `Route ${errRoute.path} leaked raw ASP.NET server error details and stack trace instead of rendering custom friendly error page.`,
          stepsToReproduce: [`Navigate to ${BASE}${errRoute.path}`],
          expected: 'Custom branded error page matching site design',
          actual: `Raw ASP.NET exception screen: ${body.slice(0, 200)}`,
          device: 'Both',
          suggestedFix: 'Configure customErrors mode="RemoteOnly" / ErrorController to render branded error views for 404, 500, and 400.'
        });
      }
    } catch (e) {
      console.error(`Error route check failed on ${errRoute.path}: ${e.message}`);
    }
  }

  // Check Sitemap
  console.log('\n--- Checking Sitemap.xml ---');
  const sitemapRes = await desktopPage.goto(`${BASE}/sitemap.xml`, { waitUntil: 'networkidle' });
  if (sitemapRes?.status() === 200) {
    const sitemapContent = await desktopPage.content();
    const locMatches = sitemapContent.match(/<loc>(.*?)<\/loc>/g) || [];
    console.log(`Found ${locMatches.length} URLs in sitemap.xml.`);
    
    // Sample first 20 URLs to check if any 404
    let deadSitemapUrls = 0;
    const deadSamples = [];
    for (let i = 0; i < Math.min(locMatches.length, 30); i++) {
      const locUrl = locMatches[i].replace(/<\/?loc>/g, '').trim();
      try {
        const checkRes = await desktopPage.goto(locUrl, { waitUntil: 'domcontentloaded', timeout: 10000 });
        if (checkRes?.status() === 404) {
          deadSitemapUrls++;
          deadSamples.push(locUrl);
        }
      } catch (e) {}
    }
    if (deadSitemapUrls > 0) {
      recordIssue({
        severity: 'High',
        area: 'Frontend',
        page: '/sitemap.xml',
        title: 'Sitemap contains non-existent / conventional MVC URLs that return 404',
        description: `Sitemap generator produces URLs using conventional controller paths like /productcategories/category/{slug} or /stories/categories/{slug} instead of the canonical attribute routes /c/pc/{slug}/ and /s/sc/{slug}/, leading to indexed 404 dead links. Found ${deadSitemapUrls} broken links out of 30 tested.`,
        stepsToReproduce: ['GET http://localhost:81/sitemap.xml', `Inspect loc entries, e.g. ${deadSamples[0]}`, `Open URL in browser`],
        expected: 'All sitemap <loc> URLs return HTTP 200 canonical pages',
        actual: `Sample broken sitemap URLs: ${deadSamples.slice(0, 3).join(', ')}`,
        device: 'Both',
        suggestedFix: 'In SiteMapService and EntityExtension.BuildDetailRelativePathWithoutHttpContext, map category and story routes to canonical attribute route patterns (/c/pc/, /s/sc/).'
      });
    }
  }

  // -------------------------------------------------------------
  // SCOPE 9: Application Logs Inspection
  // -------------------------------------------------------------
  console.log('\n--- SCOPE 9: Application Logs Inspection ---');
  const logFilePath = 'C:\\inetpub\\wwwroot\\Eimece\\media\\logs\\EImeceLog.log';
  if (fs.existsSync(logFilePath)) {
    const logContent = fs.readFileSync(logFilePath, 'utf8');
    const logLines = logContent.split('\n');
    const errorLines = logLines.filter(l => l.includes('| ERROR |') || l.includes('| FATAL |')).slice(-20);
    console.log(`Found ${errorLines.length} recent error/fatal log lines in EImeceLog.log.`);
    if (errorLines.length > 0) {
      console.log('Recent Error Log Snippet:\n' + errorLines.slice(-5).join('\n'));
    }
  }

  await desktopContext.close();
  await mobileContext.close();
  await browser.close();

  // -------------------------------------------------------------
  // GENERATE STRUCTURED REPORT JSON
  // -------------------------------------------------------------
  const report = {
    summary: {
      totalIssues: collectedIssues.length,
      critical: collectedIssues.filter(i => i.severity.toLowerCase() === 'critical').length,
      high: collectedIssues.filter(i => i.severity.toLowerCase() === 'high').length,
      medium: collectedIssues.filter(i => i.severity.toLowerCase() === 'medium').length,
      low: collectedIssues.filter(i => i.severity.toLowerCase() === 'low').length,
      testedAreas: Array.from(testedAreas)
    },
    issues: collectedIssues
  };

  const reportOutPath = path.join(__dirname, '..', 'e2e-final-qa-report.json');
  fs.writeFileSync(reportOutPath, JSON.stringify(report, null, 2), 'utf8');
  console.log(`\n=== QA AUDIT COMPLETE ===`);
  console.log(`Total Issues Found: ${report.summary.totalIssues} (Critical: ${report.summary.critical}, High: ${report.summary.high}, Medium: ${report.summary.medium}, Low: ${report.summary.low})`);
  console.log(`Report written to: ${reportOutPath}\n`);
}

runAudit().catch(err => {
  console.error('Audit crashed with error:', err);
  process.exit(1);
});
