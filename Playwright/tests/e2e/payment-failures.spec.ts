import { test, expect } from '@playwright/test';
import { clearCart, navigateToProduct, addToCartFromPDP, goToCart } from '../helpers/cart';
import { fillBillingDetails, goToPlaceOrder, ensureAuthenticated } from '../helpers/checkout';
import { ERROR_CARDS, SUCCESS_CARDS, payWithIyzicoCard, assertIyzicoFormReady, getPlaceOrderInitStatus, expectPaymentError, handle3DSChallenge } from '../helpers/iyzico';
import { makeBuyerInfo, DEFAULT_TEST_PASSWORD } from '../helpers/test-data';

/**
 * iyzico failure scenarios — each card should drive the merchant to a clear failure state.
 * Covered:
 *   - Yetersiz bakiye (not sufficient funds)
 *   - Geçersiz CVC (invalid CVC2)
 *   - Kart süresi dolmuş (expired)
 *   - Kayıp kart (lost)
 *   - 3D Secure başlatılamadı
 *   - (extra) 3DS challenge cancelled by user
 *
 * Every test is independent (new buyer email + empty cart). Iyzico may surface the error either as
 *   a) a redirect to /Payment/NoSuccessForYourOrder, or
 *   b) an inline alert inside the checkout iframe, or
 *   c) a 3DS mock with a failure button.
 * expectPaymentError() accepts all three.
 */

