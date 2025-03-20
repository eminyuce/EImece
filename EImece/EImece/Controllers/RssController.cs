using EImece.Domain;
using EImece.Domain.Helpers.ActionResultHelpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Text;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RssController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IStoryService StoryService { get; set; }

        [Inject]
        public IStoryCategoryService StoryCategoryService { get; set; }

        // GET: Rss
        ///
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Products(RssParams rssParams)
        {
            var comment = new StringBuilder();
            try
            {
                var items = ProductService.GetProductsRss(rssParams);
                if (items == null)
                {
                    return Content("No RSS for Stories");
                }
                comment.AppendLine("rss/products?take=100&language=1&Description=50&Width=50&Height=50&utm_source=test&utm_medium=test1&utm_campaign=test2");
                return new FeedResult(items, comment);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return Content(ex.Message);
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult StoryCategories(RssParams rssParams)
        {
            var comment = new StringBuilder();
            try
            {
                var items = StoryService.GetStoryCategoriesRss(rssParams);
                if (items == null)
                {
                    return Content("No RSS for Stories");
                }
                comment.AppendLine("/rss/storycategories?take=10&language=1&categoryId=53&description=250");
                return new FeedResult(items, comment);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return Content(ex.Message);
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult StoryCategoriesFull(RssParams rssParams)
        {
            var comment = new StringBuilder();

            var items = StoryService.GetStoryCategoriesRssFull(rssParams);
            if (items == null)
            {
                return Content("No RSS for Stories");
            }
            comment.AppendLine("/rss/storycategories?take=10&language=1&categoryId=53&description=250");
            return new FeedResult(items, comment);
        }
       
    }
}