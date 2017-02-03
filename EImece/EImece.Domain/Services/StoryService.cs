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

namespace EImece.Domain.Services
{
    public class StoryService : BaseContentService<Story>, IStoryService
    {

        private static readonly Logger StoryServiceLogger = LogManager.GetCurrentClassLogger();

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
            var storyDetail = new StoryDetailViewModel();
            storyDetail.Story = GetStoryById(storyId);
            return storyDetail;
        }

        public StoryIndexViewModel GetMainPageStories(int page, int language)
        {
            var result = new StoryIndexViewModel();
            var cacheKey = String.Format("GetMainPageStories-{0}-{1}", page, language);

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                int pageSize = Settings.RecordPerPage;
                result.Stories = StoryRepository.GetMainPageStories(page, pageSize, language);
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



    }
}
