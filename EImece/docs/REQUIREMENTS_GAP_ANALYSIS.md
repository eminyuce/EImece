# E-Ticaret Gereksinimleri — Mevcut Durum ve Eksik Analizi

**Proje:** EImece (özel ASP.NET MVC e-ticaret uygulaması)  
**Amaç:** Verilen iş gereksinimlerini mevcut kod tabanı ile karşılaştırmak; eksikleri ve öncelikli geliştirme alanlarını belirlemek.  
**Kapsam:** Teknik altyapı, sipariş/ürün yönetimi, pazarlama ve diğer operasyonel notlar.

---

## 1. Yönetici Özeti

Mevcut uygulama, ürün kataloğu, sepet, Iyzico ödeme, sipariş yaşam döngüsü, SEO, içerik yönetimi ve admin paneli açısından **kısmen olgun** bir özel e-ticaret sistemidir. Çekirdek mağaza akışı çalışır durumdadır.

Buna karşılık gereksinim listesindeki kritik boşluklar şunlardır:

1. **Kampanya / fiyat kural motoru** (dönemsel indirim, 3 al 2 öde, 2. ürün indirimi, bundle, özellik bazlı kampanya)
2. **Muhasebe entegrasyonu (Bizim Hesap)**
3. **SMS gönderimi** (üyelik onayı + sipariş bilgilendirme)
4. **İYS uyumlu bülten** (onay kaydı + şablonlu gönderim)
5. **Kargo operasyonu** (entegrasyon, “kargoya verilenler” paneli, müşteriye kargo şirketi seçimi)
6. **Panelden manuel sipariş oluşturma**
7. **Toptan satış fiyatlandırması**, favoriler, stok gelince haber ver

Bu rapor, her madde için mevcut durumu, boşluğu ve önerilen önceliği özetler.

---

## 2. Teknoloji Özeti

| Katman | Teknoloji |
|--------|-----------|
| Framework | ASP.NET MVC 5 / .NET Framework 4.8 |
| Veri | Entity Framework 6 + SQL Server |
| Ödeme | Iyzico |
| Admin | Areas/Admin paneli |
| İçerik editörü | TinyMCE |
| Ön yüz | Bootstrap tabanlı responsive tema |
| E-posta | SMTP + Razor şablonları |
| Analitik | Google Analytics / GTM (yapıştırılabilir script) |

---

## 3. Karşılaştırma Matrisi

### 3.1 Teknik Altyapı

| Gereksinim | Durum | Not |
|------------|-------|-----|
| Bizim Hesap entegrasyonu | **Eksik** | Kodda referans yok |
| Responsive tasarım | **Mevcut** | Bootstrap + viewport; mobil uyumlu |
| SEO; panelde ürün bazlı meta/etiket | **Mevcut** | Meta, OG, Product Schema, sitemap, robots |
| Gelişmiş raporlama (ciro, tarih, müşteri sipariş sayısı, kategori filtresi) | **Kısmi** | Tarih/finans/ürün raporları var; müşteri bazlı sipariş sayısı ve kategori filtreli ciro yok |
| Pazaryeri entegrasyonu | **Eksik** (opsiyonel) | Trendyol/N11 vb. yok |
| İçerik yönetimi (blog, hakkımızda vb.) | **Mevcut** | Stories, Info sayfaları, menü sistemi |
| İletişim formu (ürün/sipariş sorusu vb.) | **Kısmi** | Form ve sebep alanı var; admin’den tip yönetimi zayıf |
| Google yorumlarını gösterme | **Eksik** | Sitedeki ürün yorumları var; Google Places/Reviews widget yok |
| Ticket / destek sistemi | **Eksik** | Helpdesk entity/controller yok |
| Hosting | **Mevcut** (altyapı) | IIS/Web.config, env tabanlı bağlantı |
| Mail hesapları / SMTP | **Mevcut** | SMTP ayarları panelde; şablonlu mail |
| Sipariş alındı e-postası | **Mevcut** | Order confirmation şablonları |
| Üyelikte SMS (işaretlendiyse) | **Eksik** | SmsService stub; sağlayıcı yok |
| Google Analytics | **Mevcut** | Panelden script ekleme |
| Akakçe / Cimri | **Eksik** (opsiyonel) | Özel feed/API yok; genel RSS mevcut |
| Kargo entegrasyonu + kargoya verilenler bölümü | **Kısmi** | Takip no / şirket adı / takip sayfası var; kargo API ve ayrı “kargoya verilenler” paneli yok |

