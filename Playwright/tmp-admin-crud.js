/**
 * Admin CRUD smoke: create → edit → delete.
 * Requires BypassAdminAuth. Writes Playwright/tmp-admin-crud-report.json
 */
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const BASE = 'http://localhost:81';
const stamp = Date.now().toString(36);
const marker = `QA-CRUD-${stamp}`;

const AJAX_DELETE = {
  Faq: 'DeleteFaqGridItem',
  Brands: 'DeleteBrandGridItem',
  Tags: 'DeleteTagGridItem',
  TagCategories: 'DeleteTagCategoriesGridItem',
  Coupons: 'DeleteCouponsGridItem',
  Lists: 'DeleteListGridItem',
  Templates: 'DeleteTemplateGridItem',
  MailTemplates: 'DeleteMailTemplateGridItem',
  StoryCategories: 'DeleteStoryCategoryGridItem',
  Stories: 'DeleteStoryGridItem',
  Menus: 'DeleteMenusGridItem',
  MainPageImages: 'DeleteMainPageImageGridItem',
  ProductCategories: 'DeleteProductCategoriesGridItem',
  Products: 'DeleteProductGridItem',
};

const MODULES = [
  {
    name: 'Faq',
    list: '/admin/faq/',
    create: '/admin/faq/saveoredit',
    listTerm: (m) => `${m}-faq`,
    needsRichAnswer: true,
    fields: [
      { sel: '#Name, input[name="Name"]', value: (m) => `${m}-faq` },
      { sel: '#Question, textarea[name="Question"], input[name="Question"]', value: (m) => `${m}-faq` },
    ],
  },
  {
    name: 'Brands',
    list: '/admin/brands/',
    create: '/admin/brands/saveoredit',
    fields: [{ sel: '#Name, input[name="Name"]', value: (m) => `${m}-brand` }],
  },
  {
    name: 'Tags',
    list: '/admin/tags/',
    create: '/admin/tags/saveoredit',
    pickSelect: '#TagCategoryId, select[name="TagCategoryId"]',
    fields: [{ sel: '#Name, input[name="Name"]', value: (m) => `${m}-tag` }],
  },
  {
    name: 'TagCategories',
    list: '/admin/tagcategories/',
    create: '/admin/tagcategories/saveoredit',
    fields: [{ sel: '#Name, input[name="Name"]', value: (m) => `${m}-tagcat` }],
  },
  {
    name: 'Coupons',
    list: '/admin/coupons/',
    create: '/admin/coupons/saveoredit',
    fields: [
      { sel: '#Name, input[name="Name"]', value: (m) => `${m}-coupon` },
      { sel: '#Code, input[name="Code"]', value: () => `C${stamp}`.slice(0, 12) },
      { sel: '#Discount, input[name="Discount"]', value: () => '10', optional: true },
      { sel: '#DiscountPercentage, input[name="DiscountPercentage"]', value: () => '0', optional: true },
      { sel: '#StartDateStr, input[name="StartDateStr"]', value: () => '01.01.2026', optional: true },
      { sel: '#EndDateStr, input[name="EndDateStr"]', value: () => '31.12.2026', optional: true },
    ],
  },
  {
    name: 'Lists',
    list: '/admin/lists/',
    create: '/admin/lists/saveoredit',
    fields: [
      { sel: '#Name, input[name="Name"]', value: (m) => `${m}-list` },
      { sel: '#ItemText, textarea[name="ItemText"], textarea[name="itemText"]', value: () => 'A\nB\nC' },
    ],
  },
  {
    name: 'Templates',
    list: '/admin/templates/',
    create: '/admin/templates/saveoredit',
    loadTemplateSample: true,
    fields: [{ sel: '#Name, input[name="Name"]', value: (m) => `${m}-tpl` }],
  },
  {
    name: 'MailTemplates',
    list: '/admin/mailtemplates/',
    create: '/admin/mailtemplates/saveoredit',
    needsRichBody: true,
    fields: [
      { sel: '#Name, input[name="Name"]', value: (m) => `${m}-mail`, forceEval: true },
      { sel: '#Subject, input[name="Subject"]', value: (m) => `${m} subject` },
    ],
  },
  {
    name: 'StoryCategories',
    list: '/admin/storycategories/',
    create: '/admin/storycategories/saveoredit',
    needsRichDescription: true,
    pickPageTheme: true,
    fields: [{ sel: '#Name, input[name="Name"]', value: (m) => `${m}-scat` }],
  },
  {
    name: 'Stories',
    list: '/admin/stories/',
    create: '/admin/stories/saveoredit',
    fields: [{ sel: '#Name, input[name="Name"]', value: (m) => `${m}-story` }],
    pickSelect: '#StoryCategoryId, select[name="StoryCategoryId"]',
  },
  {
    name: 'Menus',
    list: '/admin/menus/',
    create: '/admin/menus/saveoredit',
    pickSelect: '#MenuLink, select[name="MenuLink"]',
    fields: [{ sel: '#Name, input[name="Name"]', value: (m) => `${m}-menu` }],
  },
  {
    name: 'MainPageImages',
    list: '/admin/mainpageimages/',
    create: '/admin/mainpageimages/saveoredit',
    fields: [
      { sel: '#Name, input[name="Name"]', value: (m) => `${m}-mpi` },
      { sel: '#Link, input[name="Link"], #ImageLink, input[name="ImageLink"]', value: () => '/', optional: true },
    ],
  },
  {
    name: 'ProductCategories',
    list: '/admin/productcategories/',
    create: '/admin/productcategories/saveoredit',
    fields: [{ sel: '#Name, input[name="Name"]', value: (m) => `${m}-pcat` }],
  },
  {
    name: 'Products',
    list: '/admin/products/',
    create: '/admin/products/saveoredit',
    pickCategoryTree: true,
    pickSelect: '#State, select[name="State"]',
    fields: [
      { sel: '#Name, input[name="Name"]', value: (m) => `${m}-product` },
      { sel: '#ProductCode, input[name="ProductCode"]', value: () => `PC${stamp}`.slice(0, 20) },
      { sel: '#PriceStr, input[name="PriceStr"], #Price, input[name="Price"]', value: () => '19,99', optional: true },
    ],
  },
];

