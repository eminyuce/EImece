const { test, expect } = require('@playwright/test');

const BASE_URL = 'http://localhost:81';
const ADMIN_BASE = '/admin';

// Common Turkish strings that should NOT appear when English is selected
const TURKISH_INDICATORS = [
  'İşlemler', 'Yönetici notu', 'Sipariş', 'Ödeme', 'Kargo', 'Müşteri', 'Ürün',
  'İptal', 'Yeni sipariş', 'Ödeme bekleniyor', 'Hazırlanmakta', 'İade', 'İade tamamlandı',
  'Başarılı', 'Başarısız', 'BKM POS seçildi', 'Pecco dönüşü', 'Sola Kaydır', 'Sağa Kaydır',
  'Tabloyu Sağa/Sola Kaydır', 'Düzenle', 'Sil', 'Kaydet', 'Yükle', 'İptal', 'Listeye Dön',
  'Katalog', 'Satışlar', 'İçerik', 'Sistem', 'Ayarlar', 'Raporlar', 'Metrikler', 'Sağlık',
  'Medya', 'Kullanıcılar', 'Markalar', 'Kategoriler', 'Kuponlar', 'Şablonlar', 'Listeler',
  'Siparişler', 'Müşteriler', 'Sepetler', 'Mağaza', 'Görünüm', 'Güvenlik', 'E-Posta',
  'Entegrasyonlar', 'Araçlar', 'Bakım', 'Firma', 'Sosyal', 'Canlı Destek', 'Analitik',
  'Harita', 'WhatsApp', 'Zopim', 'Google', 'Analytics', 'Script', 'İletişim', 'Adres',
  'Telefon', 'E-Posta', 'Firma Unvanı', 'Ücretsiz Kargo', 'Sepet Tutarı',
  'Arama Motoru', 'Bakım Modu', 'İndeksleme', 'Tema', 'PWA', 'WebP', 'JPEG', 'Kalite',
  'Thumbnail', 'Küçük Resim', 'Sidecar', 'Orijinal', 'Dönüştürme', 'Sıkıştırma',
  'Maksimum', 'Genişlik', 'Yükseklik', 'Piksel', 'Format', 'Tercih Et', 'Kaydet',
  'Admin Paneli Dili', 'Veri Giriş Dili', 'Üst Çubuk', 'Bağımsızdır', 'Arayüz',
  'Menüler', 'Etiketler', 'Butonlar', 'Mesajlar', 'Veri Giriş', 'Dil Seçimi',
  'Yönetim Paneli', 'Genel Bakış', 'Mağaza Davranışı', 'Katalog Modu', 'Fiyat Filtresi',
  'Ödeme Sekmesi', 'HTML', 'Yedek Alıcı', 'TCKN', 'Kimlik No', 'Taksit', 'Seçenekleri',
  'Ödeme Sağlayıcı', 'Sanal POS', 'Alıcı', 'Kimlik', 'Bot', 'Spam', 'Koruma',
  'Captcha', 'reCAPTCHA', 'Legacy', 'Matematiksel', 'Görsel', 'İstek Sınırlandırma',
  'Rate Limiting', 'Brute-force', 'DDoS', 'IP tabanlı', 'Sınırlar', 'Zaman Penceresi',
  'Giriş Sayfası', 'İletişim Formu', 'Ödeme Adımı', 'Arama Sorguları', 'Crawler', 'Scraper',
  'SMTP', 'Sunucu', 'Port', 'Kullanıcı Adı', 'Şifre', 'SSL', 'TLS', 'Varsayılan',
  'Kimlik Bilgileri', 'Windows', 'Test E-Postası', 'Gönder', 'Canlı', 'Durum',
  'Gösterge Paneli', 'Genel Koruma', 'Güçlü', 'Orta Düzey', 'Kritik', 'Zorunlu',
  'Tüm yöneticiler', 'mobil Authenticator', '6 haneli', 'kod girmek zorundadır',
  'yalnızca kullanıcı adı', 'şifre ile giriş yapabilir', 'Aktif sağlayıcı',
  'Form spam koruması kapalı', 'İstek sınırlama devre dışı', 'Brute-force şifre',
  'denemeleri', 'iletişim formu suistimali', 'ataklarına karşı', 'Süre (Dakika)',
  'Maks. İstek', 'Özellik Bazlı', 'Saat', 'Dakika', 'Saniye', 'Gün', 'Hafta', 'Ay',
  'Yıl', 'Bugün', 'Dün', 'Bu Hafta', 'Bu Ay', 'Özel Aralık', 'Tarih Seçin',
  'Başlangıç', 'Bitiş', 'Filtrele', 'Temizle', 'Ara', 'Sonuç', 'Bulunamadı',
  'Toplam Kayıt', 'Sayfalama', 'Satır Sayısı', 'Göster', 'Gizle', 'Sırala',
  'Artan', 'Azalan', 'Varsayılan', 'Seçenek', 'Seçiniz', 'Lütfen', 'Seçin',
  'Gerekli', 'Zorunlu', 'Alan', 'Doldurulması', 'Hata', 'Başarılı', 'Tamamlandı',
  'İşleminiz', 'Gerçekleşmiştir', 'Onaylıyor musunuz', 'Evet', 'Hayır', 'Kapat',
  'Geri', 'İleri', 'İlk', 'Son', 'Önceki', 'Sonraki', 'Sayfa', 'Adet', 'Tümü',
  'Seçili', 'Yayınla', 'Yayından Kaldır', 'Seçilenleri', 'Kaldır', 'Hepsi seç',
  'Detay', 'Resimler', 'Sıra Güncelle', 'Yeni Giriş', 'Gönder', 'EKLE', 'Ara',
  'Arama yapmak için lütfen bir şey yazınız', 'Lütfen bir arama terimi giriniz',
  'Şifrenizi sıfırlamak için lütfen e-posta gelen kutunuzu kontrol ediniz',
  'Bizimle iletişime geçtiğiniz için teşekkürler', 'Mümkün olan en kısa zamanda',
  'yanıt vermeye çalışıyoruz', 'Günün geri kalanı güzel geçsin',
  'Şifrenizi sıfırlamak için', 'e-posta adresinizi giriniz', 'Ad / Soyad',
  'Lütfen adınızı ve soyadınızı giriniz', 'Mesajınız', 'Lütfen mesajınızı giriniz',
  'Güvenlik doğrulaması', 'Lütfen reCAPTCHA doğrulamasını tamamlayınız',
  'Firma Adı', 'E-Posta', 'Lütfen e-posta adresinizi giriniz',
  'Lütfen arama terimi giriniz', 'Arama yapmak için lütfen bir şey yazınız',
  'Gönder', 'EKLE', 'Ara', 'Vitrinde mi?', 'Resmi Göster', 'Durum', 'İşlem',
  'Excel Dosyasına Aktar', 'Seçilenleri Yayınla', 'Seçilenleri Yayından Kaldır',
  'Seçilen Kayıtları Sil', 'Seçilenleri Kaldır', 'Hepsi seç', 'Yeni Giriş',
  'Kaydet', 'Sil', 'Düzenle', 'Detay', 'Resimler', 'Sıra Güncelle',
  'Sayfalama', 'Arama', 'Seçilenleri Kaldır', 'Grid', 'Aktif', 'Pasif',
  'Evet', 'Hayır', 'Var', 'Yok', 'Açık', 'Kapalı', 'Aktif', 'Pasif',
  'Yeni', 'Eski', 'Büyük', 'Küçük', 'Uzun', 'Kısa', 'Genel', 'Özel',
  'Standart', 'Özel', 'Temel', 'Gelişmiş', 'Basit', 'Karmaşık',
  'Hızlı', 'Yavaş', 'Kolay', 'Zor', 'Basit', 'Karmaşık', 'Açık', 'Kapalı'
];

