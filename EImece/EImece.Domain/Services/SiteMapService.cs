using Microsoft.Extensions.Logging;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Models.Enums;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EImece.Domain.Services
{
    public class SiteMapService
    {
        private readonly ILogger<SiteMapService> _logger;

        private readonly IMainPageImageService MainPageImageService;
        private readonly ISettingService SettingService;
        private readonly IProductService ProductService;
        private readonly IProductCategoryService ProductCategoryService;
        private readonly IMenuService MenuService;
        private readonly IStoryService StoryService;
        private readonly IStoryCategoryService StoryCategoryService;
        private readonly ITagService TagService;
        private readonly ITagCategoryService TagCategoryService;
        private readonly ISubscriberService SubsciberService;
        private readonly IFileStorageService FileStorageService;
        private readonly IImageDownloadService ImageDownloadService;
        private readonly ITemplateService TemplateService;
        private readonly IMailTemplateService MailTemplateService;

        public SiteMapService(IMainPageImageService mainPageImageService,
            ISettingService settingService,
            IProductService productService,
            IProductCategoryService productCategoryService,
            IMenuService menuService,
            IStoryService storyService,
            IStoryCategoryService storyCategoryService,
            ITagService tagService,
            ITagCategoryService tagCategoryService,
            ISubscriberService subsciberService,
            IFileStorageService fileStorageService,
            IImageDownloadService imageDownloadService,
            ITemplateService templateService,
            IMailTemplateService mailTemplateService, ILogger<SiteMapService> logger)
         {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            MainPageImageService = mainPageImageService ?? throw new ArgumentNullException(nameof(mainPageImageService));
            SettingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
            ProductCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
            MenuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            StoryService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            StoryCategoryService = storyCategoryService ?? throw new ArgumentNullException(nameof(storyCategoryService));
            TagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            TagCategoryService = tagCategoryService ?? throw new ArgumentNullException(nameof(tagCategoryService));
            SubsciberService = subsciberService ?? throw new ArgumentNullException(nameof(subsciberService));
            FileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            ImageDownloadService = imageDownloadService ?? throw new ArgumentNullException(nameof(imageDownloadService));
            TemplateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            MailTemplateService = mailTemplateService ?? throw new ArgumentNullException(nameof(mailTemplateService));
        }

        [Timed("service.sitemap.generate_sync")]
        public virtual List<SitemapItem> GenerateSiteMap()
        {
            List<EImeceLanguage> eImeceLanguages = EnumHelper.GetLanguageEnumListFromWebConfig();

            var sitemapItems = new List<SitemapItem>();
            int language = 0;
            foreach (var item in eImeceLanguages)
            {
                language = (int)item;
                GenerateMenuSiteMap(sitemapItems, language);
                List<ProductCategory> productCategories = GenerateProductCategorySiteMap(sitemapItems, language);
                GenerateProductSiteMap(sitemapItems, language, productCategories);
                List<StoryCategory> storyCategories = new List<StoryCategory>();
                storyCategories = GenerateStoryCategorySiteMap(sitemapItems, language, storyCategories);
                GenerateStorySiteMap(sitemapItems, language, storyCategories);
                GenerateTagSiteMap(sitemapItems, language);
            }

            return sitemapItems;
        }

        [Timed("service.sitemap.generate")]
        public virtual async Task<List<SitemapItem>> GenerateSiteMapAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            List<EImeceLanguage> eImeceLanguages = EnumHelper.GetLanguageEnumListFromWebConfig();

            var sitemapItems = new List<SitemapItem>();
            foreach (var item in eImeceLanguages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int language = (int)item;
                await GenerateMenuSiteMapAsync(sitemapItems, language, cancellationToken).ConfigureAwait(false);
                List<ProductCategory> productCategories = await GenerateProductCategorySiteMapAsync(sitemapItems, language, cancellationToken).ConfigureAwait(false);
                await GenerateProductSiteMapAsync(sitemapItems, language, productCategories, cancellationToken).ConfigureAwait(false);
                List<StoryCategory> storyCategories = await GenerateStoryCategorySiteMapAsync(sitemapItems, language, cancellationToken).ConfigureAwait(false);
                await GenerateStorySiteMapAsync(sitemapItems, language, storyCategories, cancellationToken).ConfigureAwait(false);
                await GenerateTagSiteMapAsync(sitemapItems, language, cancellationToken).ConfigureAwait(false);
            }

            return sitemapItems;
        }

        private async Task GenerateMenuSiteMapAsync(List<SitemapItem> sitemapItems, int language, CancellationToken cancellationToken)
        {
            try
            {
                var menus = await MenuService.GetActiveBaseEntitiesFromCacheAsync(true, language).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                AddMenuSitemapItems(sitemapItems, menus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task<List<ProductCategory>> GenerateProductCategorySiteMapAsync(List<SitemapItem> sitemapItems, int language, CancellationToken cancellationToken)
        {
            var productCategories = new List<ProductCategory>();
            try
            {
                productCategories = await ProductCategoryService.GetActiveBaseEntitiesFromCacheAsync(true, language).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var productCategory in productCategories)
                {
                    DateTime? lastModified = productCategory.UpdatedDate;
                    SitemapItem sm = new SitemapItem(productCategory.GetDetailPageUrl("Category", "ProductCategories", "",
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return productCategories;
        }

        private async Task GenerateProductSiteMapAsync(List<SitemapItem> sitemapItems, int language, List<ProductCategory> productCategories, CancellationToken cancellationToken)
        {
            try
            {
                var products = await ProductService.GetStorefrontActiveProductsAsync(language, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var product in products)
                {
                    string productCategoryName = !string.IsNullOrEmpty(product.ProductCategoryName) ? product.ProductCategoryName : "no_category";
                    DateTime? lastModified = product.UpdatedDate;
                    var dummy = new Product { Id = product.Id, Name = product.Name, NameLong = product.NameLong };
                    SitemapItem sm = new SitemapItem(dummy.GetDetailPageUrl(Constants.DetailAction, "Products", productCategoryName,
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task<List<StoryCategory>> GenerateStoryCategorySiteMapAsync(List<SitemapItem> sitemapItems, int language, CancellationToken cancellationToken)
        {
            var storyCategories = new List<StoryCategory>();
            try
            {
                storyCategories = await StoryCategoryService.GetActiveBaseEntitiesFromCacheAsync(true, language).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var storyCategory in storyCategories)
                {
                    DateTime? lastModified = storyCategory.UpdatedDate;
                    SitemapItem sm = new SitemapItem(storyCategory.GetDetailPageUrl("Categories", Constants.StoriesAction, "",
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return storyCategories;
        }

        private async Task GenerateStorySiteMapAsync(List<SitemapItem> sitemapItems, int language, List<StoryCategory> storyCategories, CancellationToken cancellationToken)
        {
            try
            {
                var stories = await StoryService.GetActiveBaseEntitiesFromCacheAsync(true, language).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var story in stories)
                {
                    var storyCategory = storyCategories.FirstOrDefault(r => r.Id == story.StoryCategoryId);
                    if (storyCategory == null || !storyCategory.IsActive)
                    {
                        continue;
                    }
                    string storyCategoryName = storyCategory.Name;

                    DateTime? lastModified = story.UpdatedDate;
                    SitemapItem sm = new SitemapItem(story.GetDetailPageUrl(Constants.DetailAction, Constants.StoriesAction, storyCategoryName,
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private void GenerateTagSiteMap(List<SitemapItem> sitemapItems, int language)
        {
            try
            {
                var productTags = TagService.GetProductTags(language);
                if (productTags != null)
                {
                    foreach (var item in productTags)
                    {
                        DateTime? lastModified = item.UpdatedDate;
                        SitemapItem sm = new SitemapItem(item.GetDetailPageUrl("Tag", "Products", null,
                          AppConfig.HttpProtocol),
                                lastModified,
                                SitemapChangeFrequency.Daily,
                                priority: 1.0);

                        sitemapItems.Add(sm);
                    }
                }

                var storyTags = TagService.GetStorefrontTagsWithStoryCounts(language, 1);
                if (storyTags != null)
                {
                    foreach (var item in storyTags)
                    {
                        var dummy = new Tag { Id = item.Id, Name = item.Name };
                        SitemapItem sm = new SitemapItem(dummy.GetDetailPageUrl("Tag", Constants.StoriesAction, null,
                          AppConfig.HttpProtocol),
                                null,
                                SitemapChangeFrequency.Daily,
                                priority: 1.0);

                        sitemapItems.Add(sm);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task GenerateTagSiteMapAsync(List<SitemapItem> sitemapItems, int language, CancellationToken cancellationToken)
        {
            try
            {
                var productTags = await TagService.GetProductTagsAsync(language, cancellationToken).ConfigureAwait(false);
                if (productTags != null)
                {
                    foreach (var item in productTags)
                    {
                        DateTime? lastModified = item.UpdatedDate;
                        SitemapItem sm = new SitemapItem(item.GetDetailPageUrl("Tag", "Products", null,
                          AppConfig.HttpProtocol),
                                lastModified,
                                SitemapChangeFrequency.Daily,
                                priority: 1.0);

                        sitemapItems.Add(sm);
                    }
                }

                var storyTags = await TagService.GetStorefrontTagsWithStoryCountsAsync(language, 1, cancellationToken).ConfigureAwait(false);
                if (storyTags != null)
                {
                    foreach (var item in storyTags)
                    {
                        var dummy = new Tag { Id = item.Id, Name = item.Name };
                        SitemapItem sm = new SitemapItem(dummy.GetDetailPageUrl("Tag", Constants.StoriesAction, null,
                          AppConfig.HttpProtocol),
                                null,
                                SitemapChangeFrequency.Daily,
                                priority: 1.0);

                        sitemapItems.Add(sm);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private static void AddTagSitemapItems(List<SitemapItem> sitemapItems, List<Tag> tags)
        {
            foreach (var item in tags)
            {
                DateTime? lastModified = item.UpdatedDate;
                SitemapItem sm = new SitemapItem(item.GetDetailPageUrl("Tag", Constants.StoriesAction, null,
                         AppConfig.HttpProtocol),
                               lastModified,
                               SitemapChangeFrequency.Daily,
                               priority: 1.0);

                sitemapItems.Add(sm);

                sm = new SitemapItem(item.GetDetailPageUrl("Tag", "Products", null,
                  AppConfig.HttpProtocol),
                        lastModified,
                        SitemapChangeFrequency.Daily,
                        priority: 1.0);

                sitemapItems.Add(sm);
            }
        }

        private void GenerateStorySiteMap(List<SitemapItem> sitemapItems, int language, List<StoryCategory> storyCategories)
        {
            try
            {
                var stories = StoryService.GetActiveBaseEntitiesFromCache(true, language);
                foreach (var story in stories)
                {
                    var storyCategory = storyCategories.FirstOrDefault(r => r.Id == story.StoryCategoryId);
                    if (storyCategory == null || !storyCategory.IsActive)
                    {
                        continue;
                    }
                    string storyCategoryName = storyCategory.Name;

                    DateTime? lastModified = story.UpdatedDate;
                    SitemapItem sm = new SitemapItem(story.GetDetailPageUrl(Constants.DetailAction, Constants.StoriesAction, storyCategoryName,
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private List<StoryCategory> GenerateStoryCategorySiteMap(List<SitemapItem> sitemapItems, int language, List<StoryCategory> storyCategories)
        {
            try
            {
                storyCategories = StoryCategoryService.GetActiveBaseEntitiesFromCache(true, language);
                foreach (var storyCategory in storyCategories)
                {
                    DateTime? lastModified = storyCategory.UpdatedDate;
                    SitemapItem sm = new SitemapItem(storyCategory.GetDetailPageUrl("Categories", Constants.StoriesAction, "",
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return storyCategories;
        }

        private void GenerateProductSiteMap(List<SitemapItem> sitemapItems, int language, List<ProductCategory> productCategories)
        {
            try
            {
                var products = ProductService.GetStorefrontActiveProducts(language);
                foreach (var product in products)
                {
                    string productCategoryName = !string.IsNullOrEmpty(product.ProductCategoryName) ? product.ProductCategoryName : "no_category";
                    DateTime? lastModified = product.UpdatedDate;
                    var dummy = new Product { Id = product.Id, Name = product.Name, NameLong = product.NameLong };
                    SitemapItem sm = new SitemapItem(dummy.GetDetailPageUrl(Constants.DetailAction, "Products", productCategoryName,
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private List<ProductCategory> GenerateProductCategorySiteMap(List<SitemapItem> sitemapItems, int language)
        {
            var productCategories = new List<ProductCategory>();
            try
            {
                productCategories = ProductCategoryService.GetActiveBaseEntitiesFromCache(true, language);
                foreach (var productCategory in productCategories)
                {
                    DateTime? lastModified = productCategory.UpdatedDate;
                    SitemapItem sm = new SitemapItem(productCategory.GetDetailPageUrl("Category", "ProductCategories", "",
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return productCategories;
        }

        private void GenerateMenuSiteMap(List<SitemapItem> sitemapItems, int language)
        {
            try
            {
                var menus = MenuService.GetActiveBaseEntitiesFromCache(true, language);
                AddMenuSitemapItems(sitemapItems, menus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private void AddMenuSitemapItems(List<SitemapItem> sitemapItems, IEnumerable<Menu> menus)
        {
            foreach (var c in menus)
            {
                try
                {
                    string url;
                    if (!TryGetMenuSitemapUrl(c, out url))
                    {
                        continue;
                    }

                    sitemapItems.Add(new SitemapItem(
                        url,
                        c.UpdatedDate,
                        changeFrequency: SitemapChangeFrequency.Daily,
                        priority: 1.0));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        private bool TryGetMenuSitemapUrl(Menu c, out string url)
        {
            url = null;
            if (c.LinkIsActive && !string.IsNullOrEmpty(c.Link))
            {
                url = c.Link;
            }
            else
            {
                url = BuildMenuLinkUrl(c);
                if (url == null)
                {
                    return false;
                }
            }

            return !string.IsNullOrWhiteSpace(url) && url != "#";
        }

        private string BuildMenuLinkUrl(Menu c)
        {
            if (string.IsNullOrWhiteSpace(c.MenuLink))
            {
                return null;
            }

            var p = c.MenuLink.Split('_');
            var parts = p[0].Split('-');
            if (parts.Length < 2)
            {
                _logger.LogWarning("Skipping sitemap menu Id={0} with invalid MenuLink '{1}'", c.Id, c.MenuLink);
                return null;
            }

            var action = parts[1];
            var controller = parts[0];
            var mid = p.Length > 1 ? p[p.Length - 1] : null;
            var baseUrl = EntityExtension.GetAbsoluteApplicationBaseUrl(AppConfig.HttpProtocol);
            var relativePath = EntityExtension.BuildMenuLinkRelativePath(controller, action, mid, c);
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            return baseUrl.TrimEnd('/') + relativePath;
        }

        /// <summary>
        /// Number of sitemap URLs warmed concurrently. The previous implementation awaited each
        /// URL one-at-a-time, which was the dominant cost of the admin "Clear Cache" action; a
        /// bounded fan-out keeps the server responsive while cutting wall-clock time dramatically.
        /// </summary>
        private const int SitemapWarmUpConcurrency = 8;

        /// <summary>
        /// Warms the output cache by requesting every URL in the given sitemap XML. Fully async and
        /// now fanned out with a bounded degree of parallelism (<see cref="SitemapWarmUpConcurrency"/>):
        /// requests never block a worker thread, each fetch uses the resilient client via
        /// <see cref="IImageDownloadService"/>, and a single failed URL no longer aborts the rest.
        /// </summary>
        public async Task ReadSiteMapXmlAndRequestAsync(string xml, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrEmpty(xml))
            {
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                Urlset urlSet;
                XmlSerializer serializer = new XmlSerializer(typeof(Urlset));
                using (StringReader reader = new StringReader(xml))
                {
                    urlSet = (Urlset)serializer.Deserialize(reader);
                }

                if (urlSet?.Url == null || urlSet.Url.Count == 0)
                {
                    return;
                }

                using (var throttler = new SemaphoreSlim(SitemapWarmUpConcurrency))
                {
                    var tasks = new List<Task>(urlSet.Url.Count);
                    foreach (var tUrl in urlSet.Url)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);
                        tasks.Add(WarmUpUrlAsync(tUrl.Loc, throttler, cancellationToken));
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }

                _logger.LogInformation("ReadSiteMapXmlAndRequestAsync warmed {0} url(s) in {1} ms",
                    urlSet.Url.Count, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReadSiteMapXmlAndRequestAsync failed");
            }
        }

        private async Task WarmUpUrlAsync(string loc, SemaphoreSlim throttler, CancellationToken cancellationToken)
        {
            try
            {
                await ImageDownloadService.GetImageAsync(loc, null, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // One bad URL must not fail the whole warm-up.
                _logger.LogWarning(ex, "Sitemap warm-up request failed for {0}", loc);
            }
            finally
            {
                throttler.Release();
            }
        }
    }

    [XmlRoot(ElementName = "url")]
    public class Url
    {
        [XmlElement(ElementName = "loc")]
        public string Loc { get; set; }

        [XmlElement(ElementName = "lastmod")]
        public DateTime Lastmod { get; set; }

        [XmlElement(ElementName = "changefreq")]
        public string Changefreq { get; set; }

        [XmlElement(ElementName = "priority")]
        public double Priority { get; set; }
    }

    [XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public class Urlset
    {
        [XmlElement(ElementName = "url")]
        public List<Url> Url { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

        [XmlAttribute(AttributeName = "xsi")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "schemaLocation")]
        public string SchemaLocation { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}