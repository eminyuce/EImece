using EImece.Domain.Helpers.ActionResultHelpers;
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
        public ActionResult Products(int take = 10, int language = 1, int description = 1000, int width = 200, int height = 200)
        {
            var comment = new StringBuilder();
            try
            {
                var items = ProductService.GetProductsRss(take, language, description, width, height);

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
        public ActionResult StoryCategories(int categoryId = 0, int take = 10, int language = 1, int description = 1000, int width = 200, int height = 200)
        {
            var comment = new StringBuilder();
            try
            {
                var items = StoryService.GetStoryCategoriesRss(categoryId, take, language, description, width, height);
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