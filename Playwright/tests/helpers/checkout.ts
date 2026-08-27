import { expect, type Page } from '@playwright/test';
import type { BuyerInfo } from './test-data';

/**
 * Checkout billing helpers — Crizal /Payment/CheckoutBillingDetails and /Payment/ShoppingWithoutAccount flows.
 *
 * Notes:
 *  - City/Town/District are populated via AJAX (/ajax/getallcities, /ajax/gettownsbycity, etc.)
 *    so we wait for the options to appear and select the first real value (prefer Istanbul/İstanbul).
 *  - Selectors use getByLabel / getByRole first, falling back to IDs that exist in the Razor views.
 */

export const KNOWN_CUSTOMER = {
  email: process.env.EIMECE_CUSTOMER_EMAIL || 'eminyuce+e2e.mt43xsz3.q8gr@outlook.com',
  password: process.env.EIMECE_CUSTOMER_PASSWORD || 'Y39KbqeM',
};
// Fallback legacy account kept for local runs where the outlook address may not exist
export const LEGACY_CUSTOMER = {
  email: 'eminyuce1111@gmail.com',
  password: 'V02y.qcF',
};

/** Try to log in with the known customer account; returns true if login succeeded (left the login page). */
export async function loginAsKnownCustomer(page: Page): Promise<boolean> {
  await login(page, KNOWN_CUSTOMER.email, KNOWN_CUSTOMER.password, '/Account/Login');
  // login() leaves us either on success page or still on /Account/Login if failed
  const url = page.url();
  const onLogin = /\/account\/login/i.test(url);
  if (onLogin) {
    const err = await page.locator('.alert-danger, .validation-summary-errors, .text-danger').first().textContent().catch(() => '');
    // If error indicates bad password / user, return false so caller can fall back to register
    if (err && /geçersiz|hatalı|invalid|failed/i.test(err)) return false;
    // Still on login but maybe captcha blocked — consider failure
    return !onLogin;
  }
  return true;
}

/** Ensure we have an authenticated session: try known customer, then legacy, then fresh registration. */
export async function ensureAuthenticated(page: Page, buyer: BuyerInfo, password: string, returnUrl = ''): Promise<void> {
  await page.goto('/Account/Login', { waitUntil: 'domcontentloaded' });
  if (!/\/account\/login/i.test(page.url())) return; // already authenticated (cookie persists)
  const tryLogin = async (email: string, pwd: string) => {
    await login(page, email, pwd, '/Account/Login');
    return !/\/account\/login/i.test(page.url());
  };
  if (await tryLogin(KNOWN_CUSTOMER.email, KNOWN_CUSTOMER.password).catch(() => false)) return;
  if (await tryLogin(LEGACY_CUSTOMER.email, LEGACY_CUSTOMER.password).catch(() => false)) return;
  await registerIfNeeded(page, buyer, password, returnUrl);
}