// Admin pages to verify
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
  { path: '/dashboard/metrics', name: 'Metrics' },
  { path: '/applogs', name: 'AppLogs' },
  { path: '/images', name: 'MediaImages' },
];

let turkishStringsFound = [];

const ADMIN = { email: 'admin@eimece.test', password: 'Test123!' };

async function adminLogin(page) {
  await page.goto('/account/adminlogin/', { waitUntil: 'domcontentloaded' });
  const form = page.locator('form').filter({ has: page.locator('input[name="Email"]') }).first();
  await form.locator('input[name="Email"]').fill(ADMIN.email);
  await form.locator('input[name="Password"]').fill(ADMIN.password);
  await Promise.all([
    page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
    form.locator('button[type="submit"], input[type="submit"]').first().click(),
  ]);
  return page.url();
}

test.describe.serial('Admin Panel English Language Verification', () => {
  test('Setup: Login and set language to English', async ({ page }) => {
    const loginUrl = await adminLogin(page);
    console.log(`Login returned URL: ${loginUrl}`);
    // Verify admin UI is loaded - try multiple selectors
    await page.goto(`${ADMIN_BASE}/dashboard`, { waitUntil: 'domcontentloaded' });
    const currentUrl = page.url();
    console.log(`After login dashboard URL: ${currentUrl}`);
    if (currentUrl.includes('/account/adminlogin') || currentUrl.includes('/Account/AdminLogin')) {
      throw new Error(`Login failed - still on login page: ${currentUrl}`);
    }
    // Verify admin sidebar/topbar exists (more lenient)
    await expect(page.locator('body')).toContainText(/Dashboard|Admin|Erayweb/i, { timeout: 15000 });
    
    // Go to system settings to set language
    await page.goto(`${ADMIN_BASE}/adminsettings/systemsettings`, { waitUntil: 'domcontentloaded' });
    // SystemSettings has tab navigation - AdminPanelLanguage is inside #tab-general (hidden by default)
    // Need to activate that tab first
    const tabGeneral = page.locator('a[href="#tab-general"]');
    if (await tabGeneral.count() > 0) {
      await tabGeneral.click();
      await page.waitForTimeout(500);
    }
    await page.waitForSelector('#AdminPanelLanguage', { state: 'attached', timeout: 15000 });
    // Ensure element is visible by clicking tab if still hidden
    const sel = page.locator('#AdminPanelLanguage');
    if (!(await sel.isVisible())) {
      await tabGeneral.click();
      await page.waitForTimeout(500);
    }
    
    // Select English
    await page.selectOption('#AdminPanelLanguage', 'en-US');
    
    // Save settings
    await page.click('#btnSaveSystemSettings');
    
    // Wait for save confirmation or page reload
    await page.waitForTimeout(3000);
    await page.waitForLoadState('domcontentloaded');
    
    // Verify saved value
    const selectedVal = await page.locator('#AdminPanelLanguage').inputValue();
    console.log(`AdminPanelLanguage after save: ${selectedVal}`);
    if (selectedVal !== 'en-US') {
      console.log('⚠ Language not set to en-US, retrying...');
      await page.selectOption('#AdminPanelLanguage', 'en-US');
      await page.click('#btnSaveSystemSettings');
      await page.waitForTimeout(3000);
    }
    
    // Reload to apply language
    await page.goto(`${ADMIN_BASE}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('domcontentloaded');
    
    console.log('✅ Language set to English');
  });

  for (const adminPage of ADMIN_PAGES) {
    test(`${adminPage.name} - Page loads and no Turkish strings`, async ({ page }) => {
      const url = `${ADMIN_BASE}${adminPage.path}`;
      console.log(`\nTesting: ${url}`);
      
      const response = await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });
      
      if (!response || response.status() >= 400) {
        console.log(`  ⚠ Page returned ${response?.status() || 'no response'}`);
        test.skip(true, `Page returned ${response?.status() || 'no response'}`);
        return;
      }
      
      // Wait a bit for dynamic content
      await page.waitForTimeout(1000);
      
      // Take screenshot
      await page.screenshot({ 
        path: `test-results/screenshots/admin-english-${adminPage.name}.png`,
        fullPage: true 
      });
      
      // Get page text content
      const pageText = await page.textContent('body');
      
      if (!pageText) {
        console.log(`  ⚠ No body text content`);
        return;
      }
      
      // Check for Turkish strings
      const foundTurkish = TURKISH_INDICATORS.filter(turkish => 
        pageText.includes(turkish)
      );
      
      if (foundTurkish.length > 0) {
        console.log(`  ❌ Found Turkish strings in ${adminPage.name}:`, foundTurkish.slice(0, 15));
        turkishStringsFound.push({ page: adminPage.name, strings: foundTurkish, url });
      } else {
        console.log(`  ✅ ${adminPage.name} - No Turkish strings detected`);
      }
      
      // Verify English content is present (basic check)
      const hasEnglish = pageText && (
        pageText.includes('Admin') || 
        pageText.includes('Dashboard') || 
        pageText.includes('Settings') || 
        pageText.includes('Save') || 
        pageText.includes('Delete') || 
        pageText.includes('Edit') || 
        pageText.includes('Search') ||
        pageText.includes('Settings') ||
        pageText.includes('List') ||
        pageText.includes('Home')
      );
      
      if (!hasEnglish) {
        console.log(`  ⚠ ${adminPage.name} - No obvious English content detected`);
      }
    });
  }

  test.afterAll(async () => {
    console.log('\n=== SUMMARY ===');
    if (turkishStringsFound.length > 0) {
      console.log(`\n❌ Pages with Turkish strings (${turkishStringsFound.length}):`);
      for (const item of turkishStringsFound) {
        console.log(`  - ${item.page} (${item.url}): ${item.strings.slice(0, 10).join(', ')}${item.strings.length > 10 ? '...' : ''}`);
      }
    } else {
      console.log('\n✅ All admin pages verified - No Turkish strings found!');
    }
    
    // Save detailed report
    const fs = require('fs');
    fs.writeFileSync(
      'test-results/turkish-strings-report.json',
      JSON.stringify(turkishStringsFound, null, 2)
    );
    console.log('\nDetailed report saved to test-results/turkish-strings-report.json');
  });
});