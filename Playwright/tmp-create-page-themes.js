/**
 * Create dummy CMS pages (Tema 1–7) via admin menus and screenshot the public layouts.
 * Run: node tmp-create-page-themes.js
 */
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
const { loginWithPassword } = require('./tests/helpers');

const BASE = 'http://localhost:81';
const ADMIN = { email: 'admin@eimece.test', password: 'Test123!' };
const PARENT_NAME = 'Tema Ornekleri';
const THEMES = [
  { name: 'PT Dummy T1', value: 'T1' },
  { name: 'PT Dummy T2', value: 'T2' },
  { name: 'PT Dummy T3', value: 'T3' },
  { name: 'PT Dummy T4', value: 'T4' },
  { name: 'PT Dummy T5', value: 'T5' },
  { name: 'PT Dummy T6', value: 'T6' },
  { name: 'PT Dummy T7', value: 'T7' },
];
const LOREM =
  '<p>Vivamus ornare, justo non eleifend pulvinar, nisl mauris tincidunt sapien, in tincidunt erat lectus sit amet magna. Integer vitae sapien sit amet lorem tincidunt pulvinar. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.</p><p>Curabitur non nulla sit amet nisl tempus convallis quis ac lectus. Nulla quis lorem ut libero malesuada feugiat. Vestibulum ac diam sit amet quam vehicula elementum sed sit amet dui. Donec sollicitudin molestie malesuada. Praesent sapien massa, convallis a pellentesque nec, egestas non nisi.</p><p>Mauris blandit aliquet elit, eget tincidunt nibh pulvinar a. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Sed porttitor lectus nibh. Curabitur aliquet quam id dui posuere blandit. Nulla porttitor accumsan tincidunt.</p>';

function modifyId(id) {
  const keyIds = '9182736450'.split('');
  const alpha = 'abcdefghijklmnopqrstuvwxyz'.split('');
  const chars = String(id).split('');
  let result = '';
  for (let i = chars.length - 1; i >= 0; i--) {
    const num = parseInt(chars[i], 10);
    result += keyIds[num] + alpha[num];
  }
  return result;
}

function seoSlug(name) {
  return String(name)
    .toLowerCase()
    .replace(/ı/g, 'i')
    .replace(/ğ/g, 'g')
    .replace(/ü/g, 'u')
    .replace(/ş/g, 's')
    .replace(/ö/g, 'o')
    .replace(/ç/g, 'c')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '');
}

async function gotoStable(page, url) {
  await page.goto(BASE + url, { waitUntil: 'domcontentloaded', timeout: 90_000 });
  await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});
}

async function showFieldsTab(page) {
  const tab = page.locator('a[href="#admin-edit-tab-fields"], [data-toggle="tab"][href="#admin-edit-tab-fields"]').first();
  if (await tab.count()) {
    await tab.click().catch(() => {});
    await page.waitForTimeout(200);
  }
  await page.evaluate(() => {
    const pane = document.querySelector('#admin-edit-tab-fields');
    if (pane) {
      pane.classList.add('in', 'active');
      pane.style.display = 'block';
    }
    const content = document.querySelector('#admin-edit-tab-content');
    if (content) {
      content.classList.remove('in', 'active');
      content.style.display = 'none';
    }
  });
}

async function fillDescription(page, html) {
  const gotoContent = page.locator('[data-admin-edit-goto-content]').first();
  if (await gotoContent.count()) {
    await gotoContent.click().catch(() => {});
    await page.waitForTimeout(400);
  }
  const contentTab = page.locator('a[href="#admin-edit-tab-content"], [data-toggle="tab"][href="#admin-edit-tab-content"]').first();
  if (await contentTab.count()) {
    await contentTab.click().catch(() => {});
    await page.waitForTimeout(400);
  }
  await page.evaluate((html) => {
    const el = document.querySelector('#Description, textarea[name="Description"]');
    if (el) el.value = html;
    if (window.tinymce && tinymce.get('Description')) {
      tinymce.get('Description').setContent(html);
    }
    if (window.jQuery) {
      try {
        jQuery('#Description').summernote('code', html);
      } catch (_) {}
    }
    document.querySelectorAll('.note-editable, .tox-edit-area').forEach((ne) => {
      if (ne.isContentEditable || ne.classList.contains('note-editable')) ne.innerHTML = html;
    });
  }, html);
}

async function saveAndClose(page) {
  const btn = page.getByRole('button', { name: /Kaydet ve Kapat/i }).first();
  if (!(await btn.count())) throw new Error('Kaydet ve Kapat not found');
  await btn.click();
  await page.waitForLoadState('domcontentloaded', { timeout: 90_000 });
  await page.waitForTimeout(800);
}

async function findIdByName(page, name) {
  await gotoStable(page, `/admin/menus/?search=${encodeURIComponent(name)}`);
  return page.evaluate((exact) => {
    const rows = Array.from(document.querySelectorAll('table tbody tr'));
    for (const row of rows) {
      const cells = Array.from(row.querySelectorAll('td, a, span')).map((el) => (el.textContent || '').trim());
      if (!cells.some((c) => c === exact)) continue;
      const a = row.querySelector('a[href*="saveoredit"], a[href*="SaveOrEdit"]');
      if (!a) continue;
      const href = a.getAttribute('href') || '';
      const m = href.match(/saveoredit\/(\d+)/i) || href.match(/[?&]id=(\d+)/i);
      if (m && m[1] !== '0') return m[1];
    }
    return null;
  }, name);
}

