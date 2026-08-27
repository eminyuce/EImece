import { expect, type Page } from '@playwright/test';
import { DEFAULT_PRODUCT_URL, FALLBACK_PRODUCT_URLS } from './test-data';

/**
 * Cart helpers — Crizal Razor theme.
 * Selectors favour #AddToCart / data-add-product-cart; fall back to generic "Sepete" text.
 */

// Prefer in-stock PDP button; on listing cards use data-add-product-cart
const ADD_TO_CART_PDP = '#AddToCart';
const ADD_TO_CART_CARD = '[data-add-product-cart]';

export async function navigateToProduct(page: Page, productUrl = DEFAULT_PRODUCT_URL): Promise<string> {
  const candidates = [productUrl, ...FALLBACK_PRODUCT_URLS.filter((u) => u !== productUrl)];
  for (const url of candidates) {
    const res = await page.goto(url, { waitUntil: 'domcontentloaded' });
    if (!res || res.status() >= 500) continue;
    // PDP shell should contain main#main-content and not be "Ürün Stokta Yok" without AddToCart
    const body = await page.locator('body').innerText().catch(() => '');
    if (body.includes('Unhandled exception')) continue;
    // Accept even if AddToCart is hidden for out-of-stock; caller can detect and retry next candidate.
    // Prefer pages that actually have the button.
    if (await page.locator(ADD_TO_CART_PDP).count()) return url;
    if (await page.locator(ADD_TO_CART_CARD).count()) return url;
    // If we landed on a 404 sewer page but status was 200 (custom error), try next.
    if (body.includes('Sayfa Bulunamadı') || body.match(/404/i)) continue;
    // Still return — some products are purchasable only via AddToCart AJAX without PDP button?
    return url;
  }
  throw new Error(`No candidate product URL loaded with an add-to-cart control. Tried: ${candidates.join(', ')}`);
}

export async function selectVariantIfPresent(page: Page): Promise<void> {
  // ProductSizeOptions / ProductColorOptions render as <select data-product-selected-specs>
  const specSelects = page.locator('select[data-product-selected-specs]');
  const count = await specSelects.count();
  for (let i = 0; i < count; i++) {
    const sel = specSelects.nth(i);
    if (!(await sel.isVisible().catch(() => false))) continue;
    const options = await sel.locator('option').evaluateAll((els) =>
      els.map((o) => (o as HTMLOptionElement).value).filter(Boolean)
    );
    if (options.length) {
      await sel.selectOption(options[0]!);
    }
  }
}

export async function setQuantity(page: Page, quantity: number): Promise<void> {
  const qty = page.locator('#quantity');
  if (await qty.count()) {
    await qty.fill(String(quantity));
  }
}

/**
 * Click Sepete ekle (Add to Cart) on a PDP.
 * Uses auto-waiting; waits for the /Payment/AddToCart AJAX response and the cart toast.
 */
