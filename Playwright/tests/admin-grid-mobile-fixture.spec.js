const { test, expect } = require('@playwright/test');
const path = require('path');

const fixtureUrl = 'file://' + path.resolve(__dirname, '../../EImece/test-fixtures/admin-grid-mobile.html');

test.describe('Admin grid mobile fixture', () => {
  test('phone viewport uses card layout with labels and sort toolbar', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto(fixtureUrl);

    await expect(page.locator('body')).toHaveClass(/eg-grid-mobile-cards/);
    await expect(page.locator('.eg-mobile-grid-toolbar')).toBeVisible();
    await expect(page.locator('table thead')).toBeHidden();

    const firstCard = page.locator('tbody > tr').first();
    await expect(firstCard).toBeVisible();
    await expect(firstCard.locator('td.eg-col-category')).toHaveAttribute('data-label', 'Category');
    await expect(firstCard.locator('td.eg-col-price')).toHaveAttribute('data-label', 'Price');

    await page.screenshot({ path: '/opt/cursor/artifacts/admin_grid_mobile_cards.png', fullPage: true });
  });

  test('tablet viewport keeps horizontal scroll table', async ({ page }) => {
    await page.setViewportSize({ width: 820, height: 1180 });
    await page.goto(fixtureUrl);

    await expect(page.locator('body')).not.toHaveClass(/eg-grid-mobile-cards/);
    await expect(page.locator('.eg-mobile-grid-toolbar')).toHaveCount(0);
    await expect(page.locator('table thead')).toBeVisible();

    const tableWidth = await page.locator('table.eg-grid-table').evaluate((el) => el.scrollWidth);
    const containerWidth = await page.locator('.griddly-scrollable-container').evaluate((el) => el.clientWidth);
    expect(tableWidth).toBeGreaterThan(containerWidth);

    await page.screenshot({ path: '/opt/cursor/artifacts/admin_grid_tablet_scroll.png', fullPage: true });
  });

  test('desktop viewport keeps table headers visible', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 900 });
    await page.goto(fixtureUrl);

    await expect(page.locator('table thead')).toBeVisible();
    await expect(page.locator('.eg-mobile-grid-toolbar')).toHaveCount(0);

    await page.screenshot({ path: '/opt/cursor/artifacts/admin_grid_desktop.png', fullPage: true });
  });
});