async function gotoStable(page, url) {
  await page.goto(BASE + url, { waitUntil: 'domcontentloaded', timeout: 90_000 });
  await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});
}

async function fillFields(page, fields, markerVal) {
  for (const f of fields) {
    const loc = page.locator(f.sel).first();
    const count = await loc.count();
    if (!count) {
      if (f.optional) continue;
      throw new Error(`Field not found: ${f.sel}`);
    }
    const val = typeof f.value === 'function' ? f.value(markerVal) : f.value;
    if (f.forceEval) {
      await page.evaluate(
        ({ sel, v }) => {
          const el = document.querySelector(sel.split(',')[0].trim());
          if (!el) return;
          el.removeAttribute('readonly');
          el.removeAttribute('disabled');
          el.value = v;
          el.dispatchEvent(new Event('input', { bubbles: true }));
          el.dispatchEvent(new Event('change', { bubbles: true }));
        },
        { sel: f.sel, v: String(val) }
      );
    } else if (f.force) {
      await loc.evaluate((el, v) => {
        el.value = v;
        el.dispatchEvent(new Event('input', { bubbles: true }));
        el.dispatchEvent(new Event('change', { bubbles: true }));
      }, String(val));
    } else {
      await loc.fill(String(val), { force: true });
    }
  }
}

async function pickFirstSelect(page, sel) {
  if (!sel) return;
  const cat = page.locator(sel).first();
  if (!(await cat.count())) return;
  const values = await cat.locator('option').evaluateAll((opts) =>
    opts
      .map((o) => o.value)
      .filter((v) => v && v !== '0' && v !== '-1' && v.toUpperCase() !== 'NONE')
  );
  if (values.length) await cat.selectOption(values[0]);
}

