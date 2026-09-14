-- ============================================================================
-- Seed Page Help Settings for Admin Contextual Modal
-- ============================================================================

DECLARE @Now DATETIME = GETDATE();
DECLARE @Lang INT = 1; -- Turkish (default)

;WITH PageHelpSettings AS (
    SELECT * FROM (VALUES
        (
            N'SystemUsers_help/info_section',
            N'<p>Bu sayfada sistem kullanıcılarını yönetirsiniz.</p><ul><li>Ad, soyad, e-posta veya kullanıcı adına göre arama yapabilirsiniz.</li><li>Rol (<strong>Yönetici</strong> / <strong>Kullanıcı</strong>) ve duruma göre filtreleme yapabilirsiniz.</li><li><strong>Yeni kullanıcı tanımla</strong> butonu ile sisteme kullanıcı ekleyebilirsiniz.</li><li>İşlemler menüsünden düzenleyebilir, şifre sıfırlayabilir veya hesabı askıya alabilirsiniz.</li></ul>',
            N'Admin System Users page contextual help section'
        ),
        (
            N'ProductPage_help/info_section',
            N'<p>Bu sayfada <strong>ürün</strong> oluşturur veya düzenlersiniz.</p><ul><li>Soldaki ağaçtan ürünün <strong>kategorisini</strong> seçin (zorunlu).</li><li><strong>Fiyat</strong>: Satış fiyatı. <strong>Ürün indirimi</strong> varsa fiyat üzerinden ek indirim uygulanır.</li><li><strong>Ana sayfa</strong>: Ürünü ana sayfada gösterir. <strong>Kampanya</strong>: Kampanya listelerinde öne çıkarır.</li><li><strong>Marka / etiketler</strong>: Filtreleme ve vitrin için kullanılır.</li><li><strong>Ürün Ölçü / Renk Seçenekleri</strong>: Mağaza ürün detayında beden ve renk açılır listeleri için virgülle ayırın (ör. <code>S,M,L,XL</code>).</li><li>Kategori şablonuna bağlı ek özellikler ürün listesinden “özellik” ikonuyla doldurulur.</li></ul>',
            N'Admin Product page contextual help section'
        ),
        (
            N'ProductCategories_help/info_section',
            N'<p>Ürün kategorisi ekleme ve düzenleme sayfasıdır.</p><ul><li>Kategori adı, üst kategori ve sıra numarasını belirleyin.</li><li>Kategoriye ait SEO başlık ve açıklamalarını doldurun.</li><li>Kategori görseli ekleyerek vitrinde şık görünmesini sağlayın.</li></ul>',
            N'Admin Product Categories contextual help section'
        ),
        (
            N'Brands_help/info_section',
            N'<p>Marka yönetimi sayfasıdır.</p><ul><li>Marka adını ve varsa logosunu ekleyin.</li><li>Markaları ürünlerle ilişkilendirerek filtrelemeyi kolaylaştırın.</li></ul>',
            N'Admin Brands contextual help section'
        ),
        (
            N'Coupons_help/info_section',
            N'<p>İndirim kuponu oluşturma ve düzenleme sayfasıdır.</p><ul><li>Kupon kodu, indirim oranı veya tutarını tanımlayın.</li><li>Başlangıç ve bitiş tarihlerini belirleyin.</li><li>Minimum sepet tutarı ve kullanım limitlerini yapılandırın.</li></ul>',
            N'Admin Coupons contextual help section'
        ),
        (
            N'Faq_help/info_section',
            N'<p>Sıkça Sorulan Sorular (SSS) yönetimidir.</p><ul><li>Müşterilerin sık sorduğu soruları ve yanıtlarını ekleyin.</li><li>Soru sırasını belirleyerek mağaza ön yüzünde düzenli görünmesini sağlayın.</li></ul>',
            N'Admin FAQ contextual help section'
        )
    ) v(SettingKey, SettingValue, Description)
)
MERGE INTO dbo.Settings AS target
USING PageHelpSettings AS source
ON (target.SettingKey = source.SettingKey AND target.Lang = @Lang)
WHEN MATCHED THEN
    UPDATE SET
        target.SettingValue = source.SettingValue,
        target.UpdatedDate = @Now,
        target.Description = source.Description
WHEN NOT MATCHED THEN
    INSERT (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Description, SettingKey, SettingValue)
    VALUES (N'Help: ' + source.SettingKey, @Now, @Now, 1, 0, @Lang, source.Description, source.SettingKey, source.SettingValue);

PRINT 'Page help settings seeded successfully.';