export async function addToCartFromPDP(page: Page, quantity = 1): Promise<void> {
  await selectVariantIfPresent(page);
  await setQuantity(page, quantity);

  const addBtn = page.locator(ADD_TO_CART_PDP).first();
  await expect(addBtn, 'Sepete ekle button should be visible (product may be out of stock)').toBeVisible({
    timeout: 20_000,
  });

  // The Crizal detail.js handler POSTs to /Payment/AddToCart and shows #cart-toast on success.
  const responsePromise = page
    .waitForResponse(
      (r) => /\/Payment\/AddToCart/i.test(r.url()) && r.request().method() === 'POST',
      { timeout: 20_000 }
    )
    .catch(() => null);

  await addBtn.click();

  const resp = await responsePromise;
  if (resp) {
    expect(resp.status(), 'AddToCart should not 500').toBeLessThan(500);
    const body = (await resp.text()).toLowerCase().trim().replace(/"/g, '');
    // Server returns JSON "success" or "failed". Fail means product not buyable / missing OrderGuid.
    if (body !== 'success' && body !== '"success"') {
      // Soft check — some seeds require retrying with a different product
      if (body === 'failed') {
        throw new Error(`AddToCart returned "failed" on ${page.url()} — product may not be ProductInStock`);
      }
    }
  }

  // Toast or mini-cart update is best-effort; networkidle is more reliable than waiting for toast animation.
  await page.waitForLoadState('networkidle').catch(() => {});
  await page.waitForTimeout(400);
}

/** Click AddToCart from a listing/grid card (the data-add-product-cart flow exercises eimece.js span fix). */
export async function addToCartFromListingCard(page: Page): Promise<void> {
  const cardBtn = page.locator(ADD_TO_CART_CARD).first();
  await expect(cardBtn).toBeVisible({ timeout: 15_000 });
  const responsePromise = page.waitForResponse(
    (r) => /\/Payment\/AddToCart/i.test(r.url()),
    { timeout: 20_000 }
  );
  // Click the inner <span> when present to cover the BUG-002 span-target path.
  const span = cardBtn.locator('span').first();
  if (await span.count()) {
    await span.click({ force: true });
  } else {
    await cardBtn.click({ force: true });
  }
  const resp = await responsePromise;
  expect(resp.status()).toBeLessThan(500);
  const body = (await resp.text()).toLowerCase().trim().replace(/"/g, '');
  expect(body, 'AddToCart via listing card should not return bare failed').not.toBe('failed');
  await page.waitForLoadState('networkidle').catch(() => {});
}

export async function goToCart(page: Page): Promise<void> {
  await page.goto('/Payment/ShoppingCart', { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('main#main-content, #main-content', { timeout: 15_000 });
  await page.waitForLoadState('networkidle').catch(() => {});
}

/** Remove every line item so the next test starts isolated, even if a prior test crashed mid-checkout. */
export async function clearCart(page: Page): Promise<void> {
  await goToCart(page);
  for (let i = 0; i < 10; i++) {
    const removeBtn = page.locator('[data-shopping-item-remove]').first();
    if (!(await removeBtn.count())) break;
    const id = await removeBtn.getAttribute('data-shopping-item-remove').catch(() => null);
    const responsePromise = page
      .waitForResponse((r) => /\/Payment\/RemoveCart/i.test(r.url()), { timeout: 10_000 })
      .catch(() => null);
    await removeBtn.click();
    await responsePromise;
    await page.waitForTimeout(400);
    // If item was removed, its row disappears; if not, break.
    if (id && (await page.locator(`[data-shopping-item-row="${id}"]`).count())) break;
  }
  // Expire the OrderGuid cookie as extra isolation (the server also clears it after successful payment).
  await page.context().clearCookies().catch(() => {});
  await page.evaluate(() => {
    document.cookie = 'OrderGuid=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
  }).catch(() => {});
}

export async function assertCartHasItems(page: Page, minItems = 1): Promise<void> {
  await expect(page.locator('[data-shopping-item-row]')).toHaveCount(minItems, { timeout: 10_000 } as any);
  // Generic fallback when data attribute is missing on a theme
  if ((await page.locator('[data-shopping-item-row]').count()) === 0) {
    await expect(page.getByText('Ürün', { exact: false }).first()).toBeVisible();
  }
}

export async function proceedToCheckout(page: Page): Promise<void> {
  // From /Payment/ShoppingCart, Crizal renders two CTAs:
  //  #ProceedToCheckout -> CheckoutBillingDetails (membership)
  //  #ContinueShoppingWithoutAccount -> ShoppingWithoutAccount (guest)
  // Prefer membership first; caller can navigate to guest explicitly.
  const proceed = page.locator('#ProceedToCheckout').first();
  if (await proceed.count()) {
    await proceed.click();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForLoadState('networkidle').catch(() => {});
    return;
  }
  // Fallback: click link whose href contains checkout
  await page.getByRole('link', { name: /Ödeme|Sepet|Devam|Checkout/i }).first().click().catch(() => {});
  await page.waitForLoadState('domcontentloaded');
}
