using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                var tags = TagService.GetActiveBaseEntitiesFromCache(true, language);

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
                var requestContext = HttpContext.Current.Request.RequestContext;

                var menus = MenuService.GetActiveBaseEntitiesFromCache(true, language);

                foreach (var c in menus)
                {
                    try
                    {
                        var p = c.MenuLink.Split("_".ToCharArray());
                        var parts = p.First().Split("-".ToCharArray());
                        var action = parts[1];
                        var controller = parts[0];
                        String mid = "";
                        mid = p.Last();

                        DateTime? lastModified = c.UpdatedDate;

                        if (controller.Equals("pages", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string url = new UrlHelper(requestContext).Action("detail",
                                                            controller,
                                                            new { id = c.GetSeoUrl() },
                                                            AppConfig.HttpProtocol);
                            var siteMap = new SitemapItem(
                                url,
                                lastModified,
                                changeFrequency: SitemapChangeFrequency.Daily,
                                priority: 1.0);

                            sitemapItems.Add(siteMap);
                        }
                        else if (controller.Equals("stories", StringComparison.InvariantCultureIgnoreCase)
                                                    && action.Equals("categories", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var siteMap = new SitemapItem(
                                 new UrlHelper(requestContext).Action(action,
                                 controller,
                                 new { id = mid },
                                 AppConfig.HttpProtocol),
                                 lastModified,
                                 changeFrequency: SitemapChangeFrequency.Daily,
                                 priority: 1.0);

                            sitemapItems.Add(siteMap);
                        }
                        else
                        {
                            var siteMap = new SitemapItem(
                                new UrlHelper(requestContext).Action(action,
                                controller,
                                null,
                                AppConfig.HttpProtocol),
                                lastModified,
                                changeFrequency: SitemapChangeFrequency.Daily,
                                priority: 1.0);

                            sitemapItems.Add(siteMap);
                        }
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


        public void ReadSiteMapXmlAndRequest(String xml)
        {
           XmlSerializer serializer = new XmlSerializer(typeof(Urlset));
            using (StringReader reader = new StringReader(xml))
            {
               var test = (Urlset)serializer.Deserialize(reader);
                
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

    [XmlRoot(ElementName = "urlset")]
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