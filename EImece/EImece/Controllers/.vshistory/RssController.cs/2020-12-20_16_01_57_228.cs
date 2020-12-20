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
                comment.AppendLine("rss/products?take=100&Language=1&Description=50&Width=50&Height=50&utm_source=test&utm_medium=test1&utm_campaign=test2");
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

        //public void ShoutBox()
        //{
        //    Response.Buffer = false;
        //    Response.Clear();
        //    Response.ContentType = "application/xml";

        //    using (XmlWriter writer = XmlWriter.Create(Response.OutputStream))
        //    {
        //        SyndicationFeed feed = new SyndicationFeed("mrfs.se: Shouts",
        //                                               "Malmö Radioflygsällskaps Shoutbox",
        //                                               new Uri("http://www.mrfs.se"));
        //        feed.Authors.Add(new SyndicationPerson("webmaster@mrfs.se", "MRFS Webmaster", "http://www.mrfs.se"));
        //        feed.Categories.Add(new SyndicationCategory("Shouts"));
        //        feed.Copyright = new TextSyndicationContent("Copyright © 2009 Malmö Radioflygsällskap");
        //        feed.Generator = "Graffen's RSS Generator";
        //        feed.Language = "se-SE";

        //        List<SyndicationItem> items = new List<SyndicationItem>();
        //        foreach (var shout in r.GetShouts(20))
        //        {
        //            SyndicationItem item = new SyndicationItem();
        //            item.Id = shout.ShoutDate + ":" + shout.ShoutedBy;
        //            item.Title = TextSyndicationContent.CreatePlaintextContent(shout.ShoutedBy);
        //            item.Content = TextSyndicationContent.CreateXhtmlContent(shout.ShoutText);
        //            item.PublishDate = shout.ShoutDate;
        //            item.Categories.Add(new SyndicationCategory("Shouts"));

        //            items.Add(item);
        //        }
        //        feed.Items = items;
        //        Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed);
        //        if (writer != null)
        //        {
        //            rssFormatter.WriteTo(writer);
        //            writer.Flush();
        //        }
        //    }
        //    Response.End();
        //}
    }
}