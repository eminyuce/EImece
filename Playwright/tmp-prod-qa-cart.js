const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
const {
  uniqueCustomerEmail,
  addToCartFromDetail,
  goToCart,
  proceedToMembershipCheckout,
  registerCustomer,
  fillBillingDetails,
  placeOrderIfReady,
  tryPayWithIyzicoSandbox,
} = require('./tests/customer-flow-helpers');
const { assertCrizalChrome } = require('./tests/helpers');

const BASE = 'http://localhost:81';
const CANDIDATES = [
  '/p/bebek-bakim/mininest-bebek-bakim-seti-5li-140-4h1b7e5i4h1b/',
  '/p/bebek-bakim/mininest-bebek-bakim-seti-5li-60-0j1b7e5i4h1b/',
  '/p/bebek-bakim/mininest-bebek-bakim-seti-5li-9a8c7e5i4h1b/',
  '/p/ev--yasam/petfriend-evcil-hayvan-mama-kabi-seti-110-5i3f7e5i4h1b/',
];

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ baseURL: BASE, viewport: { width: 1440, height: 900 } });
  const page = await context.newPage();
  const result = { product: null, addToCart: false, cart: null, checkout: null, register: null, billing: null, payment: null };

  let found = null;
  for (const url of CANDIDATES) {
    await page.goto(url, { waitUntil: 'domcontentloaded' });
    if (/notfound/i.test(page.url())) continue;
    const add = await page.locator('#AddToCart').count();
    const body = await page.locator('body').innerText();
    const oos = /stokta yok|out of stock/i.test(body);
    if (add && !oos) { found = url; break; }
  }
  result.product = found || page.url();
  if (!found) {
    console.log(JSON.stringify({ ...result, error: 'no in-stock product with AddToCart' }, null, 2));
    await browser.close();
    process.exit(0);
  }

  await addToCartFromDetail(page, { quantity: 1 });
  result.addToCart = true;
  await goToCart(page);
  const cartText = await page.locator('body').innerText();
  result.cart = {
    url: page.url(),
    empty: /sepetinizde ürün bulunamadı|no product found/i.test(cartText),
    exception: /Unhandled exception/i.test(cartText),
  };
  if (result.cart.empty || result.cart.exception) {
    console.log(JSON.stringify(result, null, 2));
    await browser.close();
    return;
  }

  await goToCart(page);
  const proceed = page.locator('#ProceedToCheckout').first();
  await Promise.all([
    page.waitForURL(/register|login|CheckoutBilling|ShoppingWithoutAccount/i, { timeout: 30_000 }).catch(() => {}),
    proceed.click(),
  ]);
  await page.waitForLoadState('domcontentloaded').catch(() => {});
  result.checkout = page.url();
  const email = uniqueCustomerEmail('qa');
  if (!/\/account\/register/i.test(page.url()) && !/\/account\/login/i.test(page.url())) {
    try {
      await page.goto('/Account/Register?returnUrl=' + encodeURIComponent('/Payment/CheckoutBillingDetails'), { waitUntil: 'domcontentloaded' });
    } catch (e) {
      result.checkoutNavError = String(e.message || e);
    }
  }
  await page.locator('#FirstName, input[name="FirstName"]').first().fill('E2E');
  await page.locator('#LastName, input[name="LastName"]').first().fill('Customer');
  await page.locator('#Email, input[name="Email"]').first().fill(email);
  await page.locator('#PhoneNumber, input[name="PhoneNumber"]').first().fill('5551234567');
  await page.locator('#Password, input[name="Password"]').first().fill('Test123!');
  await page.locator('#ConfirmPassword, input[name="ConfirmPassword"]').first().fill('Test123!');
  const perm = page.locator('#IsPermissionGranted, input[name="IsPermissionGranted"]').first();
  if (await perm.count()) await perm.check({ force: true }).catch(() => {});
  const submit = page.locator('form').filter({ has: page.locator('#FirstName, input[name="FirstName"]') }).locator('button[type="submit"], input[type="submit"]').first();
  await Promise.all([page.waitForLoadState('domcontentloaded'), submit.click()]);
  const registered = !/\/account\/register/i.test(page.url());
  result.register = { ok: registered, url: page.url(), email, body: (await page.locator('body').innerText()).slice(0, 200) };
  if (!registered) {
    console.log(JSON.stringify(result, null, 2));
    await browser.close();
    return;
  }
  if (!/CheckoutBillingDetails/i.test(page.url())) {
    await page.goto('/Payment/CheckoutBillingDetails', { waitUntil: 'domcontentloaded' });
  }
  result.billing = { url: page.url(), cities: await page.locator('#Cities').count() };
  if (result.billing.cities) {
    await fillBillingDetails(page);
    await page.waitForLoadState('domcontentloaded').catch(() => {});
    result.review = page.url();
    try {
      await placeOrderIfReady(page);
    } catch (e) {
      result.placeOrderError = String(e.message || e);
      if (/checkoutpaymentorderreview|placeorder/i.test(page.url()) === false) {
        await page.goto('/Payment/PlaceOrder', { waitUntil: 'domcontentloaded' }).catch(() => {});
      }
    }
    if (/checkoutpaymentorderreview/i.test(page.url())) {
      const placeLink = page.locator('a[href*="PlaceOrder"], a[href*="placeorder"]').first();
      if (await placeLink.count()) {
        await Promise.all([
          page.waitForURL(/placeorder/i, { timeout: 45_000 }).catch(() => {}),
          placeLink.click(),
        ]);
      } else {
        await page.goto('/Payment/PlaceOrder', { waitUntil: 'domcontentloaded' }).catch(() => {});
      }
    }
    await page.waitForTimeout(1500);
    const payBody = await page.locator('body').innerText();
    const pay = await tryPayWithIyzicoSandbox(page);
    result.payment = { url: page.url(), exception: /Unhandled exception/i.test(payBody), warning: payBody.slice(0, 400), iyzico: pay };
    await page.screenshot({ path: path.join(__dirname, 'screenshots', 'prod-qa', 'placeorder.png'), fullPage: false });
  }
  console.log(JSON.stringify(result, null, 2));
  await browser.close();
})().catch((e) => { console.error(e); process.exit(1); });
