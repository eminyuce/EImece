const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const ADMIN = { email: 'admin@eimece.test', password: 'Dkp1.0TN' };
const ADMIN_FALLBACK = { email: 'admin@eimece.test', password: 'Test123!' };

async function adminLogin(page, creds) {
  await page.goto('/account/adminlogin/', { waitUntil: 'domcontentloaded', timeout: 20000 });
  // Find login form
  const emailInput = page.locator('input[name="Email"], input[name="email"], input[type="email"]').first();
  const passInput = page.locator('input[name="Password"], input[name="password"], input[type="password"]').first();
  await emailInput.fill(creds.email);
  await passInput.fill(creds.password);
  const submit = page.locator('button[type="submit"], input[type="submit"]').first();
  await Promise.all([
    page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
    submit.click(),
  ]);
  await page.waitForLoadState('domcontentloaded');
  return page.url();
}

async function ensureLogin(page) {
  let url = await adminLogin(page, ADMIN);
  if (/adminlogin/i.test(url)) {
    console.log(`Login with Dkp1.0TN failed, trying fallback Test123! url=${url}`);
    url = await adminLogin(page, ADMIN_FALLBACK);
  }
  console.log(`Login final url: ${url}`);
  expect(url).not.toMatch(/adminlogin/i);
}

test.describe.serial('Searchable dropdown verification', () => {
  test.setTimeout(120000);

  test('capture searchable dropdowns', async ({ page }) => {
    await ensureLogin(page);

    const screenshotsDir = path.join(__dirname, '..', 'screenshots', 'searchable-dropdown');
    fs.mkdirSync(screenshotsDir, { recursive: true });

    const targets = [
      { url: '/admin/products/saveoredit/179392', name: '01-products-saveoredit-179392', selector: '[data-brand-combobox], [data-searchable-combobox]', desc: 'Brand searchable' },
      { url: '/admin/products/saveoredit/0', name: '01b-products-saveoredit-new', selector: '[data-brand-combobox]', desc: 'Brand new' },
      { url: '/admin/stories/saveoredit/366', name: '02-stories-saveoredit-366', selector: '[data-searchable-combobox]', desc: 'Story category' },
      { url: '/admin/productcategories/saveoredit/1920', name: '03-productcategories-saveoredit-1920-template', selector: '[data-searchable-combobox]', desc: 'Template' },
      { url: '/admin/menus/movemenucategory', name: '04-menus-movemenucategory', selector: '[data-searchable-combobox]', desc: 'Menu categories' },
      { url: '/admin/productcategories/moveproductcategory', name: '05-productcategories-move', selector: '[data-searchable-combobox]', desc: 'Product category move' },
    ];

    for (const t of targets) {
      console.log(`\n=== Navigating ${t.url} ===`);
      const resp = await page.goto(t.url, { waitUntil: 'domcontentloaded', timeout: 20000 });
      console.log(`Status ${resp ? resp.status() : 'no resp'} url ${page.url()}`);
      await page.waitForLoadState('domcontentloaded');
      await page.waitForTimeout(1500);

      // Check for combobox existence
      const combos = page.locator('[data-brand-combobox], [data-searchable-combobox]');
      const count = await combos.count();
      console.log(`Found ${count} searchable combobox(es) on ${t.url}`);

      // For product page, also check native select
      const nativeSelect = page.locator('select#BrandId, select[name="BrandId"], select[name="StoryCategoryId"], select[name="TemplateId"], select[name="FirstCategoryId"], select[name="SecondCategoryId"]');
      const nativeCount = await nativeSelect.count();
      console.log(`Native selects found: ${nativeCount}`);
      for (let i = 0; i < nativeCount; i++) {
        const el = nativeSelect.nth(i);
        const id = await el.getAttribute('id');
        const name = await el.getAttribute('name');
        const hidden = await el.evaluate(e => e.classList.contains('d-none') || window.getComputedStyle(e).display === 'none');
        const opts = await el.locator('option').count();
        console.log(`  select #${id} name=${name} hidden=${hidden} options=${opts}`);
      }

      // Try to open dropdowns and screenshot
      for (let i = 0; i < count; i++) {
        const combo = combos.nth(i);
        const display = combo.locator('.admin-brand-combobox__display').first();
        const search = combo.locator('.admin-brand-combobox__search').first();
        const list = combo.locator('.admin-brand-combobox__list').first();
        const isVisibleBefore = await display.isVisible().catch(() => false);
        console.log(`Combo ${i}: display visible=${isVisibleBefore}, search visible=${await search.isVisible().catch(()=>false)}`);
        // Click display to open
        if (isVisibleBefore) {
          await display.click({ timeout: 5000 }).catch(e => console.log(`click failed ${e}`));
          await page.waitForTimeout(800);
          const dropdownVisible = await combo.locator('.admin-brand-combobox__dropdown').first().isVisible().catch(()=>false);
          console.log(`  after click dropdown visible=${dropdownVisible}`);
          // Type to filter
          if (await search.isVisible()) {
            await search.fill('a');
            await page.waitForTimeout(500);
            const visibleItems = await list.locator('.admin-brand-combobox__item:not(.is-hidden)').count();
            console.log(`  after filter 'a' visible items=${visibleItems}`);
            await search.fill('');
            await page.waitForTimeout(300);
          }
        }
        // Highlight selected
        const selected = await combo.locator('.admin-brand-combobox__item.is-selected').count();
        console.log(`  selected items=${selected}`);
      }

      // Full page screenshot
      const fullPath = path.join(screenshotsDir, `${t.name}-full.png`);
      await page.screenshot({ path: fullPath, fullPage: true });
      console.log(`Screenshot saved ${fullPath}`);

      // Try to screenshot combobox area
      if (count > 0) {
        const firstCombo = combos.first();
        const boxPath = path.join(screenshotsDir, `${t.name}-combo.png`);
        try {
          await firstCombo.screenshot({ path: boxPath });
          console.log(`Combo screenshot saved ${boxPath}`);
        } catch (e) {
          console.log(`Combo screenshot failed ${e}`);
        }
      }

      // Log HTML snippet for product brand
      if (t.url.includes('products/saveoredit')) {
        const html = await page.content();
        const hasBrandCombo = html.includes('data-brand-combobox');
        const hasDisplay = html.includes('admin-brand-combobox__display');
        const hasSearch = html.includes('admin-brand-combobox__search');
        console.log(`HTML checks: data-brand-combobox=${hasBrandCombo}, display=${hasDisplay}, search=${hasSearch}`);
        // Dump select HTML
        const selects = await page.locator('select').all();
        for (const s of selects) {
          const outer = await s.evaluate(e => e.outerHTML.slice(0, 500));
          console.log(`  select outer: ${outer}`);
        }
      }
    }
  });
});
