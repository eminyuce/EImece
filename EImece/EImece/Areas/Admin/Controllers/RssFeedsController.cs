using Microsoft.Extensions.Logging;
using EImece.Web.Areas.Admin.Controllers;
using EImece.Areas.Admin.Models;
using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class RssFeedsController : BaseAdminController
    {
        protected IStoryCategoryService StoryCategoryService { get; }
        protected IProductCategoryService ProductCategoryService { get; }

        public RssFeedsController(ISettingService settingService,
            IStoryCategoryService storyCategoryService,
            IProductCategoryService productCategoryService, ILogger<RssFeedsController> logger)
            : base(settingService, logger) {
            StoryCategoryService = storyCategoryService ?? throw new ArgumentNullException(nameof(storyCategoryService));
            ProductCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var viewModel = new RssFeedsIndexViewModel();

            // Resolve base URL
            string baseUrl = ResolveBaseUrl();
            viewModel.BaseUrl = baseUrl;

            // Populate Available Feeds
            viewModel.Feeds = GetAvailableFeeds(baseUrl);

            // Populate Story Categories for link builder dropdown
            try
            {
                var storyCategories = await StoryCategoryService.GetActiveStoryCategoriesAsync(CurrentLanguage, cancellationToken).ConfigureAwait(false);
                if (storyCategories != null && storyCategories.Any())
                {
                    viewModel.StoryCategories = storyCategories
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = $"{c.Name} (ID: {c.Id})"
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Story categories could not be loaded for RSS link builder.");
            }

            // Populate Product Categories for link builder dropdown
            try
            {
                var productCategories = await ProductCategoryService.GetActiveBaseContentsAsync(true, CurrentLanguage, cancellationToken).ConfigureAwait(false);
                if (productCategories != null && productCategories.Any())
                {
                    viewModel.ProductCategories = productCategories
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = $"{c.Name} (ID: {c.Id})"
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Product categories could not be loaded for RSS link builder.");
            }

            // Populate Languages
            viewModel.Languages = GetAvailableLanguages();

            return View(viewModel);
        }

        private string ResolveBaseUrl()
        {
            if (Request != null && Request.Url != null)
            {
                var builder = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port);
                if (Request.Url.IsDefaultPort)
                {
                    builder.Port = -1;
                }
                return builder.Uri.ToString().TrimEnd('/');
            }

            var domain = AppConfig.Domain;
            var protocol = AppConfig.HttpProtocol ?? "https";
            if (!string.IsNullOrEmpty(domain))
            {
                return $"{protocol}://{domain.TrimEnd('/')}";
            }

            return string.Empty;
        }

        private List<SelectListItem> GetAvailableLanguages()
        {
            var list = new List<SelectListItem>();
            foreach (EImeceLanguage lang in EnumHelper.GetLanguageEnumListFromWebConfig())
            {
                int langId = (int)lang;
                string langName = EnumHelper.GetEnumDescription(lang);
                if (string.IsNullOrEmpty(langName))
                {
                    langName = lang.ToString();
                }

                list.Add(new SelectListItem
                {
                    Value = langId.ToString(),
                    Text = $"{langName} ({langId})",
                    Selected = (langId == CurrentLanguage)
                });
            }
            return list;
        }

        private List<RssFeedInfo> GetAvailableFeeds(string baseUrl)
        {
            var commonUtmParams = new List<RssFeedParameterInfo>
            {
                new RssFeedParameterInfo { Name = "utm_source", Type = "string", IsRequired = false, DefaultValue = "", Description = "Kampanya kaynağı (Google Analytics / izleme için). Tıklanan bağlantıların sonuna eklenir.", Example = "google, newsletter, rss" },
                new RssFeedParameterInfo { Name = "utm_medium", Type = "string", IsRequired = false, DefaultValue = "", Description = "Kampanya aracı / ortamı (cpc, banner, feed, email).", Example = "rss, feed, cpc" },
                new RssFeedParameterInfo { Name = "utm_campaign", Type = "string", IsRequired = false, DefaultValue = "", Description = "Kampanya / promosyon adı.", Example = "spring_sale, weekly_digest" },
                new RssFeedParameterInfo { Name = "utm_term", Type = "string", IsRequired = false, DefaultValue = "", Description = "Arama terimi / hedef anahtar kelime.", Example = "ayakkabi, indirim" },
                new RssFeedParameterInfo { Name = "utm_content", Type = "string", IsRequired = false, DefaultValue = "", Description = "İçerik ayrıştırma / A-B test etiketi.", Example = "rss_feed_v1, top_banner" }
            };

            var productFeed = new RssFeedInfo
            {
                Key = "products",
                Title = "Ürünler RSS Beslemesi",
                Subtitle = "Storefront Products RSS 2.0 Feed",
                CategoryName = "E-Ticaret / Ürünler",
                Description = "Yayınlanan aktif mağaza ürünlerini başlık, açıklama, kategori, marka, indirimli fiyat ve görsel bilgileriyle birlikte RSS 2.0 formatında sunar.",
                RelativePath = "/rss/products",
                HttpMethod = "GET",
                ContentType = "application/rss+xml",
                CacheDuration = "1 Gün / 24 Saat (CustomOutputCache)",
                OutputFormat = "RSS 2.0 (XML) + Enclosure Resim + Marka/Fiyat/Kategori Genişletmeleri",
                RequiresCategoryId = false,
                ControllerAction = "EImece.Controllers.RssController.Products",
                DefaultSampleQuery = "take=20&language=1&description=200&width=300&height=250",
                Parameters = new List<RssFeedParameterInfo>
                {
                    new RssFeedParameterInfo { Name = "Take", Type = "int", IsRequired = false, DefaultValue = "10", Description = "Beslemede listelenecek maksimum ürün sayısı.", Example = "10, 25, 50, 100" },
                    new RssFeedParameterInfo { Name = "Language", Type = "int", IsRequired = false, DefaultValue = "1", Description = "Ürünlerin listeleneceği dil kimliği (1: Türkçe, 2: İngilizce vb.).", Example = "1, 2" },
                    new RssFeedParameterInfo { Name = "Description", Type = "int", IsRequired = false, DefaultValue = "200", Description = "Ürün açıklama metninin kısaltılacağı maksimum karakter uzunluğu.", Example = "150, 200, 500" },
                    new RssFeedParameterInfo { Name = "Width", Type = "int", IsRequired = false, DefaultValue = "0", Description = "Ürün görselinin kırpılacağı genişlik (piksel cinsinden).", Example = "300, 600" },
                    new RssFeedParameterInfo { Name = "Height", Type = "int", IsRequired = false, DefaultValue = "0", Description = "Ürün görselinin kırpılacağı yükseklik (piksel cinsinden).", Example = "250, 400" },
                    new RssFeedParameterInfo { Name = "CategoryId", Type = "int", IsRequired = false, DefaultValue = "0", Description = "İsteğe bağlı kategori filtreleme kimliği.", Example = "5, 12" }
                }
            };
            productFeed.Parameters.AddRange(commonUtmParams);

            var productCategoriesFeed = new RssFeedInfo
            {
                Key = "productcategories",
                Title = "Ürün Kategorisi RSS Beslemesi",
                Subtitle = "Product Category Filtered RSS 2.0 Feed",
                CategoryName = "E-Ticaret / Ürünler",
                Description = "Seçilen ürün kategorisine ait ürünleri başlık, açıklama, kategori, marka, indirimli fiyat ve görsel bilgileriyle birlikte RSS 2.0 formatında sunar.",
                RelativePath = "/rss/productcategories",
                HttpMethod = "GET",
                ContentType = "application/rss+xml",
                CacheDuration = "1 Gün / 24 Saat (CustomOutputCache)",
                OutputFormat = "RSS 2.0 (XML) + Enclosure Görsel + Kategori/Fiyat/Marka Etiketleri",
                RequiresCategoryId = true,
                ControllerAction = "EImece.Controllers.RssController.ProductCategories",
                DefaultSampleQuery = "categoryId=1&take=20&language=1&description=200&width=300&height=250",
                Parameters = new List<RssFeedParameterInfo>
                {
                    new RssFeedParameterInfo { Name = "CategoryId", Type = "int", IsRequired = true, DefaultValue = "-", Description = "Ürünleri listelenecek ürün kategori kimliği (Zorunlu).", Example = "1, 10" },
                    new RssFeedParameterInfo { Name = "Take", Type = "int", IsRequired = false, DefaultValue = "10", Description = "Beslemede listelenecek maksimum ürün sayısı.", Example = "10, 25, 50, 100" },
                    new RssFeedParameterInfo { Name = "Language", Type = "int", IsRequired = false, DefaultValue = "1", Description = "Ürünlerin listeleneceği dil kimliği (1: Türkçe, 2: İngilizce vb.).", Example = "1, 2" },
                    new RssFeedParameterInfo { Name = "Description", Type = "int", IsRequired = false, DefaultValue = "200", Description = "Ürün açıklama metninin kısaltılacağı maksimum karakter uzunluğu.", Example = "150, 200, 500" },
                    new RssFeedParameterInfo { Name = "Width", Type = "int", IsRequired = false, DefaultValue = "0", Description = "Ürün görselinin kırpılacağı genişlik (piksel cinsinden).", Example = "300, 600" },
                    new RssFeedParameterInfo { Name = "Height", Type = "int", IsRequired = false, DefaultValue = "0", Description = "Ürün görselinin kırpılacağı yükseklik (piksel cinsinden).", Example = "250, 400" }
                }
            };
            productCategoriesFeed.Parameters.AddRange(commonUtmParams);

            var storyCategoriesFeed = new RssFeedInfo
            {
                Key = "storycategories",
                Title = "İçerik / Blog Kategorisi RSS Beslemesi (Özet)",
                Subtitle = "Story / Blog Category Summary RSS 2.0 Feed",
                CategoryName = "Blog / Haber / Makale",
                Description = "Seçilen kategoriye ait haber, blog ve makalelerin başlık, özet metin, yayın tarihi ve kapak görseli (enclosure) bilgilerini RSS 2.0 formatında sunar.",
                RelativePath = "/rss/storycategories",
                HttpMethod = "GET",
                ContentType = "application/rss+xml",
                CacheDuration = "1 Gün / 24 Saat (CustomOutputCache)",
                OutputFormat = "RSS 2.0 (XML) + Enclosure Görsel + Kategori Etiketi",
                RequiresCategoryId = true,
                ControllerAction = "EImece.Controllers.RssController.StoryCategories",
                DefaultSampleQuery = "categoryId=1&take=10&language=1&description=250",
                Parameters = new List<RssFeedParameterInfo>
                {
                    new RssFeedParameterInfo { Name = "CategoryId", Type = "int", IsRequired = true, DefaultValue = "-", Description = "İçerikleri getirilecek hikaye / blog kategori kimliği (Zorunlu).", Example = "1, 53" },
                    new RssFeedParameterInfo { Name = "Take", Type = "int", IsRequired = false, DefaultValue = "10", Description = "Beslemede listelenecek maksimum içerik sayısı.", Example = "10, 20, 50" },
                    new RssFeedParameterInfo { Name = "Language", Type = "int", IsRequired = false, DefaultValue = "1", Description = "İçeriklerin dili (1: Türkçe, 2: İngilizce vb.).", Example = "1, 2" },
                    new RssFeedParameterInfo { Name = "Description", Type = "int", IsRequired = false, DefaultValue = "200", Description = "Özet açıklama metninin maksimum karakter uzunluğu.", Example = "200, 300" },
                    new RssFeedParameterInfo { Name = "Width", Type = "int", IsRequired = false, DefaultValue = "0", Description = "Kapak görseli genişliği (piksel).", Example = "400, 800" },
                    new RssFeedParameterInfo { Name = "Height", Type = "int", IsRequired = false, DefaultValue = "0", Description = "Kapak görseli yüksekliği (piksel).", Example = "250, 450" }
                }
            };
            storyCategoriesFeed.Parameters.AddRange(commonUtmParams);

            var storyCategoriesFullFeed = new RssFeedInfo
            {
                Key = "storycategoriesfull",
                Title = "İçerik / Blog Kategorisi RSS Beslemesi (Tam Metin & CDATA)",
                Subtitle = "Story / Blog Category Full HTML Content RSS 2.0 Feed",
                CategoryName = "Blog / Haber / Makale",
                Description = "Seçilen kategoriye ait haber ve makalelerin tam HTML gövdesini (CDATA blokları, yerleşik görseller ve zengin metin dahil) RSS 2.0 formatında sunar. Feed okuyucular, bülten entegrasyonları ve tam içerik dağıtımı için uygundur.",
                RelativePath = "/rss/storycategoriesfull",
                HttpMethod = "GET",
                ContentType = "application/rss+xml",
                CacheDuration = "1 Gün / 24 Saat (CustomOutputCache)",
                OutputFormat = "RSS 2.0 (XML) + CDATA HTML Tam İçerik + Görsel Tag",
                RequiresCategoryId = true,
                ControllerAction = "EImece.Controllers.RssController.StoryCategoriesFull",
                DefaultSampleQuery = "categoryId=1&take=10&language=1&description=250",
                Parameters = new List<RssFeedParameterInfo>
                {
                    new RssFeedParameterInfo { Name = "CategoryId", Type = "int", IsRequired = true, DefaultValue = "-", Description = "İçerikleri getirilecek hikaye / blog kategori kimliği (Zorunlu).", Example = "1, 53" },
                    new RssFeedParameterInfo { Name = "Take", Type = "int", IsRequired = false, DefaultValue = "10", Description = "Beslemede listelenecek maksimum içerik sayısı.", Example = "10, 20, 50" },
                    new RssFeedParameterInfo { Name = "Language", Type = "int", IsRequired = false, DefaultValue = "1", Description = "İçeriklerin dili (1: Türkçe, 2: İngilizce vb.).", Example = "1, 2" },
                    new RssFeedParameterInfo { Name = "Description", Type = "int", IsRequired = false, DefaultValue = "200", Description = "Başlık altı özet metin karakter sınırı.", Example = "200, 300" },
                    new RssFeedParameterInfo { Name = "Width", Type = "int", IsRequired = false, DefaultValue = "0", Description = "Görsel genişliği (piksel).", Example = "600, 800" },
                    new RssFeedParameterInfo { Name = "Height", Type = "int", IsRequired = false, DefaultValue = "0", Description = "Görsel yüksekliği (piksel).", Example = "400, 600" }
                }
            };
            storyCategoriesFullFeed.Parameters.AddRange(commonUtmParams);

            return new List<RssFeedInfo> { productFeed, productCategoriesFeed, storyCategoriesFeed, storyCategoriesFullFeed };
        }
    }
}
