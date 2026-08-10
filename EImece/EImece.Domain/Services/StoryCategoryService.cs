using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class StoryCategoryService : BaseContentService<StoryCategory>, IStoryCategoryService
    {
        private static readonly Logger StoryCategoryServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IStoryService StoryService { get; set; }

        private IStoryCategoryRepository StoryCategoryRepository { get; set; }

        public StoryCategoryService(IStoryCategoryRepository repository) : base(repository)
        {
            StoryCategoryRepository = repository;
        }

        public StoryCategory GetStoryCategoryById(int storyCategoryId)
        {
            return StoryCategoryRepository.GetStoryCategoryById(storyCategoryId);
        }

        public void DeleteStoryCategoryById(int storyCategoryId)
        {
            var storyCategory = GetStoryCategoryById(storyCategoryId);
            if (storyCategory.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(storyCategory.MainImageId.Value);
            }
            var storyIdList = storyCategory.Stories.Select(r => r.Id).ToList();
            foreach (var id in storyIdList)
            {
                StoryService.DeleteStoryById(id);
            }
            DeleteEntity(storyCategory);
        }

        public async Task DeleteStoryCategoryByIdAsync(int storyCategoryId)
        {
            var storyCategory = GetStoryCategoryById(storyCategoryId);
            if (storyCategory.MainImageId.HasValue)
            {
                await FileStorageService.DeleteFileStorageAsync(storyCategory.MainImageId.Value).ConfigureAwait(false);
            }
            var storyIdList = storyCategory.Stories.Select(r => r.Id).ToList();
            foreach (var id in storyIdList)
            {
                await StoryService.DeleteStoryByIdAsync(id).ConfigureAwait(false);
            }
            await DeleteEntityAsync(storyCategory).ConfigureAwait(false);
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteStoryCategoryById(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                StoryCategoryServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                StoryCategoryServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    await DeleteStoryCategoryByIdAsync(id).ConfigureAwait(false);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                StoryCategoryServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                StoryCategoryServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public List<StoryCategory> GetActiveStoryCategories(int language)
        {
            return StoryCategoryRepository.GetActiveStoryCategories(language);
        }

        public async Task<List<StoryCategory>> GetActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await StoryCategoryRepository.GetActiveStoryCategoriesAsync(language, cancellationToken).ConfigureAwait(false);
        }
    }
}