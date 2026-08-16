# Medya ve Seed Resim Yönetimi Rehberi (Media & Seed Images Guide)

Bu doküman, EImece projesinde resimlerin ve galeri dosyalarının **Storefront** ile **Admin Paneli** arasındaki çalışma mantığını, veritabanı ilişkilerini ve seed (örnek) verilerin neden bazen admin panelinde görünmediğini açıklamaktadır.

---

## 1. Mimari Genel Bakış

EImece'de resim yönetimi iki katmandan oluşur:

1. **Veritabanı Katmanı (Metadata & İlişkiler):**
   * [`dbo.Menus`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Entities/Menu.cs) / [`dbo.Products`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Entities/Product.cs) / [`dbo.Stories`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Entities/Story.cs): Ana görsel ID'sini (`MainImageId`) ve durumunu (`ImageState`) tutar.
   * [`dbo.FileStorages`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Entities/FileStorage.cs): Dosya adı (`FileName`), dosya tipi (`Type`: `MenuMainImage`, `MenuGallery`, `ProductMainImage`, vb.), boyut ve mime type bilgilerini tutar.
   * [`dbo.MenuFiles`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Entities/MenuFile.cs) / [`dbo.ProductFiles`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Entities/ProductFile.cs) / [`dbo.StoryFiles`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Entities/StoryFile.cs): İçerik kaydı (`MenuId`, `ProductId`, `StoryId`) ile `FileStorageId` arasındaki çoka-çok ilişkiyi kurar.

2. **Dosya Sistemi Katmanı (Physical Storage):**
   * Yüklenen ve üretilen fiziksel dosyalar `~/media/images/` (`AppConfig.StorageRoot`) dizininde saklanır.
   * Küçük resimler (thumbnails) ise `~/media/images/thumbs/thb{FileName}` yolunda yer alır.

---

## 2. Storefront ile Admin Paneli Arasındaki Davranış Farkı

| Özellik | Storefront (`/i/...`, `/p/...`, `/s/...`) | Admin Paneli (`/admin/...`) |
| :--- | :--- | :--- |
| **Resim Çağırma Yöntemi** | Dinamik proxy URL'leri üretir (`/images/w500h500/{id}.jpg`). | Diskteki fiziksel dosya varlığını doğrudan kontrol eder (`File.Exists`). |
| **Fiziksel Dosya Yoksa** | Varsayılan resim veya tema placeholder bloğu (`pt-ph`, `default.jpg`) render edilir. | Menüde **"Resim Yok"**, Medya galerisinde ise **"Dosya Bulunamadı"** uyarısı gösterilir. |
| **Galeri Sayfası** | Sayfa teması doğrudan `MenuFiles` listesini HTML galerisi olarak çizer. | `Admin/Media` sayfası iki sekmelidir; varsayılan olarak **Dosya Yükleme** sekmesiyle açılır. |

---

## 3. Seed Verileri Neden Admin Panelinde Görünmeyebilir?

Eğer veritabanına SQL script'leri (`SeedDummyData.sql` veya `SeedThemePages.sql`) doğrudan SQL Server Management Studio (SSMS) üzerinden çalıştırıldıysa:

1. **Fiziksel JPEG Dosyaları Eksiktir:**
   * SQL script'i yalnızca veritabanındaki tablolara kayıt atar, diskteki `~/media/images/` klasörüne fiziksel JPEG dosyalarını yazmaz.
   * Admin paneli [`FilesHelper.IsMainImageExists`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Helpers/FilesHelper.cs) ile `File.Exists("~/media/images/" + FileName)` kontrolü yaptığı için, diskte dosya bulunamayınca **"Resim Yok"** veya **"Dosya Bulunamadı"** hatası verir.

2. **Medya Yöneticisinde 1. Sekmede Kalınması:**
   * Admin medya sayfasına (`/admin/media/?contentId=6311&mod=Menus&imageType=MenuGallery`) gidildiğinde sayfa varsayılan olarak **"Dosya Yükleme"** (upload formu) sekmesinde açılır.
   * Veritabanında kayıtlı görselleri görmek için üstteki **"Yüklenen Dosyalar"** (2. sekme) sekmesine geçilmelidir.

---

## 4. Fiziksel Seed Resimlerini Oluşturma ve Eşitleme

Fiziksel dummy resim dosyalarını diske oluşturmak için PowerShell script'i kullanılmalıdır:

### Yalnızca Eksik Resim Dosyalarını Üretmek İçin:
```powershell
cd EImece\EImece\SqlScripts
.\RunSeedDummyData.ps1 -ImagesOnly
```

### Yalnızca Sayfa Temaları (PT Dummy T1–T8) ve Resimlerini Güncellemek İçin:
```powershell
cd EImece\EImece\SqlScripts
.\RunSeedDummyData.ps1 -ThemePages
```

### Tam Veritabanı ve Resim Seed'i (Temiz Kurulum):
```powershell
cd EImece\EImece\SqlScripts
.\RunSeedDummyData.ps1 -SeedDatabase
```

### Dummy Seed Verilerini Temizlemek İçin:
```powershell
cd EImece\EImece\SqlScripts
.\RunSeedDummyData.ps1 -CleanupDatabase
```

---

## 5. Menü / Sayfa URL ve ID Çözümleme (Örnek: PT Dummy T1)

* **Storefront URL:** `http://localhost:81/i/pt-dummy-t1-1b1b2d6g/`
* **Route:** `PagesController.Detail("pt-dummy-t1-1b1b2d6g")`
* **ID Çözümleme:** `1b1b2d6g` slug son eki [`GeneralHelper.RevertId`](file:///c:/Users/eminy/source/repos/EImece/EImece/EImece.Domain/Helpers/GeneralHelper.cs) ile **`6311`** menü ID'sine çözülür.
* **Admin Menü Sayfası:** `http://localhost:81/admin/menus/saveoredit/6311/`
* **Admin Galeri Sayfası:** `http://localhost:81/admin/media/?contentId=6311&mod=Menus&imageType=MenuGallery` (Sekme 2: *Yüklenen Dosyalar*)
