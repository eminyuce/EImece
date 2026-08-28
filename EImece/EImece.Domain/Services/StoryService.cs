using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Observability.Telemetry;
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

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        [Timed("service.story.get_detail")]

        public virtual async Task<StorefrontStoryDetailDto> GetStorefrontStoryDetailByIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.StoryDetailAsync(storyId);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => StoryRepository.GetStorefrontStoryDetailByIdAsync(storyId, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.story.get_detail_sync")]

        public virtual StorefrontStoryDetailDto GetStorefrontStoryDetailById(int storyId)
        {
            var cacheKey = CacheKeys.StoryDetail(storyId);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => StoryRepository.GetStorefrontStoryDetailById(storyId),
                AppConfig.CacheLongSeconds);
        }

        [Timed("service.story.get_featured")]

        public virtual async Task<List<StorefrontStoryCardDto>> GetStorefrontFeaturedStoriesAsync(int take, int language, int excludedStoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.StoryPrefix + $"featured:t{take}:lang{language}:ex{excludedStoryId}:async";
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => StoryRepository.GetStorefrontFeaturedStoriesAsync(take, language, excludedStoryId, cancellationToken),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.story.get_featured_sync")]
        public virtual List<StorefrontStoryCardDto> GetStorefrontFeaturedStories(int take, int language, int excludedStoryId)
        {
            var cacheKey = CacheKeys.StoryPrefix + $"featured:t{take}:lang{language}:ex{excludedStoryId}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => StoryRepository.GetStorefrontFeaturedStories(take, language, excludedStoryId),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.story.get_latest", "Time taken to get storefront latest stories")]
        public virtual async Task<List<StorefrontStoryCardDto>> GetStorefrontLatestStoriesAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.StoryPrefix + $"latest:t{take}:lang{language}:async";
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => StoryRepository.GetStorefrontLatestStoriesAsync(take, language, cancellationToken),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.story.get_latest_sync")]
        public virtual List<StorefrontStoryCardDto> GetStorefrontLatestStories(int take, int language)
        {
            var cacheKey = CacheKeys.StoryPrefix + $"latest:t{take}:lang{language}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => StoryRepository.GetStorefrontLatestStories(take, language),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.story.get_main_page")]

        public virtual async Task<PaginatedList<StorefrontStoryCardDto>> GetStorefrontMainPageStoriesAsync(int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.MainPageStoriesAsync(language) + $":p{pageIndex}:ps{pageSize}";
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => StoryRepository.GetStorefrontMainPageStoriesAsync(pageIndex, pageSize, language, cancellationToken),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.story.get_main_page_sync")]
        public virtual PaginatedList<StorefrontStoryCardDto> GetStorefrontMainPageStories(int pageIndex, int pageSize, int language)
        {
            var cacheKey = CacheKeys.MainPageStories(language) + $":p{pageIndex}:ps{pageSize}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => StoryRepository.GetStorefrontMainPageStories(pageIndex, pageSize, language),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.story.get_by_category", "Time taken to get storefront stories by category")]
        public virtual async Task<PaginatedList<StorefrontStoryCardDto>> GetStorefrontStoriesByCategoryIdAsync(int storyCategoryId, int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.StoriesByCategoryAsync(storyCategoryId, pageIndex, pageSize, language);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => StoryRepository.GetStorefrontStoriesByCategoryIdAsync(storyCategoryId, language, pageIndex, pageSize, cancellationToken),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.story.get_by_category_sync")]
        public virtual PaginatedList<StorefrontStoryCardDto> GetStorefrontStoriesByCategoryId(int storyCategoryId, int pageIndex, int pageSize, int language)
        {
            var cacheKey = CacheKeys.StoriesByCategory(storyCategoryId, pageIndex, pageSize, language);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => StoryRepository.GetStorefrontStoriesByCategoryId(storyCategoryId, language, pageIndex, pageSize),
                AppConfig.CacheMediumSeconds);
        }

        [Timed("service.story.get_related")]

        public virtual async Task<List<StorefrontStoryCardDto>> GetStorefrontRelatedStoriesAsync(int[] tagIds, int take, int language, int excludedStoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await StoryRepository.GetStorefrontRelatedStoriesAsync(tagIds, take, language, excludedStoryId, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.story.get_related_sync")]
        public virtual List<StorefrontStoryCardDto> GetStorefrontRelatedStories(int[] tagIds, int take, int language, int excludedStoryId)
        {
            return StoryRepository.GetStorefrontRelatedStories(tagIds, take, language, excludedStoryId);
        }

        [Timed("service.story.get_next", "Time taken to get storefront next story")]
        public virtual async Task<StorefrontStoryCardDto> GetStorefrontNextStoryAsync(int currentStoryId, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await StoryRepository.GetStorefrontNextStoryAsync(currentStoryId, language, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.story.get_next_sync")]
        public virtual StorefrontStoryCardDto GetStorefrontNextStory(int currentStoryId, int language)
        {
            return StoryRepository.GetStorefrontNextStory(currentStoryId, language);
        }

        [Timed("service.story.get_previous", "Time taken to get storefront previous story")]
        public virtual async Task<StorefrontStoryCardDto> GetStorefrontPreviousStoryAsync(int currentStoryId, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await StoryRepository.GetStorefrontPreviousStoryAsync(currentStoryId, language, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.story.get_previous_sync")]
        public virtual StorefrontStoryCardDto GetStorefrontPreviousStory(int currentStoryId, int language)
        {
            return StoryRepository.GetStorefrontPreviousStory(currentStoryId, language);
        }

        private void InvalidateStoryCaches()
        {
            DataCachingProvider.ClearByPrefix(CacheKeys.StoryPrefix);
        }

        /// <summary>
        /// Story active-entity/content lists live under the story: family so the invalidator above evicts them.
        /// </summary>
        protected override string ActiveListCachePrefix
        {
            get { return CacheKeys.StoryPrefix; }
        }

        protected override void InvalidateCachesAfterMutation()
        {
            InvalidateStoryCaches();
        }

        #endregion

        #region Mutation & Invalidation

        public override Story SaveOrEditEntity(Story entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateStoryCaches();
            return saved;
        }

        public override async Task<Story> SaveOrEditEntityAsync(Story entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateStoryCaches();
            return saved;
        }

        #endregion

        #region Admin Methods (Full Entities)

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
            var story = StoryRepository.GetSingle(storyId);
            if (story == null) return;

            StoryTagRepository.DeleteByWhereCondition(r => r.StoryId == storyId);
            FileStorageService.DeleteGalleryImages(storyId, MediaModType.Stories);
            if (story.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(story.MainImageId.Value);
            }
            DeleteEntity(story);
            InvalidateStoryCaches();
        }

        public async Task DeleteStoryByIdAsync(int storyId)
        {
            var story = await StoryRepository.GetSingleAsync(storyId).ConfigureAwait(false);
            if (story == null) return;

            await StoryTagRepository.DeleteByWhereConditionAsync(r => r.StoryId == storyId).ConfigureAwait(false);
            await FileStorageService.DeleteGalleryImagesAsync(storyId, MediaModType.Stories).ConfigureAwait(false);
            if (story.MainImageId.HasValue)
            {
                await FileStorageService.DeleteFileStorageAsync(story.MainImageId.Value).ConfigureAwait(false);
            }
            await DeleteEntityAsync(story).ConfigureAwait(false);
            InvalidateStoryCaches();
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

        #endregion

        #region Storefront ViewModels

        [Timed("service.story.get_detail_view_model_sync")]
        public virtual StoryDetailViewModel GetStoryDetailViewModel(int storyId)
        {
            var result = new StoryDetailViewModel();
            var storyDetail = GetStorefrontStoryDetailById(storyId);
            if (storyDetail == null)
            {
                return result;
            }
            result.StorefrontStory = storyDetail;
            int language = storyDetail.Lang;

            var tagIdList = storyDetail.StoryTags != null && storyDetail.StoryTags.Any()
                ? storyDetail.StoryTags.Select(t => t.Id).ToArray()
                : new int[0];

            result.StorefrontRelatedStories = tagIdList.Length > 0
                ? StoryRepository.GetStorefrontRelatedStories(tagIdList, 10, language, storyId)
                : new List<Models.DTOs.Storefront.StorefrontStoryCardDto>();

            result.StorefrontFeaturedStories = StoryRepository.GetStorefrontFeaturedStories(10, language, storyId);
            result.StorefrontNextStory = StoryRepository.GetStorefrontNextStory(storyId, language);
            result.StorefrontPreviousStory = StoryRepository.GetStorefrontPreviousStory(storyId, language);
            result.RelatedProducts = new List<Models.DTOs.Storefront.StorefrontProductCardDto>();
            if (tagIdList.Length > 0)
            {
                result.RelatedProducts = ProductRepository.GetStorefrontRelatedProducts(tagIdList, 10, language, 0);
            }
            var mainPageDto = MenuService.GetStorefrontPageByMenuLink(Constants.HomeIndexMenuLink, language);
            if (mainPageDto != null)
            {
                result.MainPageMenu = new StorefrontMenuDto { Id = mainPageDto.Id, Name = mainPageDto.Name, MenuLink = mainPageDto.MenuLink };
            }
            string menuLink = "stories-categories_" + storyDetail.SeoUrl;
            var blogPageDto = MenuService.GetStorefrontPageByMenuLink(menuLink, language);
            if (blogPageDto != null)
            {
                result.BlogMenu = new StorefrontMenuDto { Id = blogPageDto.Id, Name = blogPageDto.Name, MenuLink = blogPageDto.MenuLink };
            }
            result.Tags = TagService.GetStorefrontTagsWithStoryCounts(language, minStoryCount: 1);
            result.StoryCategories = StoryCategoryService.GetStorefrontActiveStoryCategories(language);
            result.SocialMediaLinks = CreateStoryDetailShareLinks(storyDetail);
            return result;
        }

        [Timed("service.story.get_detail_view_model")]

        public virtual async Task<StoryDetailViewModel> GetStoryDetailViewModelAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new StoryDetailViewModel();
            var storyDetail = await GetStorefrontStoryDetailByIdAsync(storyId, cancellationToken).ConfigureAwait(false);
            if (storyDetail == null)
            {
                return result;
            }
            result.StorefrontStory = storyDetail;
            int language = storyDetail.Lang;

            var tagIdList = storyDetail.StoryTags != null && storyDetail.StoryTags.Any()
                ? storyDetail.StoryTags.Select(t => t.Id).ToArray()
                : new int[0];

            result.StorefrontRelatedStories = tagIdList.Length > 0
                ? await StoryRepository.GetStorefrontRelatedStoriesAsync(tagIdList, 10, language, storyId, cancellationToken).ConfigureAwait(false)
                : new List<Models.DTOs.Storefront.StorefrontStoryCardDto>();

            result.StorefrontFeaturedStories = await StoryRepository.GetStorefrontFeaturedStoriesAsync(10, language, storyId, cancellationToken).ConfigureAwait(false);
            result.StorefrontNextStory = await StoryRepository.GetStorefrontNextStoryAsync(storyId, language, cancellationToken).ConfigureAwait(false);
            result.StorefrontPreviousStory = await StoryRepository.GetStorefrontPreviousStoryAsync(storyId, language, cancellationToken).ConfigureAwait(false);
            result.RelatedProducts = new List<Models.DTOs.Storefront.StorefrontProductCardDto>();
            if (tagIdList.Length > 0)
            {
                result.RelatedProducts = await ProductRepository.GetStorefrontRelatedProductsAsync(tagIdList, 10, language, 0, cancellationToken).ConfigureAwait(false);
            }
            var mainPageDto = await MenuService.GetStorefrontPageByMenuLinkAsync(Constants.HomeIndexMenuLink, language, cancellationToken).ConfigureAwait(false);
            if (mainPageDto != null)
            {
                result.MainPageMenu = new StorefrontMenuDto { Id = mainPageDto.Id, Name = mainPageDto.Name, MenuLink = mainPageDto.MenuLink };
            }
            string menuLink = "stories-categories_" + storyDetail.SeoUrl;
            var blogPageDto = await MenuService.GetStorefrontPageByMenuLinkAsync(menuLink, language, cancellationToken).ConfigureAwait(false);
            if (blogPageDto != null)
            {
                result.BlogMenu = new StorefrontMenuDto { Id = blogPageDto.Id, Name = blogPageDto.Name, MenuLink = blogPageDto.MenuLink };
            }
            result.Tags = await TagService.GetStorefrontTagsWithStoryCountsAsync(language, minStoryCount: 1, cancellationToken).ConfigureAwait(false);
            result.StoryCategories = await StoryCategoryService.GetStorefrontActiveStoryCategoriesAsync(language, cancellationToken).ConfigureAwait(false);
            result.SocialMediaLinks = CreateStoryDetailShareLinks(storyDetail);
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

            var storyDetailUrl = story.GetDetailPageUrl("Detail", "Stories", story.StoryCategory != null ? story.StoryCategory.Name : "no_category", "", "");
            return SettingService.CreateShareableSocialMediaLinks(storyDetailUrl, story.Name, imageUrl);
        }

        private Dictionary<string, string> CreateStoryDetailShareLinks(Models.DTOs.Storefront.StorefrontStoryDetailDto storyDetail)
        {
            if (storyDetail == null)
            {
                return new Dictionary<string, string>();
            }

            var imageUrl = string.Empty;
            if (storyDetail.MainImageId.HasValue)
            {
                imageUrl = storyDetail.GetCroppedImageUrl(storyDetail.MainImageId, 1000, 0, true) ?? string.Empty;
            }

            return SettingService.CreateShareableSocialMediaLinks(storyDetail.DetailPageUrl, storyDetail.Name, imageUrl);
        }

        [Timed("service.story.get_main_page_stories")]
        public virtual async Task<StoryIndexViewModel> GetMainPageStoriesAsync(int page, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = String.Format("GetMainPageStories-{0}-{1}", page, currentLanguage) + AsyncCacheKeySuffix;

            return await DataCachingProvider.GetOrAddAsync(cacheKey, async () =>
            {
                var vm = new StoryIndexViewModel();
                int pageSize = AppConfig.RecordPerPage;
                vm.StorefrontStories = await GetStorefrontMainPageStoriesAsync(page, pageSize, currentLanguage, CancellationToken.None).ConfigureAwait(false);
                vm.StoryCategories = await StoryCategoryService.GetStorefrontActiveStoryCategoriesAsync(currentLanguage, CancellationToken.None).ConfigureAwait(false);
                return vm;
            }, AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        [Timed("service.story.get_main_page_stories_sync")]
        public virtual StoryIndexViewModel GetMainPageStories(int page, int language)
        {
            var cacheKey = String.Format("GetMainPageStories-{0}-{1}", page, language);

            var result = DataCachingProvider.GetOrAdd(cacheKey, () =>
            {
                var vm = new StoryIndexViewModel();
                int pageSize = AppConfig.RecordPerPage;
                vm.StorefrontStories = GetStorefrontMainPageStories(page, pageSize, language);
                vm.StoryCategories = StoryCategoryService.GetStorefrontActiveStoryCategories(language);
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

        [Timed("service.story.get_categories_view_model_sync")]
        public virtual StoryCategoryViewModel GetStoryCategoriesViewModel(int storyCategoryId, int page)
        {
            var result = new StoryCategoryViewModel();
            int pageSize = AppConfig.RecordPerPage;

            result.StoryCategory = StoryCategoryService.GetStorefrontStoryCategoryById(storyCategoryId);
            if (result.StoryCategory == null)
            {
                return null;
            }
            int lang = result.StoryCategory.Lang;
            result.StoryCategories = StoryCategoryService.GetStorefrontActiveStoryCategories(lang);
            result.Tags = TagService.GetStorefrontTagsWithStoryCounts(lang, minStoryCount: 1);
            result.StorefrontStories = GetStorefrontStoriesByCategoryId(storyCategoryId, page, pageSize, lang);
            var mainPageDto = MenuService.GetStorefrontPageByMenuLink(Constants.HomeIndexMenuLink, lang);
            if (mainPageDto != null)
            {
                result.MainPageMenu = new StorefrontMenuDto { Id = mainPageDto.Id, Name = mainPageDto.Name, MenuLink = mainPageDto.MenuLink };
            }

            return result;
        }

        [Timed("service.story.get_categories_view_model")]
        public virtual async Task<StoryCategoryViewModel> GetStoryCategoriesViewModelAsync(int storyCategoryId, int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new StoryCategoryViewModel();
            int pageSize = AppConfig.RecordPerPage;

            result.StoryCategory = await StoryCategoryService.GetStorefrontStoryCategoryByIdAsync(storyCategoryId, cancellationToken).ConfigureAwait(false);
            if (result.StoryCategory == null)
            {
                return null;
            }
            int lang = result.StoryCategory.Lang;
            result.StoryCategories = await StoryCategoryService.GetStorefrontActiveStoryCategoriesAsync(lang, cancellationToken).ConfigureAwait(false);
            result.Tags = await TagService.GetStorefrontTagsWithStoryCountsAsync(lang, minStoryCount: 1, cancellationToken).ConfigureAwait(false);
            result.StorefrontStories = await GetStorefrontStoriesByCategoryIdAsync(storyCategoryId, page, pageSize, lang, cancellationToken).ConfigureAwait(false);
            var mainPageDto = await MenuService.GetStorefrontPageByMenuLinkAsync(Constants.HomeIndexMenuLink, lang, cancellationToken).ConfigureAwait(false);
            if (mainPageDto != null)
            {
                result.MainPageMenu = new StorefrontMenuDto { Id = mainPageDto.Id, Name = mainPageDto.Name, MenuLink = mainPageDto.MenuLink };
            }

            return result;
        }

        #endregion

        public List<Story> GetLatestStories(int language, int take)
        {
            return StoryRepository.GetLatestStories(language, take);
        }
        

        public SimiliarStoryTagsViewModel GetStoriesByTagId(int tagId, int pageIndex, int pageSize, int currentLanguage)
        {
            var result = new SimiliarStoryTagsViewModel();
            result.Tag = TagService.GetStorefrontTagById(tagId);

            // Projected story cards by tag — no entity graphs; the unused ProductTags leg was removed.
            result.StoryTags = StoryRepository.GetStorefrontStoriesByTagId(tagId, pageIndex, pageSize, currentLanguage);

            result.CompanyName = SettingService.GetSettingValueDtoByKey(Constants.CompanyName);
            return result;
        }

        public async Task<SimiliarStoryTagsViewModel> GetStoriesByTagIdAsync(int tagId, int pageIndex, int pageSize, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new SimiliarStoryTagsViewModel();
            result.Tag = await TagService.GetStorefrontTagByIdAsync(tagId, cancellationToken).ConfigureAwait(false);

            // Projected story cards by tag — no entity graphs; the unused ProductTags leg was removed.
            result.StoryTags = await StoryRepository.GetStorefrontStoriesByTagIdAsync(tagId, pageIndex, pageSize, currentLanguage, cancellationToken).ConfigureAwait(false);

            result.CompanyName = await SettingService.GetSettingValueDtoByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            return result;
        }

        public Rss20FeedFormatter GetStoryCategoriesRss(RssParams rssParams)
        {
            var storyCategory = StoryCategoryService.GetSingle(rssParams.CategoryId);
            var items = StoryRepository.GetStoriesByStoryCategoryId(rssParams.CategoryId, rssParams.Language, 1, rssParams.Take).ToList();

            var request = HttpContextFactory.Create()?.Request;
            var host = request?.Url?.Host ?? "localhost";
            var builder = new UriBuilder(AppConfig.HttpProtocol, host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            String title = SettingService.GetSettingByKey(Constants.CompanyName);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            feed.Items = items.Select(s => s.GetStorySyndicationItem(storyCategory?.Name ?? "", url, rssParams));

            return new Rss20FeedFormatter(feed);
        }

        public async Task<Rss20FeedFormatter> GetStoryCategoriesRssAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = HttpContextFactory.Create()?.Request;
            var host = request?.Url?.Host ?? "localhost";
            var builder = new UriBuilder(AppConfig.HttpProtocol, host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));

            var storyCategory = await StoryCategoryService.GetSingleAsync(rssParams.CategoryId).ConfigureAwait(false);
            var paginated = await StoryRepository.GetStoriesByStoryCategoryIdAsync(rssParams.CategoryId, rssParams.Language, 1, rssParams.Take, cancellationToken).ConfigureAwait(false);
            var items = paginated.ToList();

            String title = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            feed.Items = items.Select(s => s.GetStorySyndicationItem(storyCategory?.Name ?? "", url, rssParams));

            return new Rss20FeedFormatter(feed);
        }

        public Rss20FeedFormatter GetStoryCategoriesRssFull(RssParams rssParams)
        {
            var items = StoryRepository.GetStoriesByStoryCategoryId(rssParams.CategoryId, rssParams.Language, 1, rssParams.Take).ToList();
            if (items.IsEmpty())
            {
                return null;
            }
            var storyCategory = StoryCategoryService.GetSingle(rssParams.CategoryId);
            var request = HttpContextFactory.Create()?.Request;
            var host = request?.Url?.Host ?? "localhost";
            var builder = new UriBuilder(AppConfig.HttpProtocol, host);
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
            feed.Items = items.Select(s => s.GetStorySyndicationItemFull(storyCategory?.Name ?? "", url, rssParams));

            var urlHelper = request?.RequestContext != null ? new UrlHelper(request.RequestContext) : null;
            String imagePath = urlHelper?.Action("StoryCategoriesFull", "Rss", null, AppConfig.HttpProtocol)
                ?? $"{url}/rss/StoryCategoriesFull";

            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", imagePath), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }

        public async Task<Rss20FeedFormatter> GetStoryCategoriesRssFullAsync(RssParams rssParams, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = HttpContextFactory.Create()?.Request;
            var host = request?.Url?.Host ?? "localhost";
            var builder = new UriBuilder(AppConfig.HttpProtocol, host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            var urlHelper = request?.RequestContext != null ? new UrlHelper(request.RequestContext) : null;
            String imagePath = urlHelper?.Action("StoryCategoriesFull", "Rss", null, AppConfig.HttpProtocol)
                ?? $"{url}/rss/StoryCategoriesFull";

            var paginated = await StoryRepository.GetStoriesByStoryCategoryIdAsync(rssParams.CategoryId, rssParams.Language, 1, rssParams.Take, cancellationToken).ConfigureAwait(false);
            var items = paginated.ToList();
            if (items.IsEmpty())
            {
                return null;
            }
            var storyCategory = await StoryCategoryService.GetSingleAsync(rssParams.CategoryId).ConfigureAwait(false);

            String title = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            feed.AddGoogleContentNameSpace();
            feed.AddYahooMediaNamespace();
            feed.LastUpdatedTime = new DateTimeOffset(items.Max(t => t.UpdatedDate));
            feed.Items = items.Select(s => s.GetStorySyndicationItemFull(storyCategory?.Name ?? "", url, rssParams));

            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", imagePath), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

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
