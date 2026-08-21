/**
 * Customer shopping-flow helpers for Crizal E2E (guest → register → cart → checkout).
 */
const fs = require('fs');
const path = require('path');
const {
  assertCrizalChrome,
  loginWithPassword,
  submitWithLegacyCaptchaBruteForce,
} = require('./helpers');

/** Prefer known in-stock products (many demo SKUs are OutOfStock). */
const PRODUCT_CATEGORY = '/c/pc/oturma-grubu-4h3f4h1b/';
const PRODUCT_DETAIL = '/p/oturma-grubu/nordline-mese-sehpa-90cm-130-4h4h2d5i4h1b/';
const PRODUCT_DETAIL_ALT =
  '/p/oturma-grubu/nordline-mese-sehpa-90cm-130-4h4h2d5i4h1b/';

/** Official iyzico sandbox success card (Akbank commercial MasterCard). */
const IYZICO_SUCCESS_CARD = {
  number: '5526080000000006',
  expireMonth: '12',
  expireYear: '2030',
  cvc: '123',
  holder: 'EIMECE TEST',
};

const VIEWPORTS = {
  desktop: { width: 1440, height: 900 },
  mobile: { width: 390, height: 844 },
};

function uniqueCustomerEmail(prefix = 'e2e') {
  const stamp = Date.now().toString(36);
  const rand = Math.random().toString(36).slice(2, 6);
  // Real inbox for confirmation / order mail during happy-path runs when prefix is outlook.
  if (prefix === 'outlook') {
    return `eminyuce+e2e.${stamp}.${rand}@outlook.com`;
  }
  return `${prefix}.${stamp}.${rand}@eimece.test`;
}

function screenshotDir() {
  const dir = path.join(__dirname, '..', 'screenshots', 'customer-e2e');
  fs.mkdirSync(dir, { recursive: true });
  return dir;
}

/**
 * @param {import('@playwright/test').Page} page
 * @param {string} name
 * @param {'desktop'|'mobile'} viewport
 */
async function shot(page, name, viewport = 'desktop') {
  const dir = screenshotDir();
  const file = path.join(dir, `${viewport}-${name}.png`);
  await page.screenshot({ path: file, fullPage: true });
  return file;
}

/**
 * Capture the same step on desktop + mobile by resizing the current page.
 * @param {import('@playwright/test').Page} page
 * @param {string} name
 */
async function shotBoth(page, name) {
  const desktop = VIEWPORTS.desktop;
  const mobile = VIEWPORTS.mobile;
  await page.setViewportSize(desktop);
  await page.waitForTimeout(200);
  await shot(page, name, 'desktop');
  await page.setViewportSize(mobile);
  await page.waitForTimeout(200);
  await shot(page, name, 'mobile');
  await page.setViewportSize(desktop);
}

async function openProductDetail(page, productPath = PRODUCT_DETAIL) {
  await page.goto(productPath, { waitUntil: 'domcontentloaded' });
  await assertCrizalChrome(page);
  const addBtn = page.locator('#AddToCart');
  if (!(await addBtn.count())) {
    // Fallback to alternate in-stock SKU
    if (productPath !== PRODUCT_DETAIL_ALT) {
      await page.goto(PRODUCT_DETAIL_ALT, { waitUntil: 'domcontentloaded' });
      await assertCrizalChrome(page);
    }
  }
  await page.waitForSelector('#AddToCart', { timeout: 20_000 });
}

async function addToCartFromDetail(page, { quantity = 1 } = {}) {
  const addBtn = page.locator('#AddToCart').first();
  if (!(await addBtn.count())) {
    throw new Error(`AddToCart button missing on ${page.url()} (product likely out of stock)`);
  }
  const qty = page.locator('#quantity');
  if (await qty.count()) {
    await qty.fill(String(quantity));
  }
  const responsePromise = page
    .waitForResponse(
      (r) => /addtocart|getshoppingcart|shoppingcart/i.test(r.url()) && r.status() < 500,
      { timeout: 20_000 }
    )
    .catch(() => null);
  await addBtn.click();
  await responsePromise;
  await page.waitForTimeout(500);
}

async function goToCart(page) {
  await page.goto('/Payment/ShoppingCart', { waitUntil: 'domcontentloaded' });
  await assertCrizalChrome(page);
}

