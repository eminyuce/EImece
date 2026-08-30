using EImece.Web.Controllers;
using EImece.Domain;
using EImece.Web.Infrastructure.ActionResults;
using EImece.Web.Filters;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class RssController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IProductService ProductService;
        private readonly IStoryService StoryService;
        private readonly IStoryCategoryService StoryCategoryService;

        public RssController(
            ISettingService settingService,
            AutoMapper.IMapper mapper,
            IProductService productService,
            IStoryService storyService,
            IStoryCategoryService storyCategoryService)
            : base(settingService, mapper)
        {
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
            StoryService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            StoryCategoryService = storyCategoryService ?? throw new ArgumentNullException(nameof(storyCategoryService));
        }

        // GET: Rss
        /// rss/products/?Take=10&Description=1&CategoryId=2&Language=1&Width=300&Height=250&utm_source=google&utm_medium=cpc&utm_campaign=spring_sale&utm_term=shoes&utm_content=ad1
        [CustomOutputCache(CacheProfile = Constants.Cache1Day)]
        public async Task<ActionResult> Products(RssParams rssParams)
        {
            var comment = new StringBuilder();
            try
            {
                var items = await ProductService.GetProductsRssAsync(rssParams);
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

        [CustomOutputCache(CacheProfile = Constants.Cache1Day)]
        public async Task<ActionResult> StoryCategories(RssParams rssParams)
        {
            var comment = new StringBuilder();
            try
            {
                var items = await StoryService.GetStoryCategoriesRssAsync(rssParams);
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

        [CustomOutputCache(CacheProfile = Constants.Cache1Day)]
        public async Task<ActionResult> ProductCategories(RssParams rssParams)
        {
            var comment = new StringBuilder();
            try
            {
                var items = await ProductService.GetProductCategoriesRssAsync(rssParams);
                if (items == null)
                {
                    return Content("No RSS for Products");
                }
                comment.AppendLine("/rss/productcategories?take=10&language=1&categoryId=1&description=200");
                return new FeedResult(items, comment);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return Content(ex.Message);
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache1Day)]
        public async Task<ActionResult> StoryCategoriesFull(RssParams rssParams)
        {
            var comment = new StringBuilder();

            var items = await StoryService.GetStoryCategoriesRssFullAsync(rssParams);
            if (items == null)
            {
                return Content("No RSS for Stories");
            }
            comment.AppendLine("/rss/storycategories?take=10&language=1&categoryId=53&description=250");
            return new FeedResult(items, comment);
        }
    }
}