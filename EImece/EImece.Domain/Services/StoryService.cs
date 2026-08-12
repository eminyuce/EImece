using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<List<Story>> GetAdminPageListAsync(int categoryId, string search, int lang)
        {
            return await StoryRepository.GetAdminPageListAsync(categoryId, search, lang).ConfigureAwait(false);
        }

        public List<StoryTag> GetStoryTagsByStoryId(int storyId)
        {
            return StoryTagRepository.GetStoryTagsByStoryId(storyId);
        }

        public async Task<List<StoryTag>> GetStoryTagsByStoryIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await StoryTagRepository.GetStoryTagsByStoryIdAsync(storyId, cancellationToken).ConfigureAwait(false);
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

        public async Task DeleteStoryByIdAsync(int storyId)
        {
            var story = await GetStoryByIdAsync(storyId).ConfigureAwait(false);
            await StoryTagRepository.DeleteByWhereConditionAsync(r => r.StoryId == storyId).ConfigureAwait(false);
            if (story.MainImageId.HasValue)
            {
                await FileStorageService.DeleteFileStorageAsync(story.MainImageId.Value).ConfigureAwait(false);
            }
            if (story.StoryFiles != null)
            {
                var menuFiles = new List<StoryFile>(story.StoryFiles);
                foreach (var file in menuFiles)
                {
                    await FileStorageService.DeleteUploadImageByFileStorageAsync(storyId, MediaModType.Stories, file.FileStorageId).ConfigureAwait(false);
                }
                await StoryFileRepository.DeleteByWhereConditionAsync(r => r.StoryId == storyId).ConfigureAwait(false);
            }
            await DeleteEntityAsync(story).ConfigureAwait(false);
        }

        public Story GetStoryById(int storyId)
        {
            return StoryRepository.GetStoryById(storyId);
        }

        public async Task<Story> GetStoryByIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await StoryRepository.GetStoryByIdAsync(storyId, cancellationToken).ConfigureAwait(false);
        }

        public void SaveStoryTags(int storyId, int[] tags)
        {
            StoryTagRepository.SaveStoryTags(storyId, tags);
        }

        public async Task SaveStoryTagsAsync(int storyId, int[] tags)
        {
            await StoryTagRepository.SaveStoryTagsAsync(storyId, tags).ConfigureAwait(false);
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
            result.NextStory = StoryRepository.GetNextStory(storyId, language);
            result.PreviousStory = StoryRepository.GetPreviousStory(storyId, language);
            result.RelatedProducts = new List<Product>();
            if (result.Story != null && result.Story.StoryTags.Any())
            {
                var tagIdList = result.Story.StoryTags.Select(t => t.TagId).ToArray();
                result.RelatedProducts = ProductRepository.GetRelatedProducts(tagIdList, 10, result.Story.Lang, 0);
            }
            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, language).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            string menuLink = "stories-categories_" + result.Story.GetSeoUrl();
            result.BlogMenu = MenuService.GetActiveBaseContentsFromCache(true, language).FirstOrDefault(r1 => r1.MenuLink.Equals(menuLink, StringComparison.InvariantCultureIgnoreCase));
            // Sidebar/footer tag cloud needs story ItemCount (same source as category pages).
            result.Tags = TagService.GetTagsWithStoryCounts(language, minStoryCount: 1)
                .OrderByDescending(t => t.ItemCount)
                .ThenBy(t => t.Name)
                .ToList();
            result.StoryCategories = StoryCategoryService.GetActiveStoryCategories(language);
            result.SocialMediaLinks = CreateStoryShareLinks(result.Story);
            return result;
        }

        public async Task<StoryDetailViewModel> GetStoryDetailViewModelAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new StoryDetailViewModel();
            result.Story = await GetStoryByIdAsync(storyId, cancellationToken).ConfigureAwait(false);
            int language = result.Story.Lang;
            result.RelatedStories = new List<Story>();
            if (result.Story != null && result.Story.StoryTags.Any())
            {
                var tagIdList = result.Story.StoryTags.Select(t => t.TagId).ToArray();
                result.RelatedStories = await StoryRepository.GetRelatedStoriesAsync(tagIdList, 10, language, storyId, cancellationToken).ConfigureAwait(false);
            }
            result.FeaturedStories = await StoryRepository.GetFeaturedStoriesAsync(10, language, storyId, cancellationToken).ConfigureAwait(false);
            result.NextStory = await StoryRepository.GetNextStoryAsync(storyId, language, cancellationToken).ConfigureAwait(false);
            result.PreviousStory = await StoryRepository.GetPreviousStoryAsync(storyId, language, cancellationToken).ConfigureAwait(false);
            result.RelatedProducts = new List<Product>();
            if (result.Story != null && result.Story.StoryTags.Any())
            {
                var tagIdList = result.Story.StoryTags.Select(t => t.TagId).ToArray();
                result.RelatedProducts = await ProductRepository.GetRelatedProductsAsync(tagIdList, 10, result.Story.Lang, 0, cancellationToken).ConfigureAwait(false);
            }
            var menus = await MenuService.GetActiveBaseContentsFromCacheAsync(true, language).ConfigureAwait(false);
            result.MainPageMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            string menuLink = "stories-categories_" + result.Story.GetSeoUrl();
            result.BlogMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals(menuLink, StringComparison.InvariantCultureIgnoreCase));
            result.Tags = (await TagService.GetTagsWithStoryCountsAsync(language, minStoryCount: 1, cancellationToken).ConfigureAwait(false))
                .OrderByDescending(t => t.ItemCount)
                .ThenBy(t => t.Name)
                .ToList();
            result.StoryCategories = await StoryCategoryService.GetActiveStoryCategoriesAsync(language, cancellationToken).ConfigureAwait(false);
            result.SocialMediaLinks = CreateStoryShareLinks(result.Story);
            return result;
        }

        private Dictionary<string, string> CreateStoryShareLinks(Story story)
        {
            if (story == null)
            {
                return new Dictionary<string, string>();
            }

            var imageUrl = string.Empty;
            if (story.MainImageId.HasValue)
            {
                imageUrl = story.GetCroppedImageUrl(story.MainImageId, 1000, 0, true) ?? string.Empty;
            }

            return SettingService.CreateShareableSocialMediaLinks(story.DetailPageUrl, story.Name, imageUrl);
        }

        public async Task<StoryIndexViewModel> GetMainPageStoriesAsync(int page, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = String.Format("GetMainPageStories-{0}-{1}", page, currentLanguage) + AsyncCacheKeySuffix;

            return await DataCachingProvider.GetOrAddAsync(cacheKey, async () =>
            {
                var vm = new StoryIndexViewModel();
                int pageSize = AppConfig.RecordPerPage;
                vm.Stories = await StoryRepository.GetMainPageStoriesAsync(page, pageSize, currentLanguage, CancellationToken.None).ConfigureAwait(false);
                vm.StoryCategories = await StoryCategoryService.GetActiveStoryCategoriesAsync(currentLanguage, CancellationToken.None).ConfigureAwait(false);
                return vm;
            }, AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        public StoryIndexViewModel GetMainPageStories(int page, int language)
        {
            var cacheKey = String.Format("GetMainPageStories-{0}-{1}", page, language);

            var result = DataCachingProvider.GetOrAdd(cacheKey, () =>
            {
                var vm = new StoryIndexViewModel();
                int pageSize = AppConfig.RecordPerPage;
                vm.Stories = StoryRepository.GetMainPageStories(page, pageSize, language);
                vm.StoryCategories = StoryCategoryService.GetActiveStoryCategories(language);
                return vm;
            }, AppConfig.CacheMediumSeconds);
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

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    await DeleteStoryByIdAsync(id).ConfigureAwait(false);
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
            // Story category sidebar links to /s/t/{tag}. Only show tags that have at least one active story.
            result.Tags = TagService.GetTagsWithStoryCounts(lang, minStoryCount: 1)
                .OrderByDescending(t => t.ItemCount)
                .ThenBy(t => t.Name)
                .ToList();
            result.Stories = StoryRepository.GetStoriesByStoryCategoryId(storyCategoryId, result.StoryCategory.Lang, page, pageSize);
            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, lang).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));

            return result;
        }

        public async Task<StoryCategoryViewModel> GetStoryCategoriesViewModelAsync(int storyCategoryId, int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new StoryCategoryViewModel();
            int pageSize = AppConfig.RecordPerPage;

            result.StoryCategory = await StoryCategoryService.GetSingleAsync(storyCategoryId).ConfigureAwait(false);
            int lang = result.StoryCategory.Lang;
            result.StoryCategories = await StoryCategoryService.GetActiveBaseContentsAsync(true, result.StoryCategory.Lang, cancellationToken).ConfigureAwait(false);
            result.Tags = (await TagService.GetTagsWithStoryCountsAsync(lang, minStoryCount: 1, cancellationToken).ConfigureAwait(false))
                .OrderByDescending(t => t.ItemCount)
                .ThenBy(t => t.Name)
                .ToList();
            result.Stories = await StoryRepository.GetStoriesByStoryCategoryIdAsync(storyCategoryId, result.StoryCategory.Lang, page, pageSize, cancellationToken).ConfigureAwait(false);
            var menus = await MenuService.GetActiveBaseContentsFromCacheAsync(true, lang).ConfigureAwait(false);
            result.MainPageMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));

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

        public async Task<SimiliarStoryTagsViewModel> GetStoriesByTagIdAsync(int tagId, int pageIndex, int pageSize, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new SimiliarStoryTagsViewModel();
            result.Tag = await TagService.GetSingleAsync(tagId).ConfigureAwait(false);
            result.ProductTags = await ProductTagRepository.GetProductsByTagIdAsync(tagId, 1, 10, currentLanguage, cancellationToken).ConfigureAwait(false);
            result.StoryTags = await StoryTagRepository.GetStoriesByTagIdAsync(tagId, pageIndex, pageSize, currentLanguage, cancellationToken).ConfigureAwait(false);
            result.CompanyName = await SettingService.GetSettingObjectByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            return result;
        }

        public Rss20FeedFormatter GetStoryCategoriesRss(RssParams rssParams)
        {
            var storyCategory = StoryCategoryService.GetSingle(rssParams.CategoryId);
            var items = StoryRepository.GetStoriesByStoryCategoryId(rssParams.CategoryId, rssParams.Language, 1, 9999).Take(rssParams.Take).ToList();

            // FIX: injected abstraction instead of static HttpContext.Current.
            var builder = new UriBuilder(AppConfig.HttpProtocol, HttpContextFactory.Create().Request.Url.Host);
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

        public async Task<Rss20FeedFormatter> GetStoryCategoriesRssAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken))
        {
            var storyCategory = await StoryCategoryService.GetSingleAsync(rssParams.CategoryId).ConfigureAwait(false);
            var paginated = await StoryRepository.GetStoriesByStoryCategoryIdAsync(rssParams.CategoryId, rssParams.Language, 1, 9999, cancellationToken).ConfigureAwait(false);
            var items = paginated.Take(rssParams.Take).ToList();

            var builder = new UriBuilder(AppConfig.HttpProtocol, HttpContextFactory.Create().Request.Url.Host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            String title = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

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
            // FIX: injected abstraction instead of static HttpContext.Current.
            var builder = new UriBuilder(AppConfig.HttpProtocol, HttpContextFactory.Create().Request.Url.Host);
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

            var urlHelper = new UrlHelper(HttpContextFactory.Create().Request.RequestContext);
            String imagePath = urlHelper.Action("StoryCategoriesFull", "Rss", null, AppConfig.HttpProtocol);

            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", imagePath.ToString()), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }

        public async Task<Rss20FeedFormatter> GetStoryCategoriesRssFullAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken))
        {
            var paginated = await StoryRepository.GetStoriesByStoryCategoryIdAsync(rssParams.CategoryId, rssParams.Language, 1, 9999, cancellationToken).ConfigureAwait(false);
            var items = paginated.Take(rssParams.Take).ToList();
            if (items.IsEmpty())
            {
                return null;
            }
            var storyCategory = await StoryCategoryService.GetSingleAsync(rssParams.CategoryId).ConfigureAwait(false);
            var builder = new UriBuilder(AppConfig.HttpProtocol, HttpContextFactory.Create().Request.Url.Host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            String title = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            feed.AddGoogleContentNameSpace();
            feed.AddYahooMediaNamespace();
            feed.LastUpdatedTime = new DateTimeOffset(items.Max(t => t.UpdatedDate));
            feed.Items = items.Select(s => s.GetStorySyndicationItemFull(storyCategory.Name, url, rssParams));

            var urlHelper = new UrlHelper(HttpContextFactory.Create().Request.RequestContext);
            String imagePath = urlHelper.Action("StoryCategoriesFull", "Rss", null, AppConfig.HttpProtocol);

            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", imagePath.ToString()), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }

        public List<Story> GetFeaturedStories(int take, int language, int storyId)
        {
            return StoryRepository.GetFeaturedStories(take, language, storyId);
        }

        public async Task<List<Story>> GetFeaturedStoriesAsync(int take, int language, int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await StoryRepository.GetFeaturedStoriesAsync(take, language, storyId, cancellationToken).ConfigureAwait(false);
        }

        public Story GetPreviousStory(int currentStoryId, int language)
        {
            return StoryRepository.GetPreviousStory(currentStoryId, language);
        }

        public Story GetNextStory(int currentStoryId, int language)
        {
            return StoryRepository.GetNextStory(currentStoryId, language);
        }
    }
}