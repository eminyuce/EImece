using EImece.Domain.Helpers.ActionResultHelpers;
using EImece.Domain.Models.FrontModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RssController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // GET: Rss
        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult Products(RssParams rssParams)
        {
            var comment = new StringBuilder();
            try
            {
                var items = ProductService.GetProductsRss(rssParams);

                comment.AppendLine("");
                return new FeedResult(items, comment);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return Content(ex.Message);
            }
        }
 
        [OutputCache(CacheProfile = "Cache20Minutes")]
        public ActionResult StoryCategories(RssParams rssParams)
        {
            var comment = new StringBuilder();
            try
            {
                var items = StoryService.GetStoryCategoriesRss(rssParams);
                comment.AppendLine("");
                return new FeedResult(items, comment);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return Content(ex.Message);
            }
        }
    }
}