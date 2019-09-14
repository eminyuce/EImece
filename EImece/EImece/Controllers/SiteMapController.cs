using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers;
using NLog;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using System.Text.RegularExpressions;

namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [CustomOutputCache(CacheProfile = "Cache1Hour")]
        public ActionResult Index()
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

            return new SitemapResult(sitemapItems);

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
                             ApplicationConfigs.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);

                    sm = new SitemapItem(item.GetDetailPageUrl("Tag", "Products", null,
                      ApplicationConfigs.HttpProtocol),
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
                             ApplicationConfigs.HttpProtocol),
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
                             ApplicationConfigs.HttpProtocol),
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
                             ApplicationConfigs.HttpProtocol),
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
            List<ProductCategory> productCategories = new List<ProductCategory>();
            try
            {
                productCategories = ProductCategoryService.GetActiveBaseEntitiesFromCache(true, language);
                foreach (var productCategory in productCategories)
                {

                    DateTime? lastModified = productCategory.UpdatedDate;
                    SitemapItem sm = new SitemapItem(productCategory.GetDetailPageUrl("Category", "ProductCategories", "",
                             ApplicationConfigs.HttpProtocol),
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
                            var siteMap = new SitemapItem(
                                Url.Action("detail",
                                controller,
                                new { id = c.GetSeoUrl() },
                                ApplicationConfigs.HttpProtocol),
                                lastModified,
                                changeFrequency: SitemapChangeFrequency.Daily,
                                priority: 1.0);


                            sitemapItems.Add(siteMap);
                        }
                        else if (controller.Equals("stories", StringComparison.InvariantCultureIgnoreCase)
                                                    && action.Equals("categories", StringComparison.InvariantCultureIgnoreCase))
                        {


                            var siteMap = new SitemapItem(
                                 Url.Action(action,
                                 controller,
                                 new { id = mid },
                                 ApplicationConfigs.HttpProtocol),
                                 lastModified,
                                 changeFrequency: SitemapChangeFrequency.Daily,
                                 priority: 1.0);


                            sitemapItems.Add(siteMap);

                        }
                        else
                        {
                            var siteMap = new SitemapItem(
                                Url.Action(action,
                                controller,
                                null,
                                ApplicationConfigs.HttpProtocol),
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

    }
}