async function pickCategoryTree(page) {
  const leaf = page.locator('.eg-tree-node.is-leaf button.eg-tree-link').first();
  if (await leaf.count()) {
    await leaf.click();
    return;
  }
  // fallback: set hidden from data-category-id
  await page.evaluate(() => {
    const node = document.querySelector('.eg-tree-node.is-leaf[data-category-id]');
    const hid = document.getElementById('ProductCategoryId');
    if (node && hid) {
      hid.value = node.getAttribute('data-category-id');
      hid.dispatchEvent(new Event('change', { bubbles: true }));
    }
  });
}

async function fillRich(page, selector, html) {
  await page.evaluate(
    ({ selector, html }) => {
      const el = document.querySelector(selector);
      if (el) el.value = html;
      if (window.jQuery) {
        try {
          jQuery(selector).summernote('code', html);
        } catch (_) {}
        jQuery('.note-editable').each(function () {
          this.innerHTML = html;
        });
      } else {
        document.querySelectorAll('.note-editable').forEach((ne) => {
          ne.innerHTML = html;
        });
      }
    },
    { selector, html }
  );
}

async function loadTemplateSample(page) {
  await page.waitForTimeout(800);
  await page.evaluate(() => {
    if (typeof AdminTemplateBuilder === 'undefined') return;
    const samples = AdminTemplateBuilder.SAMPLES || [];
    if (!samples.length) return;
    const sample = samples[0];
    const root = document.querySelector('#templateBuilder');
    // Trigger builder via sample bar internals: set editor + rebuild from xml
    if (typeof editor !== 'undefined' && editor && editor.setValue) {
      editor.setValue(sample.xml);
      if (editor.save) editor.save();
    }
    const ta = document.getElementById('code');
    if (ta) ta.value = sample.xml;
    // Prefer clicking Apply XML if present after setting advanced xml
    const apply = document.getElementById('btnApplyXmlToBuilder');
    if (apply) apply.click();
  });
  // If builder still empty, click sample with dialog auto-accept
  const hasFields = await page.evaluate(() => {
    const nodes = document.querySelectorAll('[data-tb-field], .tb-field, .admin-tb-field');
    return nodes.length > 0;
  });
  if (!hasFields) {
    page.once('dialog', (d) => d.accept().catch(() => {}));
    const sampleBtn = page.locator('[data-tb-sample]').first();
    if (await sampleBtn.count()) {
      await sampleBtn.click();
      await page.waitForTimeout(400);
    }
  }
  // Ensure at least one textbox exists in builder xml
  await page.evaluate(() => {
    const xml =
      '<component>\n  <group name="QA Group">\n    <textbox name="Renk" />\n  </group>\n</component>';
    if (typeof editor !== 'undefined' && editor && editor.setValue) {
      editor.setValue(xml);
      if (editor.save) editor.save();
    }
    const ta = document.getElementById('code');
    if (ta) ta.value = xml;
    const apply = document.getElementById('btnApplyXmlToBuilder');
    if (apply) apply.click();
  });
  await page.waitForTimeout(300);
}

async function saveAndClose(page) {
  const btn = page.getByRole('button', { name: /Kaydet ve Kapat/i }).first();
  if (!(await btn.count())) {
    throw new Error('Kaydet ve Kapat button not found');
  }
  await btn.click();
  await page.waitForLoadState('domcontentloaded', { timeout: 90_000 });
  await page.waitForTimeout(800);
}

async function searchList(page, listPath, term) {
  await gotoStable(page, `${listPath}?search=${encodeURIComponent(term)}`);
  const search = page
    .locator('input[name="search"]:visible, #search:visible, input[type="search"]:visible')
    .first();
  if (await search.count()) {
    await search.fill(term, { force: true }).catch(() => {});
    await Promise.all([
      page.waitForLoadState('domcontentloaded', { timeout: 90_000 }),
      search.press('Enter'),
    ]).catch(async () => {
      await gotoStable(page, `${listPath}?search=${encodeURIComponent(term)}`);
    });
    await page.waitForTimeout(500);
  }
}