async function updateFirstCartQuantity(page, quantity) {
  const qtyInput = page.locator('input[name*="Quantity"], input.quantity, input[type="number"]').first();
  if (!(await qtyInput.count())) return false;
  await qtyInput.fill(String(quantity));
  const updateBtn = page
    .locator('button:has-text("Güncelle"), a:has-text("Güncelle"), button[onclick*="Update"], #UpdateCart')
    .first();
  if (await updateBtn.count()) {
    await updateBtn.click();
    await page.waitForLoadState('domcontentloaded');
    return true;
  }
  // Many Crizal carts update on change/blur via AJAX
  await qtyInput.blur();
  await page.waitForTimeout(800);
  return true;
}

async function removeFirstCartItem(page) {
  const remove = page
    .locator(
      'a:has-text("Kaldır"), button:has-text("Kaldır"), a:has-text("Sil"), button:has-text("Sil"), a[href*="Remove"], button[onclick*="Remove"], .remove-from-cart, [data-action="remove"]'
    )
    .first();
  if (!(await remove.count())) return false;
  await remove.click();
  await page.waitForLoadState('domcontentloaded');
  await page.waitForTimeout(500);
  return true;
}

/**
 * Register a new customer. Prefers CaptchaProvider=None; falls back to Legacy brute-force.
 */
async function registerCustomer(page, { email, password = 'Test123!', firstName = 'E2E', lastName = 'Customer', phone = '5551234567', returnUrl = '' } = {}) {
  const registerPath = returnUrl
    ? `/Account/Register?returnUrl=${encodeURIComponent(returnUrl)}`
    : '/Account/Register';
  await page.goto(registerPath, { waitUntil: 'domcontentloaded' });
  await assertCrizalChrome(page);

  const fillForm = async () => {
    await page.locator('#FirstName, input[name="FirstName"]').first().fill(firstName);
    await page.locator('#LastName, input[name="LastName"]').first().fill(lastName);
    await page.locator('#Email, input[name="Email"]').first().fill(email);
    await page.locator('#PhoneNumber, input[name="PhoneNumber"]').first().fill(phone);
    await page.locator('#Password, input[name="Password"]').first().fill(password);
    await page.locator('#ConfirmPassword, input[name="ConfirmPassword"]').first().fill(password);
    const perm = page.locator('#IsPermissionGranted, input[name="IsPermissionGranted"]').first();
    if (await perm.count()) {
      await perm.check({ force: true }).catch(() => {});
    }
  };

  const registerForm = () =>
    page.locator('form.form-horizontal, form[action*="Register"], main form[method="post"]').filter({
      has: page.locator('#FirstName, input[name="FirstName"]'),
    }).first();

  const submitRegister = async () => {
    const form = registerForm();
    const submit = form
      .locator(
        'button.crizal-customer-login__submit, button[type="submit"].butn-style8, button[type="submit"], input[type="submit"]'
      )
      .first();
    await Promise.all([page.waitForLoadState('domcontentloaded'), submit.click()]);
  };

  await fillForm();
  const captchaVisible = await registerForm()
    .locator('input[name="Captcha"], #Captcha')
    .first()
    .isVisible()
    .catch(() => false);
  if (!captchaVisible) {
    await submitRegister();
  } else {
    const ok = await submitWithLegacyCaptchaBruteForce(
      page,
      async () => {
        await page.goto(registerPath, { waitUntil: 'domcontentloaded' });
        await fillForm();
      },
      async (p) => !/\/account\/register/i.test(p.url())
    );
    if (!ok) return false;
  }

  // Success: leave register page (returnUrl, customers area, or billing)
  return !/\/account\/register/i.test(page.url());
}

async function proceedToMembershipCheckout(page) {
  await goToCart(page);
  const proceed = page.locator('#ProceedToCheckout').first();
  await proceed.click();
  await page.waitForLoadState('domcontentloaded');
}

/**
 * Fill CheckoutBillingDetails with Istanbul / first available town.
 */
