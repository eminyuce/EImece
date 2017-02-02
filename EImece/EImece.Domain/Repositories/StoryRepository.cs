using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using System.Linq.Expressions;
using GenericRepository;
using NLog;
using GenericRepository.EntityFramework.Enums;

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
            stories = stories.OrderBy(r => r.Position).ThenByDescending(r => r.Id);

            return stories.ToList();
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
        public List<Story> GetRelatedStories(int[] tagIdList, int take, int lang)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.StoryTags);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.StoryCategory);
            Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.Lang == lang && r2.StoryTags.Any(t=> tagIdList.Contains(t.TagId));
            Expression<Func<Story, int>> keySelector = t => t.Position;
            return FindAllIncluding(match, take, 0, keySelector, OrderByType.Ascending, includeProperties.ToArray());
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