/** Register a new membership user (bypasses guest checkout). Uses Captcha-free path when CaptchaProvider=None. */
export async function registerIfNeeded(page: Page, buyer: BuyerInfo, password: string, returnUrl = ''): Promise<void> {
  const registerPath = returnUrl
    ? `/Account/Register?returnUrl=${encodeURIComponent(returnUrl)}`
    : '/Account/Register';
  await page.goto(registerPath, { waitUntil: 'domcontentloaded' });

  // Already logged in? The server redirects away from Register.
  if (!/\/account\/register/i.test(page.url())) return;

  await page.getByLabel(/Ad\b|FirstName/i).first().fill(buyer.name).catch(async () => {
    await page.locator('#FirstName, input[name="FirstName"]').first().fill(buyer.name);
  });
  await page.getByLabel(/Soyad|LastName/i).first().fill(buyer.surname).catch(async () => {
    await page.locator('#LastName, input[name="LastName"]').first().fill(buyer.surname);
  });
  await page.locator('#Email, input[name="Email"]').first().fill(buyer.email);
  await page.locator('#PhoneNumber, input[name="PhoneNumber"]').first().fill(buyer.gsmNumber.replace(/\D/g, '').slice(-10));
  await page.locator('#Password, input[name="Password"]').first().fill(password);
  await page.locator('#ConfirmPassword, input[name="ConfirmPassword"]').first().fill(password);
  const permission = page.locator('#IsPermissionGranted, input[name="IsPermissionGranted"]').first();
  if (await permission.count()) await permission.check({ force: true }).catch(() => {});

  // Captcha handling: if visible, brute-force Legacy sums 2..8; otherwise normal submit.
  const captcha = page.locator('input[name="Captcha"], #Captcha').first();
  const hasCaptcha = (await captcha.count()) && (await captcha.isVisible().catch(() => false));
  if (!hasCaptcha) {
    // Scope to the register form — the page also has a hidden search submit (#searchSubmitButton)
    const registerForm = page.locator('form.crizal-customer-login__form, form[action*="/Account/Register"]').first();
    const submit = registerForm.locator('button.crizal-customer-login__submit, button[type="submit"]:visible').first();
    // Wait for navigation after register POST (success leaves /Account/Register)
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 30_000 }).catch(() => {}),
      submit.click(),
    ]);
    // Also wait for load state in case navigation already happened
    await page.waitForLoadState('domcontentloaded').catch(() => {});
    return;
  }

  for (let attempt = 0; attempt < 16; attempt++) {
    const answer = String((attempt % 7) + 2);
    await page.goto(registerPath, { waitUntil: 'domcontentloaded' });
    await page.locator('#FirstName, input[name="FirstName"]').first().fill(buyer.name);
    await page.locator('#LastName, input[name="LastName"]').first().fill(buyer.surname);
    await page.locator('#Email, input[name="Email"]').first().fill(buyer.email);
    await page.locator('#PhoneNumber, input[name="PhoneNumber"]').first().fill(buyer.gsmNumber.replace(/\D/g, '').slice(-10));
    await page.locator('#Password, input[name="Password"]').first().fill(password);
    await page.locator('#ConfirmPassword, input[name="ConfirmPassword"]').first().fill(password);
    const perm2 = page.locator('#IsPermissionGranted, input[name="IsPermissionGranted"]').first();
    if (await perm2.count()) await perm2.check({ force: true }).catch(() => {});
    const c = page.locator('input[name="Captcha"], #Captcha').first();
    if (await c.count()) await c.fill(answer);
    const submit2 = page
      .locator('form.crizal-customer-login__form button.crizal-customer-login__submit, form.crizal-customer-login__form button[type="submit"]:visible')
      .first();
    await Promise.all([page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 5_000 }).catch(() => {}), submit2.click().catch(async () => {
      await page.locator('form.crizal-customer-login__form').first().evaluate((f: HTMLFormElement) => f.requestSubmit()).catch(() => {});
    })]);
    if (!/\/account\/register/i.test(page.url())) return;
  }
  throw new Error('Register failed — captcha or validation blocked the form');
}

export async function login(page: Page, email: string, password: string, loginPath = '/Account/Login'): Promise<void> {
  await page.goto(loginPath, { waitUntil: 'domcontentloaded' });
  const emailInput = page.locator('form.crizal-customer-login__form #Email, form[action*="/Account/Login"] #Email, #Email').first();
  await emailInput.fill(email);
  await page.locator('form.crizal-customer-login__form #Password, form[action*="/Account/Login"] #Password, #Password').first().fill(password);
  const captcha = page.locator('input[name="Captcha"], #Captcha').first();
  const hasCaptcha = (await captcha.count()) && (await captcha.isVisible().catch(() => false));
  if (!hasCaptcha) {
    const form = page.locator('form.crizal-customer-login__form, form[action*="/Account/Login"]').first();
    await form.locator('button.crizal-customer-login__submit, button[type="submit"]:visible').first().click();
    await page.waitForLoadState('domcontentloaded');
    return;
  }
  // brute-force path same as register
  for (let i = 0; i < 16; i++) {
    await captcha.fill(String((i % 7) + 2));
    const form = page.locator('form.crizal-customer-login__form, form[action*="/Account/Login"]').first();
    await form.locator('button.crizal-customer-login__submit, button[type="submit"]:visible').first().click();
    await page.waitForLoadState('domcontentloaded');
    if (!/\/account\/(login|adminlogin)/i.test(page.url())) return;
    await page.goto(loginPath, { waitUntil: 'domcontentloaded' });
    await page.locator('#Email, input[name="Email"]').first().fill(email);
    await page.locator('#Password, input[name="Password"]').first().fill(password);
  }
}

