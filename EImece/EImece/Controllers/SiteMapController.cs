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

namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [OutputCache(CacheProfile = "Cache1Hour")]
        public ActionResult Index()
        {


            var sitemapItems = new List<SitemapItem>();

            try
            {


                var menus = MenuService.GetActiveBaseEntitiesFromCache(true, null);

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
                                Settings.HttpProtocol),
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
                                 Settings.HttpProtocol),
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
                                Settings.HttpProtocol),
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
            List<ProductCategory> productCategories = new List<ProductCategory>();
            try
            {
                productCategories = ProductCategoryService.GetActiveBaseEntitiesFromCache(true, null);
                foreach (var productCategory in productCategories)
                {

                    DateTime? lastModified = productCategory.UpdatedDate;
                    SitemapItem sm = new SitemapItem(productCategory.GetDetailPageUrl("Category", "ProductCategories", "",
                             Settings.HttpProtocol),
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
            try
            {
                var products = ProductService.GetActiveBaseEntitiesFromCache(true, null);
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
                             Settings.HttpProtocol),
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
            List<StoryCategory> storyCategories = new List<StoryCategory>();
            try
            {
                storyCategories = StoryCategoryService.GetActiveBaseEntitiesFromCache(true, null);
                foreach (var storyCategory in storyCategories)
                {

                    DateTime? lastModified = storyCategory.UpdatedDate;
                    SitemapItem sm = new SitemapItem(storyCategory.GetDetailPageUrl("Categories", "Stories", "",
                             Settings.HttpProtocol),
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
            try
            {
                var stories = StoryService.GetActiveBaseEntitiesFromCache(true, null);
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
                             Settings.HttpProtocol),
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
            try
            {
                var tags = TagService.GetActiveBaseEntitiesFromCache(true, null);

                foreach (var item in tags)
                {
                    DateTime? lastModified = item.UpdatedDate;
                    SitemapItem sm = new SitemapItem(item.GetDetailPageUrl("Tag", "Stories", null,
                             Settings.HttpProtocol),
                                   lastModified,
                                   SitemapChangeFrequency.Daily,
                                   priority: 1.0);

                    sitemapItems.Add(sm);

                    sm = new SitemapItem(item.GetDetailPageUrl("Tag", "Products", null,
                      Settings.HttpProtocol),
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
            return new SitemapResult(sitemapItems);

        }

    }
}