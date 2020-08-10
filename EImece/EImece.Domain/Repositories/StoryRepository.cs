using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class StoryRepository : BaseContentRepository<Story>, IStoryRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public StoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<Story> GetAdminPageList(int categoryId, string search, int lang)
        {
            Expression<Func<Story, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Story, object>> includeProperty2 = r => r.StoryCategory;
            Expression<Func<Story, object>>[] includeProperties = { includeProperty2, includeProperty3 };
            var stories = GetAllIncluding(includeProperties).Where(r => r.Lang == lang);
            if (!String.IsNullOrEmpty(search))
            {
                stories = stories.Where(r => r.Name.ToLower().Contains(search));
            }
            if (categoryId > 0)
            {
                stories = stories.Where(r => r.StoryCategoryId == categoryId);
            }
            stories = stories.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);

            return stories.ToList();
        }

        public List<Story> GetFeaturedStories(int take, int language, int excludedStoryId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.StoryTags.Select(r1 => r1.Tag));
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.StoryCategory);
            Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.IsFeaturedStory && r2.Id != excludedStoryId;
            Expression<Func<Story, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Ascending, take, 0, includeProperties.ToArray());

            return result.ToList();
        }

        public List<Story> GetLatestStories(int language, int take)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.StoryCategory);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.StoryTags.Select(q => q.Tag));
            Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.MainPage && r2.Lang == language;
            Expression<Func<Story, DateTime>> keySelector = t => t.UpdatedDate;
            var items = this.FindAllIncluding(match, keySelector, OrderByType.Descending, take, 0, includeProperties.ToArray());

            return items.ToList();
        }

        public PaginatedList<Story> GetMainPageStories(int pageIndex, int pageSize, int language)
        {
            try
            {
                var includeProperties = GetIncludePropertyExpressionList();
                includeProperties.Add(r => r.StoryCategory);
                includeProperties.Add(r => r.MainImage);
                includeProperties.Add(r => r.StoryFiles);
                includeProperties.Add(r => r.StoryTags.Select(q => q.Tag));
                Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.MainPage && r2.Lang == language;
                Expression<Func<Story, int>> keySelector = t => t.Position;
                var items = this.PaginateDescending(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());

                return items;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, exception.Message);
                throw;
            }
        }

        public List<Story> GetRelatedStories(int[] tagIdList, int take, int lang, int excludedStoryId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.StoryTags.Select(r1 => r1.Tag));
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.StoryCategory);
            Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.Lang == lang && r2.StoryTags.Any(t => tagIdList.Contains(t.TagId)) && r2.Id != excludedStoryId;
            Expression<Func<Story, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Ascending, take, 0, includeProperties.ToArray());

            return result.ToList();
        }

        public PaginatedList<Story> GetStoriesByStoryCategoryId(int storyCategoryId, int language, int pageIndex, int pageSize)
        {
            try
            {
                var includeProperties = GetIncludePropertyExpressionList();
                includeProperties.Add(r => r.MainImage);
                includeProperties.Add(r => r.StoryFiles);
                includeProperties.Add(r => r.StoryTags.Select(q => q.Tag));
                Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.StoryCategoryId == storyCategoryId && r2.MainPage && r2.Lang == language;
                Expression<Func<Story, int>> keySelector = t => t.Position;
                var items = this.PaginateDescending(pageIndex, pageSize, keySelector, match, includeProperties.ToArray());

                return items;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, exception.Message);
                throw;
            }
        }

        public Story GetStoryById(int storyId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.StoryCategory);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.StoryFiles.Select(t => t.FileStorage));
            includeProperties.Add(r => r.StoryTags.Select(q => q.Tag));
            return GetSingleIncluding(storyId, includeProperties.ToArray());
        }
    }
}