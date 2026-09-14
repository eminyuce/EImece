using EImece.Domain.Caching;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers
{
    public class PageHelpItemDto
    {
        public string Key { get; set; }
        public string PageName { get; set; }
        public string Category { get; set; }
        public string RouteInfo { get; set; }
        public string DefaultContent { get; set; }
        public string CurrentContent { get; set; }
        public bool IsCustomized { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool HasContent => !string.IsNullOrWhiteSpace(CurrentContent) || !string.IsNullOrWhiteSpace(DefaultContent);
    }

    public static class PageHelpHelper
    {
        public const string KeySuffix = "_help/info_section";

        private static readonly Dictionary<string, string> SlugMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "change-password", "Users_ChangePassword_help/info_section" },
            { "users-changepassword", "Users_ChangePassword_help/info_section" },
            { "users", "SystemUsers_help/info_section" },
            { "system-users", "SystemUsers_help/info_section" },
            { "register-user", "Users_Register_help/info_section" },
            { "user-register", "Users_Register_help/info_section" },
            { "users-register", "Users_Register_help/info_section" },
            { "edit-user", "Users_Edit_help/info_section" },
            { "user-edit", "Users_Edit_help/info_section" },
            { "users-edit", "Users_Edit_help/info_section" },
            { "products", "ProductPage_help/info_section" },
            { "product-page", "ProductPage_help/info_section" },
            { "product-specs", "ProductSpecs_help/info_section" },
            { "products-move", "Products_Move_help/info_section" },
            { "product-categories", "ProductCategories_help/info_section" },
            { "categories", "ProductCategories_help/info_section" },
            { "categories-move", "ProductCategories_Move_help/info_section" },
            { "brands", "Brands_help/info_section" },
            { "coupons", "Coupons_help/info_section" },
            { "faq", "Faq_help/info_section" },
            { "lists", "Lists_help/info_section" },
            { "mail-templates", "MailTemplates_help/info_section" },
            { "main-page-images", "MainPageImages_help/info_section" },
            { "banners", "MainPageImages_help/info_section" },
            { "menus", "Menus_help/info_section" },
            { "menus-move", "Menus_Move_help/info_section" },
            { "metrics", "Metrics_help/info_section" },
            { "website-logo", "WebSiteLogo_help/info_section" },
            { "logo", "WebSiteLogo_help/info_section" },
            { "stories", "Stories_help/info_section" },
            { "story-categories", "StoryCategories_help/info_section" },
            { "tag-categories", "TagCategories_help/info_section" },
            { "tags", "Tags_help/info_section" },
            { "templates", "Templates_help/info_section" },
            { "admin-settings", "AdminSettings_help/info_section" },
            { "adminsettings", "AdminSettings_help/info_section" },
            { "adminsettings_index", "AdminSettings_help/info_section" },
            { "adminsettings-index", "AdminSettings_help/info_section" },
            { "system-settings", "SystemSettings_help/info_section" },
            { "systemsettings", "SystemSettings_help/info_section" },
            { "adminsettings_systemsettings", "SystemSettings_help/info_section" },
            { "adminsettings-systemsettings", "SystemSettings_help/info_section" }
        };

        public const string SettingNullValue = "RETURN-NULL-VALUE";

        public static bool IsValidContent(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            string trimmed = value.Trim();
            if (string.Equals(trimmed, SettingNullValue, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (trimmed.Contains("<") || trimmed.Contains("&nbsp;"))
            {
                if (trimmed.IndexOf("<img", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trimmed.IndexOf("<iframe", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trimmed.IndexOf("<video", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trimmed.IndexOf("<svg", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                string stripped = System.Text.RegularExpressions.Regex.Replace(trimmed, "<[^>]*>", string.Empty);
                stripped = stripped.Replace("&nbsp;", " ").Trim();
                if (string.IsNullOrWhiteSpace(stripped))
                {
                    return false;
                }
            }

            return true;
        }

        public static string ResolveKey(string pageKey)
        {
            if (string.IsNullOrWhiteSpace(pageKey))
            {
                return string.Empty;
            }

            string clean = pageKey.Trim().Trim('/').Trim('\\');

            // Strip prefix if someone calls "api/help/change-password"
            if (clean.StartsWith("api/help/", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring("api/help/".Length);
            }

            if (SlugMap.TryGetValue(clean, out string mappedKey))
            {
                return mappedKey;
            }

            // If it already ends with key suffix
            if (clean.EndsWith(KeySuffix, StringComparison.OrdinalIgnoreCase))
            {
                return clean;
            }

            string cleanNoAdmin = clean.StartsWith("admin/", StringComparison.OrdinalIgnoreCase)
                ? clean.Substring("admin/".Length)
                : (clean.StartsWith("admin_", StringComparison.OrdinalIgnoreCase)
                    ? clean.Substring("admin_".Length)
                    : (clean.StartsWith("admin-", StringComparison.OrdinalIgnoreCase)
                        ? clean.Substring("admin-".Length)
                        : clean));

            if (SlugMap.TryGetValue(cleanNoAdmin, out mappedKey))
            {
                return mappedKey;
            }

            // Check if matches a predefined page's Key or RouteInfo
            var matched = GetPredefinedPages().FirstOrDefault(p =>
            {
                if (string.Equals(p.Key, clean, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.RouteInfo, clean, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.RouteInfo?.Replace('/', '_'), clean, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.RouteInfo?.Replace('/', '-'), clean, StringComparison.OrdinalIgnoreCase) ||
                    (p.RouteInfo != null && (clean + "/index").Equals(p.RouteInfo, StringComparison.OrdinalIgnoreCase)) ||
                    (p.RouteInfo != null && (clean + "_index").Equals(p.RouteInfo.Replace('/', '_'), StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(p.RouteInfo))
                {
                    string rNoAdmin = p.RouteInfo.StartsWith("Admin/", StringComparison.OrdinalIgnoreCase)
                        ? p.RouteInfo.Substring("Admin/".Length)
                        : p.RouteInfo;

                    if (string.Equals(rNoAdmin, clean, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rNoAdmin, cleanNoAdmin, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rNoAdmin.Replace('/', '_'), clean, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rNoAdmin.Replace('/', '_'), cleanNoAdmin, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rNoAdmin.Replace('/', '-'), clean, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rNoAdmin.Replace('/', '-'), cleanNoAdmin, StringComparison.OrdinalIgnoreCase) ||
                        (clean + "/Index").Equals(rNoAdmin, StringComparison.OrdinalIgnoreCase) ||
                        (clean + "_Index").Equals(rNoAdmin.Replace('/', '_'), StringComparison.OrdinalIgnoreCase) ||
                        (cleanNoAdmin + "/Index").Equals(rNoAdmin, StringComparison.OrdinalIgnoreCase) ||
                        (cleanNoAdmin + "_Index").Equals(rNoAdmin.Replace('/', '_'), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            });

            if (matched != null)
            {
                return matched.Key;
            }

            return NormalizeKey(clean);
        }

        public static string GetPageTitle(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return "Sayfa Yardım Rehberi";

            string cleanKey = ResolveKey(key);
            var page = GetPredefinedPages().FirstOrDefault(p => string.Equals(p.Key, cleanKey, StringComparison.OrdinalIgnoreCase));
            if (page != null && !string.IsNullOrWhiteSpace(page.PageName))
            {
                return page.PageName;
            }

            string raw = cleanKey.Replace(KeySuffix, "").Replace('_', ' ');
            return raw;
        }

        public static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            string clean = key.Trim();
            if (!clean.EndsWith(KeySuffix, StringComparison.OrdinalIgnoreCase))
            {
                clean += KeySuffix;
            }

            return clean;
        }

        public static List<PageHelpItemDto> GetPredefinedPages()
        {
            return new List<PageHelpItemDto>
            {
                new PageHelpItemDto
                {
                    Key = "SystemUsers_help/info_section",
                    PageName = "Sistem Kullanıcıları",
                    Category = "Kullanıcılar",
                    RouteInfo = "Admin/Users/Index",
                    DefaultContent = "<p>Bu sayfada sistem kullanıcılarını yönetirsiniz.</p><ul><li>Ad, soyad, e-posta veya kullanıcı adına göre arama yapabilirsiniz.</li><li>Rol (<strong>Yönetici</strong> / <strong>Kullanıcı</strong>) ve duruma göre filtreleme yapabilirsiniz.</li><li><strong>Yeni kullanıcı tanımla</strong> butonu ile sisteme kullanıcı ekleyebilirsiniz.</li><li>İşlemler menüsünden düzenleyebilir, şifre sıfırlayabilir veya hesabı askıya alabilirsiniz.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Users_Register_help/info_section",
                    PageName = "Yeni Kullanıcı Tanımlama",
                    Category = "Kullanıcılar",
                    RouteInfo = "Admin/Users/Register",
                    DefaultContent = "<p>Sisteme yeni bir kullanıcı veya yönetici ekleme sayfasıdır.</p><ul><li>Kullanıcının e-posta, kullanıcı adı ve geçici şifresini belirleyin.</li><li>Kullanıcıya uygun rolleri (Yönetici, Editör vb.) atayın.</li><li>Kayıt sonrası kullanıcıya bildirim iletilmesini sağlayabilirsiniz.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Users_Edit_help/info_section",
                    PageName = "Kullanıcı Bilgilerini Düzenleme",
                    Category = "Kullanıcılar",
                    RouteInfo = "Admin/Users/Edit",
                    DefaultContent = "<p>Mevcut bir kullanıcının profil ve yetkilerini güncelleme sayfasıdır.</p><ul><li>E-posta, ad-soyad ve telefon bilgilerini güncelleyin.</li><li>Hesap aktiflik veya askıya alma durumunu değiştirin.</li><li>Kullanıcı rollerini güncelleyip kaydedin.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Users_ChangePassword_help/info_section",
                    PageName = "Kullanıcı Şifre Değiştirme",
                    Category = "Kullanıcılar",
                    RouteInfo = "Admin/Users/ChangePassword",
                    DefaultContent = "<p>Kullanıcının şifresini güvenli şekilde sıfırlama veya güncelleme sayfasıdır.</p><ul><li>Güçlü bir yeni şifre belirleyin (en az bir büyük harf, rakam ve sembol).</li><li>İşlem tamamlandığında kullanıcının mevcut oturumları sonlandırılabilir.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "ProductPage_help/info_section",
                    PageName = "Ürün Tanımlama & Düzenleme",
                    Category = "Katalog",
                    RouteInfo = "Admin/Products/SaveOrEdit",
                    DefaultContent = "<p>Bu sayfada <strong>ürün</strong> oluşturur veya düzenlersiniz.</p><ul><li>Soldaki ağaçtan ürünün <strong>kategorisini</strong> seçin (zorunlu).</li><li><strong>Fiyat</strong>: Satış fiyatı. <strong>Ürün indirimi</strong> varsa fiyat üzerinden ek indirim uygulanır.</li><li><strong>Ana sayfa</strong>: Ürünü ana sayfada gösterir. <strong>Kampanya</strong>: Kampanya listelerinde öne çıkarır.</li><li><strong>Marka / etiketler</strong>: Filtreleme ve vitrin için kullanılır.</li><li><strong>Ürün Ölçü / Renk Seçenekleri</strong>: Mağaza ürün detayında beden ve renk açılır listeleri için virgülle ayırın (ör. <code>S,M,L,XL</code>).</li><li>Kategori şablonuna bağlı ek özellikler ürün listesinden “özellik” ikonuyla doldurulur.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "ProductSpecs_help/info_section",
                    PageName = "Ürün Teknik Özellikleri",
                    Category = "Katalog",
                    RouteInfo = "Admin/Products/SaveOrEditProductSpecs",
                    DefaultContent = "<p>Ürüne kategori şablonu kapsamında dinamik teknik özellikler ekleme sayfasıdır.</p><ul><li>Kategoriye tanımlanmış özellik alanları (Materyal, Boyut, İşlemci vb.) listelenir.</li><li>Ürüne uygun değerleri seçin veya serbest metin olarak doldurun.</li><li>Vitrin filtrelerinde bu özelliklerin görünmesini sağlayabilirsiniz.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Products_Move_help/info_section",
                    PageName = "Ürünleri Kategoriye Taşıma",
                    Category = "Katalog",
                    RouteInfo = "Admin/Products/MoveProductsInTrees",
                    DefaultContent = "<p>Ürünlerin kategorisini toplu veya tek tek taşıma sayfasıdır.</p><ul><li>Kaynak ve hedef kategorileri seçin.</li><li>Taşınacak ürünleri işaretleyerek tek tıkla yeni kategoriye aktarın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "ProductCategories_help/info_section",
                    PageName = "Ürün Kategorileri",
                    Category = "Katalog",
                    RouteInfo = "Admin/ProductCategories/SaveOrEdit",
                    DefaultContent = "<p>Ürün kategorisi ekleme ve düzenleme sayfasıdır.</p><ul><li>Kategori adı, üst kategori ve sıra numarasını belirleyin.</li><li>Kategoriye ait SEO başlık ve açıklamalarını doldurun.</li><li>Kategori görseli ekleyerek vitrinde şık görünmesini sağlayın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "ProductCategories_Move_help/info_section",
                    PageName = "Kategori Ağacı Taşıma",
                    Category = "Katalog",
                    RouteInfo = "Admin/ProductCategories/MoveProductCategory",
                    DefaultContent = "<p>Kategori hiyerarşisini ve alt kategorileri yeniden konumlandırma sayfasıdır.</p><ul><li>Sürükle bırak veya seçim yöntemiyle kategoriyi yeni üst dal altına taşıyın.</li><li>URL yönlendirmeleri ve menü hiyerarşisi otomatik güncellenir.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Brands_help/info_section",
                    PageName = "Marka Yönetimi",
                    Category = "Katalog",
                    RouteInfo = "Admin/Brands/SaveOrEdit",
                    DefaultContent = "<p>Marka yönetimi sayfasıdır.</p><ul><li>Marka adını ve varsa logosunu ekleyin.</li><li>Markaları ürünlerle ilişkilendirerek filtrelemeyi kolaylaştırın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Coupons_help/info_section",
                    PageName = "İndirim Kuponları",
                    Category = "Pazarlama",
                    RouteInfo = "Admin/Coupons/SaveOrEdit",
                    DefaultContent = "<p>İndirim kuponu oluşturma ve düzenleme sayfasıdır.</p><ul><li>Kupon kodu, indirim oranı veya tutarını tanımlayın.</li><li>Başlangıç ve bitiş tarihlerini belirleyin.</li><li>Minimum sepet tutarı ve kullanım limitlerini yapılandırın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Faq_help/info_section",
                    PageName = "Sıkça Sorulan Sorular (SSS)",
                    Category = "İçerik",
                    RouteInfo = "Admin/Faq/SaveOrEdit",
                    DefaultContent = "<p>Sıkça Sorulan Sorular (SSS) yönetimidir.</p><ul><li>Müşterilerin sık sorduğu soruları ve yanıtlarını ekleyin.</li><li>Soru sırasını belirleyerek mağaza ön yüzünde düzenli görünmesini sağlayın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Lists_help/info_section",
                    PageName = "Özel Ürün Listeleri",
                    Category = "Katalog",
                    RouteInfo = "Admin/Lists/SaveOrEdit",
                    DefaultContent = "<p>Özel ürün listesi tanımlama ve vitrin grupları oluşturma sayfasıdır.</p><ul><li>Öne çıkanlar, fırsat ürünleri gibi listeler oluşturun.</li><li>Listeye eklenecek ürünleri seçin ve sıralayın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "MailTemplates_help/info_section",
                    PageName = "E-posta Şablonları",
                    Category = "İletişim",
                    RouteInfo = "Admin/MailTemplates/SaveOrEdit",
                    DefaultContent = "<p>Sistem e-posta bildirim şablonlarını yönetme sayfasıdır.</p><ul><li>Sipariş, üyelik ve şifre sıfırlama gibi şablonları düzenleyin.</li><li>Dinamik Razor değişkenleri ile kişiselleştirilmiş içerik oluşturun.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "MainPageImages_help/info_section",
                    PageName = "Ana Sayfa Görselleri / Banner",
                    Category = "Tasarım",
                    RouteInfo = "Admin/MainPageImages/SaveOrEdit",
                    DefaultContent = "<p>Ana sayfa slider ve vitrin bannerlarını yönetme sayfasıdır.</p><ul><li>Görsel yükleyin, hedef bağlantı linkini ve başlığı belirleyin.</li><li>Sıralama ve aktiflik durumunu yönetin.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Menus_help/info_section",
                    PageName = "Menü Yönetimi",
                    Category = "Tasarım",
                    RouteInfo = "Admin/Menus/SaveOrEdit",
                    DefaultContent = "<p>Üst ve alt gezinti menülerini düzenleme sayfasıdır.</p><ul><li>Menü başlığı, hedef URL ve açılış biçimini belirleyin.</li><li>Menü elemanlarını sıralayın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Menus_Move_help/info_section",
                    PageName = "Menü Ağacı Düzenleme",
                    Category = "Tasarım",
                    RouteInfo = "Admin/Menus/MoveMenuCategory",
                    DefaultContent = "<p>Menülerin hiyerarşik sıralamasını ve alt menü kırılımlarını düzenleme sayfasıdır.</p><ul><li>Menü dallarını sürükleyerek alt menü oluşturun veya sırasını değiştirin.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Metrics_help/info_section",
                    PageName = "Sistem & Performans Metrikleri",
                    Category = "Sistem",
                    RouteInfo = "Admin/Metrics/Index",
                    DefaultContent = "<p>Sistem kaynakları ve istek performans metriklerini izleme sayfasıdır.</p><ul><li>Yanıt süreleri, bellek kullanımı ve veritabanı sorgu sürelerini inceleyin.</li><li>Trafik yoğunluklarını ve darboğazları tespit edin.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "WebSiteLogo_help/info_section",
                    PageName = "Site Logo Yönetimi",
                    Category = "Tasarım",
                    RouteInfo = "Admin/Settings/WebSiteLogo",
                    DefaultContent = "<p>Mağaza logosunu ve faviconu güncelleme sayfasıdır.</p><ul><li>Yüksek çözünürlüklü şeffaf PNG veya SVG logo yükleyin.</li><li>Farklı tema ve arka planlar için uygun renk kontrastını kontrol edin.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Stories_help/info_section",
                    PageName = "Hikaye / Story Yönetimi",
                    Category = "İçerik",
                    RouteInfo = "Admin/Stories/SaveOrEdit",
                    DefaultContent = "<p>Mağaza ana sayfasındaki hikaye / story paylaşımlarını yönetme sayfasıdır.</p><ul><li>Hikaye başlığı, görseli ve yönlendirme bağlantısını ekleyin.</li><li>Yayınlanma ve bitiş tarihlerini belirleyin.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "StoryCategories_help/info_section",
                    PageName = "Hikaye Kategorileri",
                    Category = "İçerik",
                    RouteInfo = "Admin/StoryCategories/SaveOrEdit",
                    DefaultContent = "<p>Hikayeleri gruplamak için kategori oluşturma sayfasıdır.</p><ul><li>Kategori adı ve kapak simgesini tanımlayın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "TagCategories_help/info_section",
                    PageName = "Etiket Kategorileri",
                    Category = "Katalog",
                    RouteInfo = "Admin/TagCategories/SaveOrEdit",
                    DefaultContent = "<p>Ürün etiketlerini sınıflandırmak için etiket grupları tanımlama sayfasıdır.</p><ul><li>Grup adı ve filtreleme özelliklerini yapılandırın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Tags_help/info_section",
                    PageName = "Etiket Yönetimi",
                    Category = "Katalog",
                    RouteInfo = "Admin/Tags/SaveOrEdit",
                    DefaultContent = "<p>Ürün filtreleme ve vitrin etiketlerini yönetme sayfasıdır.</p><ul><li>Etiket adı, ilgili kategori ve SEO dostu URL tanımlayın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "Templates_help/info_section",
                    PageName = "Şablon Yönetimi",
                    Category = "Katalog",
                    RouteInfo = "Admin/Templates/SaveOrEdit",
                    DefaultContent = "<p>Ürün kategori özellikleri için şablon tanımlama sayfasıdır.</p><ul><li>Kategori bazlı dinamik form alanlarını ve teknik özellikleri şablonlaştırın.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "AdminSettings_help/info_section",
                    PageName = "Genel Mağaza Ayarları",
                    Category = "Ayarlar",
                    RouteInfo = "Admin/AdminSettings/Index",
                    DefaultContent = "<p>Mağaza adı, firma iletişim bilgileri, fatura adresi ve sosyal medya bağlantılarını yönetirsiniz.</p><ul><li>Firma unvanı, telefon ve adres alanlarını güncel tutun.</li><li>Sosyal medya profil bağlantılarını girin.</li></ul>"
                },
                new PageHelpItemDto
                {
                    Key = "SystemSettings_help/info_section",
                    PageName = "Sistem Ayarları Merkezi",
                    Category = "Ayarlar",
                    RouteInfo = "Admin/AdminSettings/SystemSettings",
                    DefaultContent = "<p>Sistem ayarları merkezinde mağazanızın teknik, güvenlik, SMTP, PWA ve sayfa yardım rehberlerini yönetirsiniz.</p><ul><li>Görünüm, medya, güvenlik ve e-posta ayarlarını sekmelerden yapılandırın.</li><li>Sayfa Yardım Rehberleri sekmesinden admin sayfalarındaki yardım içeriklerini ekleyip düzenleyin.</li></ul>"
                }
            };
        }

        public static string GetDefaultContent(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            string cleanKey = NormalizeKey(key);
            var predefined = GetPredefinedPages().FirstOrDefault(p => string.Equals(p.Key, cleanKey, StringComparison.OrdinalIgnoreCase) || string.Equals(p.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));
            if (predefined != null && !string.IsNullOrWhiteSpace(predefined.DefaultContent))
            {
                return predefined.DefaultContent;
            }

            return null;
        }

        public static string GetGenericTemplate(string pageName = null)
        {
            string title = !string.IsNullOrWhiteSpace(pageName) ? pageName : "işlemlerinizi";
            return $"<p>Bu sayfada <strong>{title}</strong> gerçekleştirirsiniz.</p>\n" +
                   "<ul>\n" +
                   "  <li>Arama ve filtreleme alanlarını kullanarak kayıtlara hızlıca ulaşabilirsiniz.</li>\n" +
                   "  <li>İşlemler sütunundaki butonlar ile detayları düzenleyebilir veya silebilirsiniz.</li>\n" +
                   "</ul>";
        }

        #region Caching & Retrieval

        private static readonly object FallbackLock = new object();
        private static Dictionary<string, PageHelpItemDto> _fallbackCache;

        private static IEimeceCacheProvider ResolveCacheProvider(IEimeceCacheProvider explicitProvider)
        {
            if (explicitProvider != null) return explicitProvider;
            try
            {
                return DomainServiceProvider.GetService<IEimeceCacheProvider>();
            }
            catch { }
            return null;
        }

        private static ISettingService ResolveSettingService(ISettingService explicitService)
        {
            if (explicitService != null) return explicitService;
            try
            {
                return DomainServiceProvider.GetService<ISettingService>();
            }
            catch { }
            return null;
        }

        public static void EvictCache(IEimeceCacheProvider cacheProvider = null)
        {
            lock (FallbackLock)
            {
                _fallbackCache = null;
            }

            var provider = ResolveCacheProvider(cacheProvider);
            if (provider != null)
            {
                provider.Clear(CacheKeys.PageHelpDictionary);
                provider.Clear(CacheKeys.PageHelpDictionary + "_ASYNC");
                provider.ClearByPrefix(CacheKeys.PageHelpPrefix);
            }
        }

        public static Dictionary<string, PageHelpItemDto> GetAllHelpTexts(ISettingService settingService = null, IEimeceCacheProvider cacheProvider = null)
        {
            var provider = ResolveCacheProvider(cacheProvider);
            var service = ResolveSettingService(settingService);

            if (provider != null)
            {
                try
                {
                    var cached = provider.GetOrAdd(
                        CacheKeys.PageHelpDictionary,
                        () => BuildAllHelpTextsDictionary(service),
                        AppConfig.CacheLongSeconds);
                    if (cached != null) return cached;
                }
                catch { }
            }

            lock (FallbackLock)
            {
                if (_fallbackCache == null)
                {
                    _fallbackCache = BuildAllHelpTextsDictionary(service);
                }
                return _fallbackCache;
            }
        }

        public static async Task<Dictionary<string, PageHelpItemDto>> GetAllHelpTextsAsync(ISettingService settingService = null, IEimeceCacheProvider cacheProvider = null)
        {
            var provider = ResolveCacheProvider(cacheProvider);
            var service = ResolveSettingService(settingService);

            if (provider != null)
            {
                try
                {
                    var cached = await provider.GetOrAddAsync(
                        CacheKeys.PageHelpDictionary + "_ASYNC",
                        async () => await BuildAllHelpTextsDictionaryAsync(service).ConfigureAwait(false),
                        AppConfig.CacheLongSeconds).ConfigureAwait(false);
                    if (cached != null) return cached;
                }
                catch { }
            }

            lock (FallbackLock)
            {
                if (_fallbackCache != null)
                {
                    return _fallbackCache;
                }
            }

            var dict = await BuildAllHelpTextsDictionaryAsync(service).ConfigureAwait(false);
            lock (FallbackLock)
            {
                _fallbackCache = dict;
            }
            return dict;
        }

        private static Dictionary<string, PageHelpItemDto> BuildAllHelpTextsDictionary(ISettingService settingService)
        {
            var result = new Dictionary<string, PageHelpItemDto>(StringComparer.OrdinalIgnoreCase);

            foreach (var page in GetPredefinedPages())
            {
                result[page.Key] = new PageHelpItemDto
                {
                    Key = page.Key,
                    PageName = page.PageName,
                    Category = page.Category,
                    RouteInfo = page.RouteInfo,
                    DefaultContent = page.DefaultContent,
                    CurrentContent = page.DefaultContent,
                    IsCustomized = false,
                    UpdatedDate = null
                };
            }

            if (settingService != null)
            {
                List<Setting> allSettings = null;
                try
                {
                    allSettings = settingService.GetAllActiveSettings();
                }
                catch { }

                if (allSettings != null && allSettings.Count > 0)
                {
                    MergeSettingsIntoDictionary(result, allSettings);
                }
            }

            return result;
        }

        private static async Task<Dictionary<string, PageHelpItemDto>> BuildAllHelpTextsDictionaryAsync(ISettingService settingService)
        {
            var result = new Dictionary<string, PageHelpItemDto>(StringComparer.OrdinalIgnoreCase);

            foreach (var page in GetPredefinedPages())
            {
                result[page.Key] = new PageHelpItemDto
                {
                    Key = page.Key,
                    PageName = page.PageName,
                    Category = page.Category,
                    RouteInfo = page.RouteInfo,
                    DefaultContent = page.DefaultContent,
                    CurrentContent = page.DefaultContent,
                    IsCustomized = false,
                    UpdatedDate = null
                };
            }

            if (settingService != null)
            {
                List<Setting> allSettings = null;
                try
                {
                    allSettings = await settingService.GetAllActiveSettingsAsync().ConfigureAwait(false);
                }
                catch { }

                if (allSettings == null || allSettings.Count == 0)
                {
                    try
                    {
                        allSettings = await settingService.GetAllAsync().ConfigureAwait(false);
                    }
                    catch { }
                }

                if (allSettings == null || allSettings.Count == 0)
                {
                    try
                    {
                        allSettings = settingService.GetAllActiveSettings();
                    }
                    catch { }
                }

                if (allSettings != null && allSettings.Count > 0)
                {
                    MergeSettingsIntoDictionary(result, allSettings);
                }
            }

            return result;
        }

        private static void MergeSettingsIntoDictionary(Dictionary<string, PageHelpItemDto> dict, IEnumerable<Setting> settings)
        {
            if (settings == null) return;

            var helpSettings = settings
                .Where(s => s != null && s.SettingKey != null && s.SettingKey.EndsWith(KeySuffix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var setting in helpSettings)
            {
                string key = setting.SettingKey;
                bool hasVal = IsValidContent(setting.SettingValue);

                if (dict.TryGetValue(key, out var existing))
                {
                    if (hasVal)
                    {
                        existing.CurrentContent = setting.SettingValue;
                        existing.IsCustomized = true;
                        existing.UpdatedDate = setting.UpdatedDate;
                        if (!string.IsNullOrWhiteSpace(setting.Name))
                        {
                            existing.PageName = setting.Name;
                        }
                    }
                    else if (!string.Equals(setting.SettingValue, SettingNullValue, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.CurrentContent = string.Empty;
                        existing.IsCustomized = true;
                        existing.UpdatedDate = setting.UpdatedDate;
                        if (!string.IsNullOrWhiteSpace(setting.Name))
                        {
                            existing.PageName = setting.Name;
                        }
                    }
                }
                else if (hasVal)
                {
                    dict[key] = new PageHelpItemDto
                    {
                        Key = key,
                        PageName = !string.IsNullOrWhiteSpace(setting.Name) ? setting.Name : key.Replace(KeySuffix, ""),
                        Category = "Özel",
                        RouteInfo = "",
                        DefaultContent = GetDefaultContent(key),
                        CurrentContent = setting.SettingValue,
                        IsCustomized = true,
                        UpdatedDate = setting.UpdatedDate
                    };
                }
                else if (!string.Equals(setting.SettingValue, SettingNullValue, StringComparison.OrdinalIgnoreCase))
                {
                    dict[key] = new PageHelpItemDto
                    {
                        Key = key,
                        PageName = !string.IsNullOrWhiteSpace(setting.Name) ? setting.Name : key.Replace(KeySuffix, ""),
                        Category = "Özel",
                        RouteInfo = "",
                        DefaultContent = string.Empty,
                        CurrentContent = string.Empty,
                        IsCustomized = true,
                        UpdatedDate = setting.UpdatedDate
                    };
                }
            }
        }

        public static PageHelpItemDto GetHelpItem(string pageKey, ISettingService settingService = null, IEimeceCacheProvider cacheProvider = null)
        {
            if (string.IsNullOrWhiteSpace(pageKey)) return null;

            string resolvedKey = ResolveKey(pageKey);
            var allHelp = GetAllHelpTexts(settingService, cacheProvider);
            PageHelpItemDto item = null;
            if (allHelp != null && allHelp.TryGetValue(resolvedKey, out item))
            {
                if (item != null && item.IsCustomized) return item;
            }

            var service = ResolveSettingService(settingService);
            if (service != null && (item == null || !item.IsCustomized))
            {
                try
                {
                    var dbSetting = service.GetSettingObjectByKeyFromDb(resolvedKey);
                    string directContent = dbSetting?.SettingValue;
                    string directName = dbSetting?.Name;
                    DateTime? directDate = dbSetting?.UpdatedDate;

                    if (dbSetting != null && !string.Equals(directContent, SettingNullValue, StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsValidContent(directContent))
                        {
                            if (item == null)
                            {
                                item = new PageHelpItemDto
                                {
                                    Key = resolvedKey,
                                    PageName = !string.IsNullOrWhiteSpace(directName) ? directName : GetPageTitle(resolvedKey),
                                    Category = "Özel",
                                    RouteInfo = "",
                                    DefaultContent = GetDefaultContent(resolvedKey),
                                    CurrentContent = directContent,
                                    IsCustomized = true,
                                    UpdatedDate = directDate
                                };
                                if (allHelp != null) allHelp[resolvedKey] = item;
                            }
                            else
                            {
                                item.CurrentContent = directContent;
                                item.IsCustomized = true;
                                item.UpdatedDate = directDate;
                                if (!string.IsNullOrWhiteSpace(directName))
                                {
                                    item.PageName = directName;
                                }
                            }
                            return item;
                        }
                        else
                        {
                            if (item == null)
                            {
                                item = new PageHelpItemDto
                                {
                                    Key = resolvedKey,
                                    PageName = !string.IsNullOrWhiteSpace(directName) ? directName : GetPageTitle(resolvedKey),
                                    Category = "Özel",
                                    RouteInfo = "",
                                    DefaultContent = string.Empty,
                                    CurrentContent = string.Empty,
                                    IsCustomized = true,
                                    UpdatedDate = directDate
                                };
                                if (allHelp != null) allHelp[resolvedKey] = item;
                            }
                            else
                            {
                                item.CurrentContent = string.Empty;
                                item.IsCustomized = true;
                                item.UpdatedDate = directDate;
                            }
                            return item;
                        }
                    }
                }
                catch { }
            }

            if (item != null && !item.IsCustomized && !IsValidContent(item.CurrentContent))
            {
                item.CurrentContent = item.DefaultContent;
                item.IsCustomized = false;
            }

            return item;
        }

        public static async Task<PageHelpItemDto> GetHelpItemAsync(string pageKey, ISettingService settingService = null, IEimeceCacheProvider cacheProvider = null)
        {
            if (string.IsNullOrWhiteSpace(pageKey)) return null;

            string resolvedKey = ResolveKey(pageKey);
            var allHelp = await GetAllHelpTextsAsync(settingService, cacheProvider).ConfigureAwait(false);
            PageHelpItemDto item = null;
            if (allHelp != null && allHelp.TryGetValue(resolvedKey, out item))
            {
                if (item != null && item.IsCustomized) return item;
            }

            var service = ResolveSettingService(settingService);
            if (service != null && (item == null || !item.IsCustomized))
            {
                try
                {
                    var dbSetting = await service.GetSettingObjectByKeyFromDbAsync(resolvedKey).ConfigureAwait(false);
                    string directContent = dbSetting?.SettingValue;
                    string directName = dbSetting?.Name;
                    DateTime? directDate = dbSetting?.UpdatedDate;

                    if (dbSetting != null && !string.Equals(directContent, SettingNullValue, StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsValidContent(directContent))
                        {
                            if (item == null)
                            {
                                item = new PageHelpItemDto
                                {
                                    Key = resolvedKey,
                                    PageName = !string.IsNullOrWhiteSpace(directName) ? directName : GetPageTitle(resolvedKey),
                                    Category = "Özel",
                                    RouteInfo = "",
                                    DefaultContent = GetDefaultContent(resolvedKey),
                                    CurrentContent = directContent,
                                    IsCustomized = true,
                                    UpdatedDate = directDate
                                };
                                if (allHelp != null) allHelp[resolvedKey] = item;
                            }
                            else
                            {
                                item.CurrentContent = directContent;
                                item.IsCustomized = true;
                                item.UpdatedDate = directDate;
                                if (!string.IsNullOrWhiteSpace(directName))
                                {
                                    item.PageName = directName;
                                }
                            }
                            return item;
                        }
                        else
                        {
                            if (item == null)
                            {
                                item = new PageHelpItemDto
                                {
                                    Key = resolvedKey,
                                    PageName = !string.IsNullOrWhiteSpace(directName) ? directName : GetPageTitle(resolvedKey),
                                    Category = "Özel",
                                    RouteInfo = "",
                                    DefaultContent = string.Empty,
                                    CurrentContent = string.Empty,
                                    IsCustomized = true,
                                    UpdatedDate = directDate
                                };
                                if (allHelp != null) allHelp[resolvedKey] = item;
                            }
                            else
                            {
                                item.CurrentContent = string.Empty;
                                item.IsCustomized = true;
                                item.UpdatedDate = directDate;
                            }
                            return item;
                        }
                    }
                }
                catch { }
            }

            if (item != null && !item.IsCustomized && !IsValidContent(item.CurrentContent))
            {
                item.CurrentContent = item.DefaultContent;
                item.IsCustomized = false;
            }

            return item;
        }

        public static bool HasHelpContent(string pageKey, ISettingService settingService = null, IEimeceCacheProvider cacheProvider = null)
        {
            var item = GetHelpItem(pageKey, settingService, cacheProvider);
            return item != null && IsValidContent(item.CurrentContent);
        }

        public static async Task<bool> HasHelpContentAsync(string pageKey, ISettingService settingService = null, IEimeceCacheProvider cacheProvider = null)
        {
            var item = await GetHelpItemAsync(pageKey, settingService, cacheProvider).ConfigureAwait(false);
            return item != null && IsValidContent(item.CurrentContent);
        }

        #endregion
    }
}
