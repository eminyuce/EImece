using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers;

namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {
        [OutputCache(CacheProfile = "Cache1Hour")]
        public ActionResult Index()
        {

            var sitemapItems = new List<SitemapItem>();

            var menus = MenuService.GetActiveBaseEntitiesFromCache(true,null);

            foreach (var c in menus)
            {
                var parts = c.MenuLink.Split("-".ToCharArray());
                var action = parts[1];
                var controller = parts[0];
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


            var productCategories = ProductCategoryService.GetActiveBaseEntitiesFromCache(true, null);
            foreach (var productCategory in productCategories)
            {

                DateTime? lastModified = productCategory.UpdatedDate;
                SitemapItem sm = new SitemapItem(productCategory.GetDetailPageUrl("Category", "ProductCategories","",
                         Settings.HttpProtocol),
                               lastModified,
                               SitemapChangeFrequency.Daily,
                               priority: 1.0);

                sitemapItems.Add(sm);

            }
            var products = ProductService.GetActiveBaseEntitiesFromCache(true, null);
            foreach (var product in products)
            {
                var productCategory = productCategories.FirstOrDefault(r => r.Id == product.ProductCategoryId);
                string productCategoryName = productCategory != null ? GeneralHelper.GetUrlSeoString(productCategory.Name) : "";
              
                DateTime? lastModified = product.UpdatedDate;
                SitemapItem sm = new SitemapItem(product.GetDetailPageUrl("Detail", "Products", productCategoryName,
                         Settings.HttpProtocol),
                               lastModified,
                               SitemapChangeFrequency.Daily,
                               priority: 1.0);

                sitemapItems.Add(sm);

            }


            var storyCategories = StoryCategoryService.GetActiveBaseEntitiesFromCache(true, null);
            foreach (var storyCategory in storyCategories)
            {

                DateTime? lastModified = storyCategory.UpdatedDate;
                SitemapItem sm = new SitemapItem(storyCategory.GetDetailPageUrl("Category", "Stories", "",
                         Settings.HttpProtocol),
                               lastModified,
                               SitemapChangeFrequency.Daily,
                               priority: 1.0);

                sitemapItems.Add(sm);

            }
            var stories = StoryService.GetActiveBaseEntitiesFromCache(true, null);
            foreach (var story in stories)
            {
                var storyCategory = storyCategories.FirstOrDefault(r => r.Id == story.StoryCategoryId);
                string storyCategoryName = storyCategory!=null ? GeneralHelper.GetUrlSeoString(storyCategory.Name) : "";

                DateTime? lastModified = story.UpdatedDate;
                SitemapItem sm = new SitemapItem(story.GetDetailPageUrl("Detail", "Stories", storyCategoryName,
                         Settings.HttpProtocol),
                               lastModified,
                               SitemapChangeFrequency.Daily,
                               priority: 1.0);

                sitemapItems.Add(sm);

            }


            return new SitemapResult(sitemapItems);

        }
        
    }
}