async function fillBillingDetails(page, { name = 'E2E', surname = 'Customer', gsm = '5551234567', identity = '11111111111' } = {}) {
  await page.waitForSelector('#Cities option', { timeout: 25_000 }).catch(() => {});
  await page.waitForTimeout(800);

  const nameInput = page.locator('#Name, input[name="Name"]').first();
  if (await nameInput.count()) await nameInput.fill(name);
  const surnameInput = page.locator('#Surname, input[name="Surname"]').first();
  if (await surnameInput.count()) await surnameInput.fill(surname);
  const gsmInput = page.locator('#GsmNumber, input[name="GsmNumber"]').first();
  if (await gsmInput.count()) await gsmInput.fill(gsm);
  const idInput = page.locator('#IdentityNumber, input[name="IdentityNumber"]').first();
  if (await idInput.count()) await idInput.fill(identity);
  const street = page.locator('#Street, input[name="Street"]').first();
  if (await street.count()) await street.fill('Test Mah. E2E Sok. No:1');
  const zip = page.locator('#ZipCode, input[name="ZipCode"]').first();
  if (await zip.count()) await zip.fill('34000');

  const cities = page.locator('#Cities');
  await cities.waitFor({ state: 'visible', timeout: 20_000 });
  // Prefer Istanbul if present
  const cityOptions = await cities.locator('option').allTextContents();
  const istanbul = cityOptions.find((t) => /istanbul/i.test(t));
  if (istanbul) {
    await cities.selectOption({ label: istanbul.trim() });
  } else {
    const values = await cities.locator('option').evaluateAll((opts) =>
      opts.map((o) => o.value).filter((v) => v)
    );
    if (values.length) await cities.selectOption(values[0]);
  }
  await page.waitForTimeout(1000);

  const towns = page.locator('#Towns');
  await towns.waitFor({ state: 'visible', timeout: 15_000 });
  const townValues = await towns.locator('option').evaluateAll((opts) =>
    opts.map((o) => o.value).filter((v) => v)
  );
  if (townValues.length) {
    await towns.selectOption(townValues[0]);
    await page.waitForTimeout(800);
  }

  const districts = page.locator('#Districts');
  if (await districts.count()) {
    const distValues = await districts.locator('option').evaluateAll((opts) =>
      opts.map((o) => o.value).filter((v) => v)
    );
    if (distValues.length) await districts.selectOption(distValues[0]);
  }

  // Submit via hidden ReviewYourOrder or visible next button
  const review = page.locator('#ReviewYourOrder');
  if (await review.count()) {
    await Promise.all([
      page.waitForLoadState('domcontentloaded'),
      review.evaluate((el) => el.click()),
    ]);
  } else {
    await page.locator('input[type="submit"], button[type="submit"]').first().click();
    await page.waitForLoadState('domcontentloaded');
  }
}

async function placeOrderIfReady(page) {
  // From order review → PlaceOrder (avoid matching breadcrumb "Ödeme" text)
  if (/placeorder/i.test(page.url())) return;
  const placeLink = page.locator('a[href*="/Payment/PlaceOrder"], a[href*="/payment/placeorder"]').first();
  if (await placeLink.count()) {
    await Promise.all([
      page.waitForURL(/placeorder/i, { timeout: 45_000 }).catch(() => {}),
      placeLink.click(),
    ]);
    await page.waitForLoadState('domcontentloaded');
    return;
  }
  await page.goto('/Payment/PlaceOrder', { waitUntil: 'domcontentloaded' });
}

/**
 * Attempt to complete iyzico checkout form (iframe). Returns status string.
 */