### 3.2 Sipariş / Ürün Yönetimi

| Gereksinim | Durum | Not |
|------------|-------|-----|
| Panelden manuel sipariş | **Eksik** | Orders: liste + detay; Create yok |
| WhatsApp destek hattı | **Mevcut** | Ayar + ürün/footer linki |
| Canlı destek penceresi | **Mevcut** | Zopim/chat script alanı |
| Aktif sepet izleme | **Mevcut** | Admin sepet listesi/detay |
| Tarayıcı kapanınca öneri popup (exit-intent) | **Eksik** | — |
| Gelişmiş ürün filtreleme | **Mevcut** | Fiyat, kategori, etiket, marka, puan |
| Toptan satış (üye bazlı çift fiyat) | **Eksik** (opsiyonel) | Wholesale rol/fiyat yok |
| Ürün soru / yorum / puan / favori | **Kısmi** | Yorum+puan var; ürün bazlı Q&A zayıf; favori yok |
| Ürün medyası (5 foto + 1 video) | **Mevcut** | Galeri + VideoUrl |
| Ödeme ekranında sipariş notu | **Mevcut** | OrderComments |
| Stoğu bitende “gelince haber ver” | **Eksik** | AwaitingRestock durumu var; bildirim formu yok |

### 3.3 Pazarlama

| Gereksinim | Durum | Not |
|------------|-------|-----|
| Şablonlu haftalık bülten | **Kısmi** | Abone listesi var; gönderim motoru yok |
| İYS onay toplama | **Eksik** | İYS API / onay alanı abonede yok |
| Instagram paylaşımlarını listeleme | **Eksik** | Profil linki var; feed yok |
| İndirim kuponu (müşteri bazlı tercihen) | **Kısmi** | Global kupon var; müşteriye özel atama yok |
| Dönemsel ürün indirimi (otomatik eski fiyat) | **Kısmi / Eksik** | Kupon tarih aralığı ve kategori indirimi var; ürün bazlı otomatik dönemsel kampanya motoru yok |
| 3 al 2 öde | **Eksik** | — |
| 2. ürün % indirim | **Eksik** | — |
| Bundle / “yanına eklemek ister misiniz” | **Eksik** | — |
| Özellik bazlı kampanya (örn. renk) | **Eksik** (opsiyonel) | — |
| Kampanyalı ürün sepeti / paket | **Eksik** | — |
| Instagram üzerinden sipariş | **Eksik** (opsiyonel) | — |

### 3.4 Diğer Notlar

| Gereksinim | Durum | Not |
|------------|-------|-----|
| Çarkıfelek vb. gereksiz | **Uygun** | Yok |
| Gmail ile giriş gereksiz | **Uygun** | Sosyal login yok |
| KVKK popup | **Kısmi** | Cookie consent var; ana storefront’ta tam yaygın değil |
| Üyelik sözleşmesi güncelleme bildirimi | **Eksik** | Versiyonlu sözleşme / re-consent yok |
| Kart kaydet (Masterpass) gereksiz | **Uygun** | Yok |
| Font ve renk değiştirme | **Eksik** | Tema CSS sabit |
| Sınırsız kategori / alt kategori | **Mevcut** | ParentId ağacı |
| SKU değiştirmeden kategori değiştirme | **Mevcut** | ProductCode ve kategori bağımsız |
| Editörde renk seçeneği | **Mevcut** | TinyMCE |
| Ücretsiz kargo limiti değiştirilebilir | **Mevcut** | BasketMinTotalPriceForCargo |
| Müşteri kargo şirketi seçebilsin | **Eksik** | Tek sabit kargo ayarı |

---

## 4. Önceliklendirme

### P0 — En kritik (önce geliştirilmeli)

