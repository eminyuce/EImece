using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace EImece.Domain.Services
{
    public class SiteMapService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IMainPageImageService MainPageImageService { get; set; }

        [Inject]
        public ISettingService SettingService { get; set; }

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        [Inject]
        public IMenuService MenuService { get; set; }

        [Inject]
        public IStoryService StoryService { get; set; }

        [Inject]
        public IStoryCategoryService StoryCategoryService { get; set; }

        [Inject]
        public ITagService TagService { get; set; }

        [Inject]
        public ITagCategoryService TagCategoryService { get; set; }

        [Inject]
        public ISubscriberService SubsciberService { get; set; }

        [Inject]
        public IFileStorageService FileStorageService { get; set; }

        [Inject]
        public IImageDownloadService ImageDownloadService { get; set; }

        [Inject]
        public ITemplateService TemplateService { get; set; }

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        public List<SitemapItem> GenerateSiteMap()
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

        private void GenerateTagSiteMap(List<SitemapItem> sitemapItems, int language)
        {
            try
            {
                var tags = TagService.GetProductTags(language);

                foreach (var item in tags)
                {
                    DateTime? lastModified = item.UpdatedDate;
                    SitemapItem sm = new SitemapItem(item.GetDetailPageUrl("Tag", "Stories", null,
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
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
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
                    SitemapItem sm = new SitemapItem(story.GetDetailPageUrl("Detail", "Stories", storyCategoryName,
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
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
                    SitemapItem sm = new SitemapItem(storyCategory.GetDetailPageUrl("Categories", "Stories", "",
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }

            return storyCategories;
        }

        private void GenerateProductSiteMap(List<SitemapItem> sitemapItems, int language, List<ProductCategory> productCategories)
        {
            try
            {
                var products = ProductService.GetActiveBaseEntitiesFromCache(true, language);
                foreach (var product in products)
                {
                    var productCategory = productCategories.FirstOrDefault(r => r.Id == product.ProductCategoryId);
                    if (productCategory == null || !productCategory.IsActive)
                    {
                        continue;
                    }
                    string productCategoryName = productCategory.Name;

                    DateTime? lastModified = product.UpdatedDate;
                    SitemapItem sm = new SitemapItem(product.GetDetailPageUrl("Detail", "Products", productCategoryName,
                             AppConfig.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
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
                Logger.Error(ex, ex.Message);
            }

            return productCategories;
        }

        private void GenerateMenuSiteMap(List<SitemapItem> sitemapItems, int language)
        {
            try
            {
                if (HttpContext.Current == null)
                    return;
                var requestContext = HttpContext.Current.Request.RequestContext;

                var menus = MenuService.GetActiveBaseEntitiesFromCache(true, language);

                foreach (var c in menus)
                {
                    try
                    {
                        // Prefer absolute/external links; otherwise parse controller-action MenuLink.
                        string url;
                        if (c.LinkIsActive && !string.IsNullOrEmpty(c.Link))
                        {
                            url = c.Link;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(c.MenuLink))
                            {
                                continue;
                            }

                            var p = c.MenuLink.Split('_');
                            var parts = p[0].Split('-');
                            if (parts.Length < 2)
                            {
                                Logger.Warn("Skipping sitemap menu Id={0} with invalid MenuLink '{1}'", c.Id, c.MenuLink);
                                continue;
                            }

                            var action = parts[1];
                            var controller = parts[0];
                            var mid = p[p.Length - 1];
                            var urlHelper = new UrlHelper(requestContext);

                            if (controller.Equals("pages", StringComparison.InvariantCultureIgnoreCase))
                            {
                                url = urlHelper.Action("detail", controller, new { id = c.GetSeoUrl() }, AppConfig.HttpProtocol);
                            }
                            else if (controller.Equals("stories", StringComparison.InvariantCultureIgnoreCase)
                                     && action.Equals("categories", StringComparison.InvariantCultureIgnoreCase))
                            {
                                url = urlHelper.Action(action, controller, new { id = mid }, AppConfig.HttpProtocol);
                            }
                            else
                            {
                                url = urlHelper.Action(action, controller, null, AppConfig.HttpProtocol);
                            }
                        }

                        if (string.IsNullOrWhiteSpace(url) || url == "#")
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
                        Logger.Error(ex, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
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

                Logger.Info("ReadSiteMapXmlAndRequestAsync warmed {0} url(s) in {1} ms",
                    urlSet.Url.Count, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ReadSiteMapXmlAndRequestAsync failed");
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
                Logger.Warn(ex, "Sitemap warm-up request failed for {0}", loc);
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