async function tryPayWithIyzicoSandbox(page, card = IYZICO_SUCCESS_CARD) {
  try {
    if (await page.locator('.alert-warning').filter({ hasText: /ödeme formu/i }).count()) {
      return { ok: false, reason: 'iyzico form empty — set IyzicoApiKey/IyzicoSecretKey (sandbox) on IIS' };
    }

    await page.waitForSelector('text=Sandbox', { timeout: 45_000 }).catch(() => {});
    await page.locator('iframe').first().waitFor({ state: 'attached', timeout: 45_000 }).catch(() => {});
    await page.waitForTimeout(2000);

    const findPaymentFrame = async () => {
      for (const frame of page.frames()) {
        if (frame === page.mainFrame()) continue;
        const count = await frame
          .locator('input[type="tel"], input[type="text"], input:not([type="hidden"])')
          .count()
          .catch(() => 0);
        if (count >= 3) return frame;
      }
      return null;
    };

    let frame = await findPaymentFrame();
    for (let i = 0; !frame && i < 15; i++) {
      await page.waitForTimeout(1000);
      frame = await findPaymentFrame();
    }
    if (!frame) {
      return { ok: false, reason: 'iyzico payment iframe not found' };
    }

    const visibleInputs = frame.locator('input:visible');
    const inputCount = await visibleInputs.count();

    // iyzico TR sandbox placeholders observed: "Kart Üzerindeki Ad Soyad", "Kart Numarası", "Ay / Yıl", "CVC"
    const holder = frame.getByPlaceholder(/ad soyad|card holder|name on card|name/i).first();
    const number = frame.getByPlaceholder(/kart numar|card number|kart no/i).first();
    const expiry = frame.getByPlaceholder(/ay\s*\/\s*yıl|mm\s*\/\s*yy|aa\s*\/\s*yy|expiry/i).first();
    const cvc = frame.getByPlaceholder(/^cvc$|^cvv$/i).first();

    const typeInto = async (locator, value) => {
      if (!(await locator.count())) return false;
      await locator.click({ force: true });
      await locator.fill('');
      await locator.pressSequentially(String(value), { delay: 25 });
      return true;
    };

    await typeInto(holder, card.holder);
    if (!(await typeInto(number, card.number))) {
      const tel = frame.locator('input[type="tel"]:visible').first();
      await typeInto(tel, card.number);
    }
    await typeInto(expiry, `${card.expireMonth}/${card.expireYear.slice(-2)}`);
    await typeInto(cvc, card.cvc);

    // Fallback: fill first N visible text/tel inputs if placeholders didn't match
    if (!(await number.count()) && inputCount >= 3) {
      const texts = [];
      for (let i = 0; i < inputCount; i++) {
        const el = visibleInputs.nth(i);
        const type = (await el.getAttribute('type')) || 'text';
        if (type === 'checkbox' || type === 'hidden' || type === 'radio') continue;
        texts.push(el);
      }
      if (texts[0]) await texts[0].fill(card.holder);
      if (texts[1]) await texts[1].type(card.number, { delay: 10 });
      if (texts[2]) await texts[2].type(`${card.expireMonth}${card.expireYear.slice(-2)}`, { delay: 10 });
      if (texts[3]) await texts[3].type(card.cvc, { delay: 10 });
    }

    const terms = frame.locator('input[type="checkbox"]').first();
    if (await terms.count()) await terms.check({ force: true }).catch(() => {});

    await page.waitForTimeout(800);
    const payBtn = frame.locator('button:has-text("ÖDE"), button:has-text("Öde"), button[type="submit"]').first();
    await payBtn.waitFor({ state: 'visible', timeout: 15_000 });
    // Wait until enabled when possible
    for (let i = 0; i < 20; i++) {
      const disabled = await payBtn.isDisabled().catch(() => false);
      if (!disabled) break;
      await page.waitForTimeout(400);
    }
    await payBtn.click({ force: true, timeout: 15_000 });

    await page.waitForLoadState('domcontentloaded').catch(() => {});
    await page.waitForTimeout(4000);
    await page.waitForURL(/PaymentResult|ThankYou|basar|success|siparis/i, { timeout: 90_000 }).catch(() => {});

    const url = page.url();
    const body = await page.locator('body').innerText().catch(() => '');
    const ok = /PaymentResult|ThankYou|sipariş|başar|success|order|teşekkür/i.test(url + body);
    return { ok, reason: ok ? 'payment completed' : `left on ${url}` };
  } catch (err) {
    return { ok: false, reason: `iyzico payment error: ${err && err.message ? err.message : err}` };
  }
}

module.exports = {
  PRODUCT_CATEGORY,
  PRODUCT_DETAIL,
  PRODUCT_DETAIL_ALT,
  IYZICO_SUCCESS_CARD,
  VIEWPORTS,
  uniqueCustomerEmail,
  screenshotDir,
  shot,
  shotBoth,
  openProductDetail,
  addToCartFromDetail,
  goToCart,
  updateFirstCartQuantity,
  removeFirstCartItem,
  registerCustomer,
  proceedToMembershipCheckout,
  fillBillingDetails,
  placeOrderIfReady,
  tryPayWithIyzicoSandbox,
  loginWithPassword,
};
