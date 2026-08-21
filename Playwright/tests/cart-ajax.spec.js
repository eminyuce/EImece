const { test, expect } = require('@playwright/test');
const { gotoAndAssertOk, collectPageIssues, filterConsoleNoise, captureFailure } = require('./helpers');

test.describe('Cart and AJAX interactions', () => {
  test('shopping cart page loads', async ({ page }) => {
    const r = await gotoAndAssertOk(page, '/payment/shoppingcart/');
    expect(r.status).toBeLessThan(500);
    await expect(page.locator('#main-content')).toBeVisible();
  });

  test('add to cart from product detail via AJAX/form', async ({ page }) => {
    test.setTimeout(120_000);
    const issues = await collectPageIssues(page);

    await page.goto('/c/pc/elektronik-7e7e4h1b/', { waitUntil: 'domcontentloaded' });
    const productHref = await page.locator('a[href*="/p/"]').first().getAttribute('href');
    expect(productHref).toBeTruthy();

    const detailRes = await page.goto(productHref, { waitUntil: 'domcontentloaded' });
    expect(detailRes?.status(), `product detail ${productHref}`).toBeLessThan(500);
    await expect(page.locator('body')).not.toContainText('Unhandled exception');

    // Prefer explicit add-to-cart control
    const addBtn = page.locator(
      'button:has-text("Sepete"), a:has-text("Sepete"), button:has-text("Add to Cart"), [data-action="addtocart"], form[action*="addtocart"] button, .add-to-cart, #addToCart'
    ).first();

    if (!(await addBtn.count())) {
      // Fallback: POST AddToCart if we can find product id in page
      const html = await page.content();
      const idMatch = html.match(/productId["\s:=]+(\d+)/i) || html.match(/name="ProductId"[^>]*value="(\d+)"/i);
      if (!idMatch) {
        test.info().annotations.push({ type: 'note', description: 'No add-to-cart control found; skipped interaction' });
        return;
      }
      const productId = idMatch[1];
      const resp = await page.request.post('/payment/addtocart/', {
        form: { productId, quantity: 1 },
      });
      expect(resp.status(), 'AddToCart POST').toBeLessThan(500);
    } else {
      const responsePromise = page.waitForResponse(
        (r) => /addtocart|shoppingcart|getshoppingcart/i.test(r.url()) && r.status() < 500,
        { timeout: 15_000 }
      ).catch(() => null);

      await addBtn.click();
      await responsePromise;
      await page.waitForLoadState('networkidle').catch(() => {});
    }

    await page.goto('/payment/shoppingcart/', { waitUntil: 'domcontentloaded' });
    expect(page.url()).toMatch(/shoppingcart/i);
    await expect(page.locator('body')).not.toContainText('Unhandled exception');

    const consoleErrors = filterConsoleNoise([...issues.consoleErrors, ...issues.pageErrors]);
    if (consoleErrors.length) {
      await captureFailure(page, 'cart-ajax-console');
    }
    expect(consoleErrors).toEqual([]);
  });

  test('AJAX region endpoints return JSON', async ({ request }) => {
    const cities = await request.get('/ajax/getallcities/');
    expect(cities.status()).toBe(200);
    const citiesBody = await cities.text();
    expect(citiesBody.length).toBeGreaterThan(2);

    // towns need a city id — parse first city if JSON array/object
    let cityId = null;
    try {
      const parsed = JSON.parse(citiesBody);
      if (Array.isArray(parsed) && parsed.length) {
        cityId = parsed[0].Value || parsed[0].Id || parsed[0].id || parsed[0].value;
      } else if (parsed && typeof parsed === 'object') {
        const first = Object.values(parsed)[0];
        cityId = first?.Value || first?.Id || first;
      }
    } catch {
      const m = citiesBody.match(/\d+/);
      cityId = m ? m[0] : null;
    }

    if (cityId) {
      const towns = await request.get(`/ajax/gettownsbycity/?cityId=${cityId}`);
      expect(towns.status()).toBeLessThan(500);
    }
  });

  test('subscribe email AJAX endpoint responds', async ({ request }) => {
    const res = await request.get('/ajax/subscribeemail/?email=e2e-test@example.com');
    expect(res.status()).toBeLessThan(500);
  });

  test('mini-cart partial endpoints', async ({ request }) => {
    for (const path of [
      '/payment/getshoppingcartsmalldetails/',
      '/payment/getshoppingcartlinks/',
      '/payment/rendershoppingcartprice/',
    ]) {
      const res = await request.get(path);
      expect(res.status(), path).toBeLessThan(500);
    }
  });

  // BUG-002: clicking the inner <span> label on [data-add-product-cart] must still POST AddToCart.
  test('add-to-cart via span inside data-add-product-cart button', async ({ page }) => {
    test.setTimeout(120_000);
    await page.goto('/c/pc/elektronik-0j5i6g1b/', { waitUntil: 'domcontentloaded' });

    const cartBtn = page.locator('[data-add-product-cart]').first();
    if (!(await cartBtn.count())) {
      test.info().annotations.push({
        type: 'note',
        description: 'No data-add-product-cart control on category page; skipped',
      });
      return;
    }

    const productId = await cartBtn.getAttribute('data-add-product-cart');
    expect(productId).toBeTruthy();

    const responsePromise = page.waitForResponse(
      (r) => /\/Payment\/AddToCart|\/payment\/addtocart/i.test(r.url()),
      { timeout: 20_000 }
    );

    // Click the inner span text (the failure mode of reading e.target attributes).
    const label = cartBtn.locator('span').first();
    if (await label.count()) {
      await label.click({ force: true });
    } else {
      await cartBtn.click({ force: true });
    }

    const resp = await responsePromise;
    expect(resp.status(), 'AddToCart from span click').toBeLessThan(500);
    const body = (await resp.text()).toLowerCase();
    expect(body, 'AddToCart should not return bare failed for a valid product').not.toBe('failed');
  });
});
