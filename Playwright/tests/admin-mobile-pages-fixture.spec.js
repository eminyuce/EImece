const { test, expect } = require('@playwright/test');
const path = require('path');

const gridFixture = 'file://' + path.resolve(__dirname, '../../EImece/test-fixtures/admin-grid-mobile.html');
const editFixture = 'file://' + path.resolve(__dirname, '../../EImece/test-fixtures/admin-edit-form-mobile.html');

test.describe('Admin mobile page fixtures', () => {
  test('edit form stacks tree, toolbar, and fields on phone', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto(editFixture);

    const treeCol = page.locator('.eg-products-tree-col');
    const mainCol = page.locator('.eg-products-main-col');
    const treeBox = await treeCol.boundingBox();
    const mainBox = await mainCol.boundingBox();
    expect(treeBox.y).toBeLessThan(mainBox.y);

    await expect(page.locator('.admin-edit-toolbar-actions .btn').first()).toBeVisible();
    const textarea = page.locator('.eg-admin-fluid-textarea');
    const textareaWidth = await textarea.evaluate((el) => el.getBoundingClientRect().width);
    const viewportWidth = page.viewportSize().width;
    expect(textareaWidth).toBeLessThanOrEqual(viewportWidth);

    const dateInput = page.locator('.eg-spec-date-input');
    const dateWidth = await dateInput.evaluate((el) => el.getBoundingClientRect().width);
    expect(dateWidth).toBeLessThanOrEqual(viewportWidth);

    const search = page.locator('.settings-search-wrapper');
    const searchBox = await search.boundingBox();
    expect(searchBox.width).toBeLessThanOrEqual(viewportWidth);

    const pageOverflow = await page.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 2);
    expect(pageOverflow).toBeFalsy();

    await page.screenshot({ path: '/opt/cursor/artifacts/admin_edit_form_mobile.png', fullPage: true });
  });

  test('grid fixture still passes on phone after page-wide mobile rules', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto(gridFixture);
    await expect(page.locator('.eg-mobile-grid-toolbar')).toBeVisible();
    await expect(page.locator('table thead')).toBeHidden();
  });
});