| # | Konu | Gerekçe | Etki alanı |
|---|------|---------|------------|
| 1 | **Kampanya / fiyat kural motoru** | Dönemsel indirim, 3 al 2 öde, 2. ürün %, bundle, özellik bazlı kampanya ve paket sepeti buna bağlı | Sepet, fiyat, ödeme, admin |
| 2 | **Bizim Hesap** | Sipariş → muhasebe/fatura senkronu | Sipariş durumu, dış API |
| 3 | **SMS** | Üyelik onayı + sipariş bilgilendirme | Hesap, sipariş, sağlayıcı (NetGSM vb.) |
| 4 | **İYS + bülten gönderimi** | Yasal izin + şablonlu üye iletişim | Abone, onay kaydı, e-posta/SMS sağlayıcı |

### P1 — Operasyon ve dönüşüm

| # | Konu | Gerekçe |
|---|------|---------|
| 5 | Kargo paneli + (mümkünse) API + müşteri seçimi | Günlük sevkiyat takibi |
| 6 | Manuel sipariş oluşturma | Telefon / WhatsApp siparişleri |
| 7 | Rapor genişletme (müşteri sipariş sayısı, kategori filtreli ciro) | Yönetim kararları |
| 8 | Toptan fiyat (opsiyonel ama iş modeli gerektiriyorsa P1) | B2B |
| 9 | Gelince haber ver | Stok dönüşümü |
| 10 | Favoriler | Üyelik değeri |

### P2 — Orta öncelik

- Müşteri bazlı kupon  
- Exit-intent / öneri popup  
- Google yorumları widget  
- Ticket sistemi  
- Tema font/renk paneli  
- Üyelik sözleşmesi güncelleme uyarısı  
- KVKK banner’ın tüm sitede tutarlı gösterimi  
- İletişim formu sebep tiplerinin admin yönetimi  

### P3 — Opsiyonel / düşük

- Pazaryeri entegrasyonu  
- Akakçe / Cimri feed  
- Instagram feed ve Instagram sipariş  

---

## 5. Önerilen Geliştirme Sırası

```
1. Kampanya + fiyat motoru
      ↓
2. Bizim Hesap + SMS + İYS/bülten
      ↓
3. Kargo paneli / seçim + manuel sipariş
      ↓
4. Raporlar + toptan + favori + stok bildirimi
      ↓
5. Pazarlama UX (exit popup, Google reviews, ticket, tema)
      ↓
6. Opsiyoneller (pazaryeri, fiyat karşılaştırma, Instagram)
```

Bu sıra, çekirdek fiyatlandırma ve yasal/operasyonel zorunlulukları önce tamamlar; bağımlı pazarlama özelliklerini sonrasına bırakır.

---

## 6. Teknik Riskler

| Risk | Açıklama |
|------|----------|
| Kampanya motoru | Sepet toplamı, kupon ve ödeme ile çakışabilir; kural önceliği ve test matrisi gerekir |
| Bizim Hesap | Dış API, sipariş durumu senkronu, hata/yeniden deneme |
| İYS | Onay kaydı tek başına yetmez; gönderim kanalı ve saklama zorunlulukları |
| Kargo API | Sağlayıcı seçimi ve panel UX’i erken netleştirilmeli |
| Toptan fiyat | Üye tipi, görünür fiyat ve vergi/kupon etkileşimi |

---

## 7. Sonuç

Mevcut sistem, **katalog–sepet–ödeme–sipariş–SEO–içerik** hattında üretim kalitesine yakındır. Gereksinim setinin karşılanması için asıl yatırım şu dört blokta yoğunlaşmalıdır:

1. **Kampanya / fiyat motoru**  
2. **Muhasebe (Bizim Hesap) + SMS + İYS/bülten**  
3. **Kargo operasyonu + manuel sipariş**  
4. **Raporlama genişletmesi ve dönüşüm özellikleri** (favori, stok bildirimi, toptan)

Bunlar tamamlanmadan platform, verilen gereksinim listesine göre “işlevsel mağaza” olsa bile “tam kapsamlı hedef sistem” olarak değerlendirilemez.

---

## 8. Durum Özeti (sayısal)

| Durum | Yaklaşık oran (ana maddeler) |
|-------|------------------------------|
| Mevcut | ~%40 |
| Kısmi | ~%20 |
| Eksik | ~%40 |

*Oranlar madde sayısına göre kabaca hesaplanmıştır; iş değeri açısından P0/P1 eksikler toplam iş yükünün büyük kısmını oluşturur.*