test.describe('iyzico payment failures — sandbox', () => {
  test.setTimeout(240_000);

  test.beforeEach(async ({ page }) => {
    await page.context().clearCookies();
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    await clearCart(page).catch(() => {});
  });

  async function prepareCartAndBilling(page: any, buyer = makeBuyerInfo()): Promise<void> {
    await navigateToProduct(page);
    await addToCartFromPDP(page, 1);
    await goToCart(page);
    await ensureAuthenticated(page, buyer, DEFAULT_TEST_PASSWORD, '/Payment/CheckoutBillingDetails');
    if (!/checkoutbillingdetails/i.test(page.url())) {
      await page.goto('/Payment/CheckoutBillingDetails', { waitUntil: 'domcontentloaded' });
    }
    if (/checkoutbillingdetails/i.test(page.url())) {
      await fillBillingDetails(page, buyer);
    }
    await goToPlaceOrder(page);
    await expect(page).toHaveURL(/PlaceOrder/i, { timeout: 30_000 });

    const status = await getPlaceOrderInitStatus(page);
    if (status === 'missing_credentials') {
      test.skip(true, 'Iyzico sandbox keys not configured — skipping failure-matrix until keys are set');
    }
    await assertIyzicoFormReady(page);
  }

  test('yetersiz bakiye kartı reddedilir (4111111111111129)', async ({ page }) => {
    await test.step('prepare checkout', async () => {
      await prepareCartAndBilling(page);
    });
    await test.step('pay with insufficient-funds card', async () => {
      const card = ERROR_CARDS.insufficientFunds;
      await payWithIyzicoCard(page, card, { expect3DS: 'none' }).catch(() => {});
      await expectPaymentError(page, card.expectedError);
    });
  });

  test('geçersiz CVC kartı reddedilir (4124111111111116)', async ({ page }) => {
    await test.step('prepare checkout', async () => {
      await prepareCartAndBilling(page);
    });
    await test.step('pay with invalid CVC card', async () => {
      const card = ERROR_CARDS.invalidCvc;
      await payWithIyzicoCard(page, card, { expect3DS: 'none' }).catch(() => {});
      await expectPaymentError(page, card.expectedError);
      // CVC errors often stay inline in the iframe rather than redirecting — also accept that.
      const url = page.url();
      expect(/PlaceOrder|NoSuccess|PaymentResult/i.test(url)).toBeTruthy();
    });
  });

  test('süresi dolmuş kart reddedilir (4125111111111115)', async ({ page }) => {
    await test.step('prepare checkout', async () => {
      await prepareCartAndBilling(page);
    });
    await test.step('pay with expired card', async () => {
      const card = ERROR_CARDS.expiredCard;
      await payWithIyzicoCard(page, card, { expect3DS: 'none' }).catch(() => {});
      await expectPaymentError(page, card.expectedError);
    });
  });

  test('kayıp kart reddedilir (4127111111111113)', async ({ page }) => {
    await test.step('prepare checkout', async () => {
      await prepareCartAndBilling(page);
    });
    await test.step('pay with lost card', async () => {
      const card = ERROR_CARDS.lostCard;
      await payWithIyzicoCard(page, card, { expect3DS: 'none' }).catch(() => {});
      await expectPaymentError(page, card.expectedError);
    });
  });

  test('3D Secure başlatma hatası (4151111111111112)', async ({ page }) => {
    await test.step('prepare checkout', async () => {
      await prepareCartAndBilling(page);
    });
    await test.step('pay with 3DS-init-failed card', async () => {
      const card = ERROR_CARDS.threedInitFailed;
      await payWithIyzicoCard(page, card, { expect3DS: 'failure' }).catch(() => {});
      await expectPaymentError(page, card.expectedError);
    });
  });

  test('3DS challenge iptal edilirse sipariş oluşturulmaz', async ({ page }) => {
    const buyer = makeBuyerInfo();
    await test.step('prepare checkout', async () => {
      await prepareCartAndBilling(page, buyer);
    });
    await test.step('fill valid card but cancel 3DS', async () => {
      // Use a normal success card but simulate user cancelling the challenge.
      const card = SUCCESS_CARDS.masterAkbankKredi;
      await payWithIyzicoCard(page, card, { expect3DS: 'failure' }).catch(() => {});
      // Some sandboxes treat cancellation as a failure redirect; others keep the user on PlaceOrder.
      // Either way, ThankYou must NOT be reached.
      await page.waitForTimeout(2000);
      const url = page.url();
      expect(url, 'Cancelling 3DS should not land on ThankYou').not.toMatch(/ThankYouForYourOrder/i);
      // If the cancel path did redirect to a failure page, assert that; if it stayed on PlaceOrder, that's also valid.
      const body = await page.locator('body').innerText().catch(() => '');
      const stayedOrFailed =
        /PlaceOrder/i.test(url) ||
        /NoSuccess|PaymentResult|hata|başarısız|iptal|cancel/i.test(url + body);
      expect(stayedOrFailed, `Expected PlaceOrder or failure after 3DS cancel. url=${url}`).toBeTruthy();

      // Explicitly also try the popup/iframe cancel button if the challenge is still open
      await handle3DSChallenge(page, { mode: 'failure' }).catch(() => {});
      await page.waitForTimeout(1500);
      expect(page.url()).not.toMatch(/ThankYouForYourOrder/i);
    });

    await test.step('cart still has items after failed/cancelled payment', async () => {
      await goToCart(page);
      // On failure EImece does NOT clear the ShoppingCart row — user can retry with a different card.
      const rows = await page.locator('[data-shopping-item-row]').count().catch(() => 0);
      const body = await page.locator('body').innerText().catch(() => '');
      const stillHasCart = rows > 0 || !/Sepetiniz.*boş/i.test(body);
      expect(stillHasCart, 'Cart should still contain the item after a failed payment so the user can retry').toBeTruthy();
    });
  });

  test('parametrised failure matrix (data-driven)', async ({ page }) => {
    // Runs one representative failure without duplicating the full setup for each card — kept as a single
    // parametrised example for CI efficiency. The dedicated tests above remain for clearer failure messages.
    const entries = Object.entries(ERROR_CARDS).slice(0, 2);
    for (const [key, card] of entries) {
      await test.step(`matrix case: ${key} (${card.label})`, async () => {
        await page.context().clearCookies();
        await clearCart(page).catch(() => {});
        const buyer = makeBuyerInfo();
        await prepareCartAndBilling(page, buyer);
        await payWithIyzicoCard(page, card, { expect3DS: 'none' }).catch(() => {});
        await expectPaymentError(page, card.expectedError);
      });
    }
  });
});
