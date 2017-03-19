using EImece.Domain.Helpers.ActionResultHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RssController :BaseController
    {
        // GET: Rss
        public ActionResult Products(int take = 10, int language = 1, int description = 1000, int width=200, int height=200)
        {
            var items = ProductService.GetProductsRss(take,language, description,width,height);
            var comment = new StringBuilder();
            comment.AppendLine("");
            return new FeedResult(items, comment);
        }
        public ActionResult StoryCategories(int categoryId=0,int take = 10, int language = 1, int description = 1000, int width = 200, int height = 200)
        {
            var items = StoryService.GetStoryCategoriesRss(categoryId,take, language, description, width, height);
            var comment = new StringBuilder();
            comment.AppendLine("");
            return new FeedResult(items, comment);
        }
    }
}