async function saveAndReadId(page) {
  const btn = page.locator('button.admin-edit-save-btn, button[name="saveButton"]').filter({ hasText: /^Kaydet$/ }).first();
  if (await btn.count()) {
    await btn.click();
  } else {
    await page.getByRole('button', { name: /^Kaydet$/ }).first().click();
  }
  await page.waitForLoadState('domcontentloaded', { timeout: 90_000 });
  await page.waitForTimeout(1000);
  const id = await page.locator('#Id, input[name="Id"]').first().inputValue().catch(() => '');
  if (id && id !== '0') return id;
  throw new Error('Save did not produce an Id. URL=' + page.url());
}

async function ensureParent(page) {
  const existing = await findIdByName(page, PARENT_NAME);
  if (existing) return existing;
  await gotoStable(page, '/admin/menus/saveoredit');
  await showFieldsTab(page);
  await page.locator('#Name, input[name="Name"]').first().fill(PARENT_NAME, { force: true });
  const menuLink = page.locator('#MenuLink, select[name="MenuLink"]').first();
  if (await menuLink.count()) {
    await menuLink.selectOption('pages-index').catch(async () => {
      await menuLink.selectOption({ label: /Farkli Sayfa Temalari/i });
    });
  }
  await page.evaluate(() => {
    const t1 = document.querySelector('#T1, input[name="PageTheme"][value="T1"]');
    if (t1) t1.checked = true;
    const active = document.querySelector('#IsActive, input[name="IsActive"]');
    if (active && active.type === 'checkbox') active.checked = true;
  });
  await fillDescription(page, `<p>${PARENT_NAME} dummy parent</p>`);
  return saveAndReadId(page);
}

async function createThemePage(page, parentId, theme, position) {
  const existing = await findIdByName(page, theme.name);
  if (existing) return existing;
  await gotoStable(page, '/admin/menus/saveoredit');
  await showFieldsTab(page);
  const treeBtn = page.locator(`button.eg-tree-link[onclick*="${parentId}"]`).first();
  if (await treeBtn.count()) {
    await treeBtn.click();
  } else {
    await page.evaluate((id) => {
      const hidden = document.querySelector('#ParentId, input[name="ParentId"]');
      if (hidden) hidden.value = String(id);
    }, parentId);
  }
  await page.locator('#Name, input[name="Name"]').first().fill(theme.name, { force: true });
  const pos = page.locator('#Position, input[name="Position"]').first();
  if (await pos.count()) await pos.fill(String(position));
  const menuLink = page.locator('#MenuLink, select[name="MenuLink"]').first();
  if (await menuLink.count()) {
    await menuLink.selectOption('pages-index');
    await menuLink.dispatchEvent('change');
  }
  await page.waitForTimeout(300);
  await page.evaluate((value) => {
    const wrap = document.getElementById('pageThemeWrapper');
    if (wrap) wrap.style.display = '';
    const radio = document.querySelector(`input[name="PageTheme"][value="${value}"]`);
    if (radio) radio.checked = true;
    const active = document.querySelector('#IsActive, input[name="IsActive"]');
    if (active && active.type === 'checkbox') active.checked = true;
  }, theme.value);
  await fillDescription(page, LOREM);
  return saveAndReadId(page);
}

async function main() {
  const shotDir = path.join(__dirname, 'screenshots', 'page-themes');
  fs.mkdirSync(shotDir, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    ignoreHTTPSErrors: true,
    viewport: { width: 1400, height: 900 },
    baseURL: BASE,
  });
  const page = await context.newPage();
  page.setDefaultTimeout(60_000);
  page.on('dialog', (d) => d.accept().catch(() => {}));

  const loggedIn = await loginWithPassword(page, {
    email: ADMIN.email,
    password: ADMIN.password,
    loginPath: '/account/adminlogin/',
  });
  if (!loggedIn) {
    await page.screenshot({ path: path.join(shotDir, 'login-failed.png'), fullPage: true });
    throw new Error('Admin login failed: ' + page.url());
  }
  console.log('Logged in:', page.url());

  const parentId = await ensureParent(page);
  console.log('Parent id', parentId);

  const created = [];
  for (let i = 0; i < THEMES.length; i++) {
    const theme = THEMES[i];
    process.stdout.write(`Create ${theme.name} ... `);
    const id = await createThemePage(page, parentId, theme, i + 1);
    const url = `/pages/detail/${seoSlug(theme.name)}-${modifyId(id)}/`;
    created.push({ ...theme, id, url });
    console.log(`id=${id} ${url}`);
  }

  try {
    execSync('C:\\Windows\\system32\\inetsrv\\appcmd.exe recycle apppool /apppool.name:Eimece', { stdio: 'inherit' });
    await page.waitForTimeout(4000);
  } catch (e) {
    console.warn('App pool recycle skipped:', e.message);
  }

  const results = [];
  for (const item of created) {
    process.stdout.write(`Open ${item.name} ... `);
    const resp = await page.goto(BASE + item.url, { waitUntil: 'domcontentloaded', timeout: 60_000 });
    await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});
    const status = resp?.status() ?? 0;
    const file = path.join(shotDir, `${item.value}.png`);
    await page.screenshot({ path: file, fullPage: true });
    const hasThemeClass = await page.locator(`.pt-t${item.value.slice(1)}`).count();
    const ok = status < 400 && hasThemeClass > 0;
    results.push({ ...item, status, screenshot: file, ok, hasThemeClass });
    console.log(`${ok ? 'OK' : 'FAIL'} HTTP ${status} theme=${hasThemeClass}`);
  }

  const report = path.join(shotDir, 'report.json');
  fs.writeFileSync(report, JSON.stringify({ when: new Date().toISOString(), parentId, results }, null, 2));
  console.log('Report', report);
  await browser.close();
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
