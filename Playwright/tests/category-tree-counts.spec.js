const { test, expect } = require('@playwright/test');

// Expected display totals per category (own active products + all descendants),
// cross-checked against ProductCategories/Products seed data (Lang=1).
// NOTE: names carry Turkish diacritics exactly as stored in ProductCategories.Name.
const EXPECTED_COUNTS = {
  'Elektronik': 17,
  'Moda & Giyim': 30,
  'Ev & Yaşam': 24,
  'Spor & Outdoor': 19,
  'Kozmetik & Bakım': 15,
  'Bebek & Çocuk': 10,
  'Kitap & Hobi': 8,
  'Mutfak': 22,
};

test('category tree shows active product counts with parent sums', async ({ page }) => {
  const res = await page.goto('/c/pc/yatak-odasi-5i3f4h1b', { waitUntil: 'domcontentloaded' });
  expect(res.status()).toBe(200);

  const tree = page.locator('#shopCategories');
  await expect(tree).toBeVisible();

  // Parse every rendered "name (count)" pair in the sidebar tree.
  const items = tree.locator('a');
  const n = await items.count();
  const rendered = new Map();
  for (let i = 0; i < n; i++) {
    const text = (await items.nth(i).innerText()).replace(/\s+/g, ' ').trim();
    const m = text.match(/^(.*?)\s*\((\d+)\)$/);
    if (m) {
      rendered.set(m[1], parseInt(m[2], 10));
    }
  }

  expect(rendered.size).toBeGreaterThan(10);

  // Parent nodes must display the SUM of their own plus all descendant counts.
  for (const [name, expected] of Object.entries(EXPECTED_COUNTS)) {
    expect(rendered.get(name), `${name} badge`).toBe(expected);
  }

  // Leaf keeps its own count (Turkish dotted-i preserved in rendering).
  const yatak = [...rendered.keys()].find((k) => k.startsWith('Yatak Oda'));
  expect(rendered.get(yatak)).toBe(7);

  // No server errors leaked into the page.
  const body = await page.locator('body').innerText();
  expect(body).not.toMatch(/Beklenmeyen hata|Unhandled exception|Object reference/i);
});