/**
 * Fill CheckoutBillingDetails (membership checkout) — the only path that reaches /Payment/PlaceOrder
 * with a real iyzico Checkout Form when IyzicoApiKey is configured. Guest ShoppingWithoutAccount
 * follows a similar schema but POSTs to a different action.
 */
export async function fillBillingDetails(page: Page, buyer: BuyerInfo): Promise<void> {
  // Billing page may redirect to /Account/Register when not authenticated — caller should have registered first.
  await page.waitForSelector('#Cities, #Towns, input[name*="Street"]', { timeout: 25_000 });

  // The Razor view uses `customer.Name` etc — IDs are `customer_Name` and names are `customer.Name`.
  // Snapshot shows labels: İsim / Soyisim / TC Kimlik Numarası / Cep Tel / Şehir / İlçe / Mahalle / Sokak / Posta kodu
  // Prefer getByLabel, fall back to any id/name variant including customer_*.
  const fillByLabelOrId = async (labelRegex: RegExp, fallbackSelectors: string, value: string) => {
    const byLabel = page.getByLabel(labelRegex).first();
    if ((await byLabel.count().catch(() => 0)) && (await byLabel.isVisible().catch(() => false))) {
      await byLabel.fill(value);
      return;
    }
    const byId = page.locator(fallbackSelectors).first();
    if (await byId.count()) {
      await byId.fill(value);
      return;
    }
    // Last resort: any input with matching name fragment
    const any = page.locator(`input[name*="${fallbackSelectors.split(',')[0].replace(/[#\[\]]/g,'').trim()}"]`).first();
    if (await any.count()) await any.fill(value).catch(()=>{});
  };

  await fillByLabelOrId(/İsim|Ad\b/, '#customer_Name, #Name, input[name="customer.Name"], input[name="Name"]', buyer.name);
  await fillByLabelOrId(/Soyisim|Soyad/, '#customer_Surname, #Surname, input[name="customer.Surname"], input[name="Surname"]', buyer.surname);
  await fillByLabelOrId(/TC Kimlik/i, '#customer_IdentityNumber, #IdentityNumber, input[name="customer.IdentityNumber"], input[name="IdentityNumber"]', buyer.identityNumber);
  await fillByLabelOrId(/Cep Tel|Gsm/i, '#customer_GsmNumber, #GsmNumber, input[name="customer.GsmNumber"], input[name="GsmNumber"]', buyer.gsmNumber);
  await fillByLabelOrId(/Sokak|Street|Adres/i, '#customer_Street, #Street, input[name="customer.Street"], input[name="Street"]', buyer.street);
  await fillByLabelOrId(/Posta kodu|Zip/i, '#customer_ZipCode, #ZipCode, input[name="customer.ZipCode"], input[name="ZipCode"]', buyer.zipCode);

  // City/Town/District are empty SelectLists hydrated via GetIller() AJAX (see _CheckoutBillingDetails scripts).
  // We wait for at least one option, then select Istanbul first, else first non-empty.
  await page.waitForTimeout(600); // allow GetIller() to fire
  await page.waitForFunction(
    () => document.querySelectorAll('#Cities option').length > 1,
    { timeout: 20_000 }
  ).catch(() => {});

  const cities = page.locator('#Cities');
  await cities.waitFor({ state: 'visible', timeout: 10_000 }).catch(() => {});
  const citySelected = await selectFirstMatchingOption(cities, [/istanbul/i, /ankara/i]);
  await page.waitForTimeout(700);

  // Town is populated after city change (GetTowns)
  const towns = page.locator('#Towns');
  await page.waitForFunction(
    () => document.querySelectorAll('#Towns option').length > 1,
    { timeout: 20_000 }
  ).catch(() => {});
  await towns.waitFor({ state: 'visible', timeout: 10_000 }).catch(() => {});
  await selectFirstAvailableOption(towns);
  await page.waitForTimeout(600);

  const districts = page.locator('#Districts');
  if (await districts.count()) {
    await page.waitForFunction(
      () => document.querySelectorAll('#Districts option').length > 1,
      { timeout: 10_000 }
    ).catch(() => {});
    await selectFirstAvailableOption(districts).catch(() => {});
  }

  // Verify selections stuck (city required by server validation)
  const cityVal = await cities.inputValue().catch(() => '');
  if (!cityVal) {
    // Force-select first non-empty value as last resort
    await selectFirstAvailableOption(cities);
  }

  // Submit — Crizal uses a hidden submit #ReviewYourOrder triggered by a decorated button
  const hiddenSubmit = page.locator('#ReviewYourOrder');
  const nextBtn = page.getByRole('button', { name: /Siparişi|Ödeme|İleri|Next|Review/i }).first();

  // Some builds show "Aynı adres" checkbox; keep default checked.
  const sameAsShipping = page.locator('#sameAsShipping');
  if (await sameAsShipping.count()) {
    if (!(await sameAsShipping.isChecked().catch(() => true))) {
      await sameAsShipping.check({ force: true }).catch(() => {});
    }
  }

  if (await hiddenSubmit.count()) {
    await Promise.all([
      page.waitForLoadState('domcontentloaded'),
      hiddenSubmit.evaluate((el: HTMLElement) => el.click()),
    ]);
  } else if (await nextBtn.count()) {
    await Promise.all([page.waitForLoadState('domcontentloaded'), nextBtn.click()]);
  } else {
    await page.locator('form[action*="CheckoutBillingDetails"] input[type="submit"], form[action*="CheckoutBillingDetails"] button[type="submit"]').first().click();
    await page.waitForLoadState('domcontentloaded');
  }

  // After POST we should land on CheckoutPaymentOrderReview — wait for either that or validation errors to settle.
  await page.waitForLoadState('networkidle').catch(() => {});
  const url = page.url();
  // If we stayed on CheckoutBillingDetails, surface validation errors for diagnostics.
  if (/checkoutbillingdetails/i.test(url)) {
    const errors = await page.locator('.text-danger, .field-validation-error').allTextContents().catch(() => []);
    const visibleErrors = errors.map((s) => s.trim()).filter(Boolean);
    if (visibleErrors.length) {
      throw new Error(`Billing validation failed: ${visibleErrors.join(' | ')} | url=${url}`);
    }
  }
}

