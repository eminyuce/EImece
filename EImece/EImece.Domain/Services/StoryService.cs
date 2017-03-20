using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Helpers;
using NLog;
using System.Data.Entity.Validation;
using System.Linq.Expressions;
using System.ServiceModel.Syndication;
using System.Web;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;

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
                foreach (var file in story.StoryFiles)
                {
                    FileStorageService.DeleteFileStorage(file.FileStorageId);
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
            result.RelatedStories = new List<Story>();
            if (result.Story != null && result.Story.StoryTags.Any())
            {
                var tagIdList = result.Story.StoryTags.Select(t => t.TagId).ToArray();
                result.RelatedStories = StoryRepository.GetRelatedStories(tagIdList, 10, result.Story.Lang,storyId);
            }
            result.RelatedProducts = new List<Product>();
            if (result.Story != null && result.Story.StoryTags.Any())
            {
                var tagIdList = result.Story.StoryTags.Select(t => t.TagId).ToArray();
                result.RelatedProducts = ProductRepository.GetRelatedProducts(tagIdList, 10, result.Story.Lang, 0);
            }
            return result;
        }

        public StoryIndexViewModel GetMainPageStories(int page, int language)
        {
            StoryIndexViewModel result = null;
            var cacheKey = String.Format("GetMainPageStories-{0}-{1}", page, language);

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new StoryIndexViewModel();
                int pageSize = Settings.RecordPerPage;
                result.Stories = StoryRepository.GetMainPageStories(page, pageSize, language);
                result.StoryCategories = StoryCategoryService.GetActiveStoryCategories(language);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);

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
                int pageSize = Settings.RecordPerPage;

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
            result.CompanyName = SettingService.GetSettingObjectByKey(Settings.CompanyName);
            return result;
        }

        public Rss20FeedFormatter GetStoryCategoriesRss(int storyCategoryId, int take, int language, int description, int width, int height)
        {
            var storyCategory = StoryCategoryService.GetSingle(storyCategoryId);
            var items = StoryRepository.GetStoriesByStoryCategoryId(storyCategoryId, language, 1, 9999).Take(take).ToList();
            
            var builder = new UriBuilder(Settings.HttpProtocol, HttpContext.Current.Request.Url.Host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));
            String title = SettingService.GetSettingByKey(Settings.CompanyName);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            feed.AddNamespace("StoryCategories", url + "/stories/categories/"+storyCategoryId);

            feed.Items = items.Select(s => s.GetStorySyndicationItem(storyCategory.Name,url, description, width, height));

            return new Rss20FeedFormatter(feed);
        }
    }
}