async function findEntityId(page, term) {
  const hrefs = await page.locator('a[href*="saveoredit"], a[href*="SaveOrEdit"]').evaluateAll((as) =>
    as.map((a) => a.getAttribute('href') || '')
  );
  for (const href of hrefs) {
    const m = href.match(/saveoredit\/(\d+)/i) || href.match(/[?&]id=(\d+)/i);
    if (m && m[1] !== '0') return { id: m[1], href };
  }
  const row = page.locator('table tbody tr').filter({ hasText: term }).first();
  if (await row.count()) {
    const href = await row.locator('a[href*="saveoredit"]').first().getAttribute('href');
    if (href) {
      const m = href.match(/saveoredit\/(\d+)/i) || href.match(/[?&]id=(\d+)/i);
      if (m && m[1] !== '0') return { id: m[1], href };
    }
  }
  return null;
}

async function ajaxDelete(page, action, id) {
  const status = await page.evaluate(
    async ({ action, id }) => {
      const token =
        document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      const resp = await fetch(`/admin/Ajax/${action}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json; charset=utf-8',
          RequestVerificationToken: token,
          'X-Requested-With': 'XMLHttpRequest',
        },
        body: JSON.stringify({ values: [String(id)] }),
        credentials: 'same-origin',
      });
      return resp.status;
    },
    { action, id }
  );
  return status;
}

async function captureSaveErrors(page) {
  const alerts = (await page.locator('.alert, .validation-summary-errors, .text-danger').allInnerTexts())
    .map((t) => t.trim())
    .filter(Boolean)
    .slice(0, 8);
  return alerts;
}

async function runModule(page, mod) {
  const result = { module: mod.name, ok: false, create: null, edit: null, del: null, detail: '' };
  const createdName = (mod.listTerm || mod.fields[0].value)(marker);
  const editedName = `${createdName}-EDIT`;

  try {
    await gotoStable(page, mod.create);
    if (mod.loadTemplateSample) await loadTemplateSample(page);
    if (mod.pickCategoryTree) await pickCategoryTree(page);
    await pickFirstSelect(page, mod.pickSelect);
    if (mod.pickPageTheme) {
      await page.evaluate(() => {
        const theme = document.querySelector('input[name="PageTheme"]');
        if (theme) theme.checked = true;
      });
    }
    await fillFields(page, mod.fields, marker);

    if (mod.needsRichAnswer) {
      const tab = page.getByRole('tab', { name: /Cevap/i }).first();
      if (await tab.count()) await tab.click().catch(() => {});
      await fillRich(page, '#Answer', `<p>${marker} answer</p>`);
    }
    if (mod.needsRichBody) {
      await fillRich(page, '#Body', `<p>${marker} body</p>`);
    }
    if (mod.needsRichDescription) {
      await fillRich(page, '#Description', `<p>${marker} description</p>`);
    }

    await saveAndClose(page);
    // If still on edit form, capture validation
    if (/saveoredit/i.test(page.url())) {
      const errs = await captureSaveErrors(page);
      throw new Error(`Create stayed on form: ${errs.join(' | ') || page.url()}`);
    }

    await searchList(page, mod.list, createdName);
    let text = await page.locator('body').innerText();
    if (!text.includes(createdName)) throw new Error(`Create missing in list: ${createdName}`);
    result.create = 'OK';

    const found = await findEntityId(page, createdName);
    if (!found) throw new Error('Could not resolve entity id after create');

    const editUrl = found.href.startsWith('http') ? found.href.replace(BASE, '') : found.href;
    await gotoStable(page, editUrl);
    const nameInput = page.locator(mod.fields[0].sel).first();
    if (await nameInput.count()) {
      await nameInput.fill(editedName, { force: true }).catch(async () => {
        await page.evaluate(
          ({ sel, v }) => {
            const el = document.querySelector(sel.split(',')[0].trim());
            if (el) {
              el.removeAttribute('readonly');
              el.removeAttribute('disabled');
              el.value = v;
            }
          },
          { sel: mod.fields[0].sel, v: editedName }
        );
      });
    }
    // Faq grid shows Question — keep Question in sync for list verify
    if (mod.name === 'Faq') {
      const q = page.locator('#Question, textarea[name="Question"]').first();
      if (await q.count()) await q.fill(editedName, { force: true });
    }
    await saveAndClose(page);
    await searchList(page, mod.list, editedName);
    text = await page.locator('body').innerText();
    if (!text.includes(editedName)) throw new Error(`Edit missing in list: ${editedName}`);
    result.edit = 'OK';

    const afterEdit = await findEntityId(page, editedName);
    const id = afterEdit ? afterEdit.id : found.id;
    const delStatus = await ajaxDelete(page, AJAX_DELETE[mod.name], id);
    await searchList(page, mod.list, editedName);
    text = await page.locator('body').innerText();
    if (text.includes(editedName)) {
      throw new Error(`Delete failed (status=${delStatus}); still listed: ${editedName}`);
    }
    result.del = 'OK';
    result.ok = true;
  } catch (e) {
    result.detail = e && e.message ? e.message : String(e);
    try {
      await gotoStable(page, '/admin/dashboard/');
    } catch (_) {}
  }
  return result;
}

async function smokeReadOnly(page) {
  const pages = [
    '/admin/orders/',
    '/admin/customers/',
    '/admin/subscribers/',
    '/admin/shoppingcarts/',
    '/admin/productcomments/',
    '/admin/applogs/',
    '/admin/users/',
    '/admin/settings/',
    '/admin/media/',
    '/admin/report/',
    '/admin/metrics/',
    '/admin/adminsettings/',
    '/admin/adminsettings/systemsettings/',
    '/admin/importdata/',
    '/admin/fileupload/',
  ];
  const out = [];
  for (const p of pages) {
    try {
      const res = await page.goto(BASE + p, { waitUntil: 'domcontentloaded', timeout: 90_000 });
      const status = res?.status() ?? 0;
      const text = await page.locator('body').innerText();
      const bad = status >= 500 || /Server Error|Unhandled exception|Parser Error/i.test(text);
      out.push({
        module: p,
        ok: !bad && status < 400,
        create: 'n/a',
        edit: 'n/a',
        del: 'n/a',
        detail: bad ? `status=${status}` : `index ${status}`,
      });
    } catch (e) {
      out.push({ module: p, ok: false, detail: String(e.message || e) });
    }
  }
  return out;
}

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ ignoreHTTPSErrors: true });
  const page = await context.newPage();
  page.setDefaultTimeout(60_000);
  page.on('dialog', (d) => d.accept().catch(() => {}));

  console.log(`Marker: ${marker}`);
  const results = [];

  for (const mod of MODULES) {
    process.stdout.write(`CRUD ${mod.name} ... `);
    const r = await runModule(page, mod);
    results.push(r);
    console.log(
      r.ok ? 'OK' : `FAIL [c=${r.create} e=${r.edit} d=${r.del}] ${r.detail || ''}`
    );
  }

  console.log('\nRead-only / index pages:');
  const readOnly = await smokeReadOnly(page);
  for (const r of readOnly) {
    results.push(r);
    console.log(`${r.ok ? 'OK  ' : 'FAIL'} ${r.module} ${r.detail || ''}`);
  }

  const crud = results.filter((r) => MODULES.some((m) => m.name === r.module));
  const report = {
    marker,
    when: new Date().toISOString(),
    summary: {
      total: results.length,
      ok: results.filter((r) => r.ok).length,
      fail: results.filter((r) => !r.ok).length,
      crudOk: crud.filter((r) => r.ok).length,
      crudFail: crud.filter((r) => !r.ok).length,
    },
    results,
  };
  const outPath = path.join(__dirname, 'tmp-admin-crud-report.json');
  fs.writeFileSync(outPath, JSON.stringify(report, null, 2));
  console.log(`\nReport: ${outPath}`);
  console.log(
    `SUMMARY ok=${report.summary.ok}/${report.summary.total} crud=${report.summary.crudOk}/${crud.length}`
  );

  await browser.close();
  process.exit(report.summary.fail ? 1 : 0);
}

main().catch((e) => {
  console.error(e);
  process.exit(2);
});
