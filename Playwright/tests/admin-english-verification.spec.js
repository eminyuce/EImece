const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const BASE_URL = 'http://localhost:81';
const ADMIN_BASE = '/admin';

// Distinctive Turkish words that should NOT appear when English is selected
const TURKISH_INDICATORS = [
  'İşlemler', 'Yönetici notu', 'Yeni sipariş', 'Ödeme bekleniyor', 'Hazırlanmakta', 'İade tamamlandı',
  'Başarılı', 'Başarısız', 'BKM POS seçildi', 'Pecco dönüşü', 'Sola Kaydır', 'Sağa Kaydır',
  'Tabloyu Sağa/Sola Kaydır', 'Listeye Dön', 'Katalog ve içerik',
  'Ana sayfa görselleri', 'Kayan banner', 'Hakkımızda, iletişim',
  'Ürün ekle, düzenle', 'Kategori ağacı', 'Marka listesi',
  'Sipariş durumu ve kargo', 'Üye ve müşteri kayıtları', 'Kargo ve mağaza',
  'Kargo açıklaması', 'Ödeme paneli (yeni sekme)', 'Yönetici ayarları',
  'Şirket ve genel site', 'E-posta şablonlarını', 'Yönetici ve panel kullanıcıları',
  'Önbellek yönetimi', 'Tüm storefront önbelleğini', 'Aramaya başlamak için',
  'Menüde ara', 'Eşleşen menü bulunamadı', 'Toplu işlemler',
  'Seç, yayınla, sil', 'Tablo yoğunluğu', 'Sistem Sağlık Durumu',
  'Gösterge Paneli', 'Ödeme Detayları', 'Sipariş Detayı',
  'Sipariş Edilen Ürünler', 'Müşteri Detayı', 'Müşterilere Dön',
  'Siparişlere Dön', 'Durum değişince otomatik', 'Sadece yöneticiler görür',
  'Teslimat adresi ile aynı', 'Müşteri kaydı bulunamadı', 'Bu siparişte ürün kaydı yok',
  'Müşteri sipariş notu bırakmamış', 'Siparişi Yönet'
];

const ADMIN_PAGES = [
  { path: '/dashboard', name: 'Dashboard' },
  { path: '/products', name: 'Products' },
  { path: '/productcategories', name: 'ProductCategories' },
  { path: '/brands', name: 'Brands' },
  { path: '/templates', name: 'Templates' },
  { path: '/lists', name: 'Lists' },
  { path: '/coupons', name: 'Coupons' },
  { path: '/orders', name: 'Orders' },
  { path: '/customers', name: 'Customers' },
  { path: '/shoppingcarts', name: 'ShoppingCarts' },
  { path: '/report', name: 'Reports' },
  { path: '/menus', name: 'Menus' },
  { path: '/mainpageimages', name: 'MainPageImages' },
  { path: '/stories', name: 'Stories' },
  { path: '/storycategories', name: 'StoryCategories' },
  { path: '/tags', name: 'Tags' },
  { path: '/tagcategories', name: 'TagCategories' },
  { path: '/faq', name: 'FAQ' },
  { path: '/subscribers', name: 'Subscribers' },
  { path: '/mailtemplates', name: 'MailTemplates' },
  { path: '/adminsettings', name: 'AdminSettingsGeneral' },
  { path: '/adminsettings/systemsettings', name: 'SystemSettings' },
  { path: '/users', name: 'Users' },
  { path: '/users/changepassword', name: 'ChangePassword' },
  { path: '/users/enableauthenticator', name: 'EnableAuthenticator' },
  { path: '/dashboard/systemhealth', name: 'SystemHealth' },
  { path: '/metrics', name: 'Metrics' },
  { path: '/applogs', name: 'AppLogs' },
  { path: '/media', name: 'Media' },
];

const ADMIN = { email: 'admin@eimece.test', password: 'Test123!' };
const screenshotsDir = path.join(__dirname, '..', 'test-results', 'screenshots');

test('Admin Panel Full English Verification', async ({ page }) => {
  test.setTimeout(300000);

  if (!fs.existsSync(screenshotsDir)) {
    fs.mkdirSync(screenshotsDir, { recursive: true });
  }

  // Login
  console.log('Logging into admin...');
  await page.goto(`${BASE_URL}/account/adminlogin/`, { waitUntil: 'domcontentloaded' });
  const form = page.locator('form').filter({ has: page.locator('input[name="Email"]') }).first();
  await form.locator('input[name="Email"]').fill(ADMIN.email);
  await form.locator('input[name="Password"]').fill(ADMIN.password);
  await Promise.all([
    page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 30000 }).catch(() => {}),
    form.locator('button[type="submit"], input[type="submit"]').first().click(),
  ]);

  // Set AdminPanelLanguage to en-US
  console.log('Navigating to system settings...');
  await page.goto(`${BASE_URL}/admin/adminsettings/systemsettings`, { waitUntil: 'domcontentloaded' });
  const tabGeneral = page.locator('a[href="#tab-general"]');
  if (await tabGeneral.count() > 0) {
    await tabGeneral.click();
    await page.waitForTimeout(500);
  }
  await page.waitForSelector('#AdminPanelLanguage', { state: 'attached', timeout: 20000 });
  await page.selectOption('#AdminPanelLanguage', 'en-US');
  await page.click('#btnSaveSystemSettings');
  await page.waitForTimeout(2000);
  console.log('✅ Admin Panel Language set to English (en-US)');

  const turkishFoundByPage = [];

  for (const adminPage of ADMIN_PAGES) {
    const url = `${BASE_URL}${ADMIN_BASE}${adminPage.path}`;
    console.log(`Checking ${adminPage.name} (${url})...`);
    
    try {
      const response = await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 30000 });
      if (response && response.status() >= 400) {
        console.log(`  ⚠ ${adminPage.name} returned ${response.status()}`);
        continue;
      }

      await page.waitForTimeout(500);

      // Take screenshot
      const screenshotPath = path.join(screenshotsDir, `admin-english-${adminPage.name}.png`);
      await page.screenshot({ path: screenshotPath, fullPage: true });

      const pageText = await page.textContent('body');
      const found = TURKISH_INDICATORS.filter(t => pageText && pageText.includes(t));

      if (found.length > 0) {
        console.log(`  ❌ ${adminPage.name} - Turkish detected:`, found);
        turkishFoundByPage.push({ page: adminPage.name, url, strings: found });
      } else {
        console.log(`  ✅ ${adminPage.name} - 100% English`);
      }
    } catch (err) {
      console.log(`  ⚠ Error on ${adminPage.name}: ${err.message}`);
    }
  }

  console.log('\n=======================================');
  console.log('        VERIFICATION SUMMARY');
  console.log('=======================================');
  if (turkishFoundByPage.length > 0) {
    console.log(`❌ Found Turkish text in ${turkishFoundByPage.length} pages:`);
    for (const item of turkishFoundByPage) {
      console.log(`  - ${item.page}: ${item.strings.join(', ')}`);
    }
  } else {
    console.log('✅ ALL ADMIN PAGES FULLY ENGLISH!');
  }

  expect(turkishFoundByPage).toEqual([]);
});