async function selectFirstMatchingOption(select: ReturnType<Page['locator']>, patterns: RegExp[]): Promise<boolean> {
  const options = await select.locator('option').all();
  for (const pat of patterns) {
    for (const opt of options) {
      const text = (await opt.textContent())?.trim() ?? '';
      const value = (await opt.getAttribute('value')) ?? '';
      if (!value) continue;
      if (pat.test(text)) {
        await select.selectOption({ label: text });
        return true;
      }
    }
  }
  return selectFirstAvailableOption(select);
}

async function selectFirstAvailableOption(select: ReturnType<Page['locator']>): Promise<boolean> {
  const values: string[] = await select.locator('option').evaluateAll((els: HTMLOptionElement[]) =>
    els.map((o) => o.value).filter((v) => v && v !== '0')
  );
  if (!values.length) return false;
  await select.selectOption(values[0]!);
  return true;
}

/** From CheckoutPaymentOrderReview, click through to /Payment/PlaceOrder (where the iyzico form lives). */
export async function goToPlaceOrder(page: Page): Promise<void> {
  if (/placeorder/i.test(page.url())) return;
  // Crizal order review has a single primary CTA linking to PlaceOrder
  const link = page.locator('a[href*="PlaceOrder"], a[href*="placeorder"]').first();
  if (await link.count()) {
    await Promise.all([
      page.waitForURL(/placeorder/i, { timeout: 30_000 }).catch(() => {}),
      link.click(),
    ]);
    await page.waitForLoadState('domcontentloaded');
    await page.waitForLoadState('networkidle').catch(() => {});
    return;
  }
  await page.goto('/Payment/PlaceOrder', { waitUntil: 'domcontentloaded' });
  await page.waitForLoadState('networkidle').catch(() => {});
}

export async function expectOnBillingOrReview(page: Page): Promise<void> {
  await expect(page).toHaveURL(/CheckoutBillingDetails|CheckoutPaymentOrderReview|PlaceOrder/i, { timeout: 20_000 });
}
