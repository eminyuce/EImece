using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace EImece.Domain.Services
{
    public class StoryService : BaseContentService<Story>, IStoryService
    {
        private static readonly Logger StoryServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ITagService TagService { get; set; }

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IProductRepository ProductRepository { get; set; }

        [Inject]
        public IStoryCategoryService StoryCategoryService { get; set; }

        private IStoryRepository StoryRepository { get; set; }

        public StoryService(IStoryRepository repository) : base(repository)
        {
            StoryRepository = repository;
        }

        public List<Story> GetAdminPageList(int categoryId, string search, int lang)
        {
            return StoryRepository.GetAdminPageList(categoryId, search, lang);
        }

        public List<StoryTag> GetStoryTagsByStoryId(int storyId)
        {
            return StoryTagRepository.GetStoryTagsByStoryId(storyId);
        }

        public void DeleteStoryById(int storyId)
        {
            var story = GetStoryById(storyId);
            StoryTagRepository.DeleteByWhereCondition(r => r.StoryId == storyId);
            if (story.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(story.MainImageId.Value);
            }
            if (story.StoryFiles != null)
            {
                var menuFiles = new List<StoryFile>(story.StoryFiles);
                foreach (var file in menuFiles)
                {
                    FileStorageService.DeleteUploadImageByFileStorage(storyId, MediaModType.Stories, file.FileStorageId);
                }
                StoryFileRepository.DeleteByWhereCondition(r => r.StoryId == storyId);
            }
            DeleteEntity(story);
        }

        public Story GetStoryById(int storyId)
        {
            return StoryRepository.GetStoryById(storyId);
        }

        public void SaveStoryTags(int storyId, int[] tags)
        {
            StoryTagRepository.SaveStoryTags(storyId, tags);
        }

        public StoryDetailViewModel GetStoryDetailViewModel(int storyId)
        {
            var result = new StoryDetailViewModel();
            result.Story = GetStoryById(storyId);
            int language = result.Story.Lang;
            result.RelatedStories = new List<Story>();
            if (result.Story != null && result.Story.StoryTags.Any())
            {
                var tagIdList = result.Story.StoryTags.Select(t => t.TagId).ToArray();
                result.RelatedStories = StoryRepository.GetRelatedStories(tagIdList, 10, language, storyId);
            }
            result.FeaturedStories = StoryRepository.GetFeaturedStories(10, language, storyId);
            result.RelatedProducts = new List<Product>();
            if (result.Story != null && result.Story.StoryTags.Any())
            {
                var tagIdList = result.Story.StoryTags.Select(t => t.TagId).ToArray();
                result.RelatedProducts = ProductRepository.GetRelatedProducts(tagIdList, 10, result.Story.Lang, 0);
            }
            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, language).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            string menuLink = "stories-categories_" + result.Story.GetSeoUrl();
            result.BlogMenu = MenuService.GetActiveBaseContentsFromCache(true, language).FirstOrDefault(r1 => r1.MenuLink.Equals(menuLink, StringComparison.InvariantCultureIgnoreCase));
            result.Tags = TagService.GetActiveBaseEntities(true, language);
            result.StoryCategories = StoryCategoryService.GetActiveStoryCategories(language);
            return result;
        }

        public StoryIndexViewModel GetMainPageStories(int page, int language)
        {
            StoryIndexViewModel result = null;
            var cacheKey = String.Format("GetMainPageStories-{0}-{1}", page, language);

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new StoryIndexViewModel();
                int pageSize = AppConfig.RecordPerPage;
                result.Stories = StoryRepository.GetMainPageStories(page, pageSize, language);
                result.StoryCategories = StoryCategoryService.GetActiveStoryCategories(language);
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
            }
            return result;
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteStoryById(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                StoryServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                StoryServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public StoryCategoryViewModel GetStoryCategoriesViewModel(int storyCategoryId, int page)
        {
            StoryCategoryViewModel result = null;

            result = new StoryCategoryViewModel();
            int pageSize = AppConfig.RecordPerPage;

            result.StoryCategory = StoryCategoryService.GetSingle(storyCategoryId);
            int lang = result.StoryCategory.Lang;
            result.StoryCategories = StoryCategoryService.GetActiveBaseContents(true, result.StoryCategory.Lang);
            result.Tags = TagService.GetActiveBaseEntities(true, lang);
            result.Stories = StoryRepository.GetStoriesByStoryCategoryId(storyCategoryId, result.StoryCategory.Lang, page, pageSize);
            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, lang).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));

            return result;
        }

        public List<Story> GetLatestStories(int language, int take)
        {
            return StoryRepository.GetLatestStories(language, take);
        }

        public SimiliarStoryTagsViewModel GetStoriesByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var result = new SimiliarStoryTagsViewModel();
            result.Tag = TagService.GetSingle(tagId);
            result.ProductTags = ProductTagRepository.GetProductsByTagId(tagId, 1, 10, lang);
            result.StoryTags = StoryTagRepository.GetStoriesByTagId(tagId, pageIndex, pageSize, lang);
            result.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
            return result;
        }

        public Rss20FeedFormatter GetStoryCategoriesRss(RssParams rssParams)
        {
            var storyCategory = StoryCategoryService.GetSingle(rssParams.CategoryId);
            var items = StoryRepository.GetStoriesByStoryCategoryId(rssParams.CategoryId, rssParams.Language, 1, 9999).Take(rssParams.Take).ToList();

            var builder = new UriBuilder(AppConfig.HttpProtocol, HttpContext.Current.Request.Url.Host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            String title = SettingService.GetSettingByKey(Constants.CompanyName);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            //feed.AddNamespace("StoryCategories", url + "/stories/categories/"+rssParams.CategoryId);

            feed.Items = items.Select(s => s.GetStorySyndicationItem(storyCategory.Name, url, rssParams));

            return new Rss20FeedFormatter(feed);
        }

        public Rss20FeedFormatter GetStoryCategoriesRssFull(RssParams rssParams)
        {
            var items = StoryRepository.GetStoriesByStoryCategoryId(rssParams.CategoryId, rssParams.Language, 1, 9999).Take(rssParams.Take).ToList();
            if (items.IsEmpty())
            {
                return null;
            }
            var storyCategory = StoryCategoryService.GetSingle(rssParams.CategoryId);
            var builder = new UriBuilder(AppConfig.HttpProtocol, HttpContext.Current.Request.Url.Host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            String title = SettingService.GetSettingByKey(Constants.CompanyName);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            feed.AddGoogleContentNameSpace();
            feed.AddYahooMediaNamespace();
            feed.LastUpdatedTime = new DateTimeOffset(items.Max(t => t.UpdatedDate));
            feed.Items = items.Select(s => s.GetStorySyndicationItemFull(storyCategory.Name, url, rssParams));

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            String imagePath = urlHelper.Action("StoryCategoriesFull", "Rss", null, AppConfig.HttpProtocol);

            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", imagePath.ToString()), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }
    }
}