using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class StoryCategoryRepository : BaseContentRepository<StoryCategory>, IStoryCategoryRepository
    {
        private static readonly Logger StoryCategoryLogger = LogManager.GetCurrentClassLogger();

        public StoryCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<StoryCategory> GetActiveStoryCategories(int language)
        {
            // EImeceDbContext.Configuration.LazyLoadingEnabled = false;
            // EImeceDbContext.Database.Log = s => StoryCategoryLogger.Trace(s);
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            // includeProperties.Add(r => r.Stories);
            Expression<Func<StoryCategory, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.Stories.Count() > 0;
            Expression<Func<StoryCategory, int>> keySelector = t => t.Position;
            var item = FindAllIncluding(match, keySelector, OrderByType.Descending, null, null, includeProperties.ToArray());
            //var item = FindAll(match,keySelector,OrderByType.Descending, null,null);
            // var item =this.EImeceDbContext.StoryCategories.Where(match).OrderBy(keySelector).ThenByDescending(r => r.UpdatedDate).ToList();
            // EImeceDbContext.Database.Log = s => StoryCategoryLogger.Trace(s);
            return item.ToList();
        }

        public StoryCategory GetStoryCategoryById(int storyCategoryId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.Stories.Select(t => t.StoryFiles.Select(q => q.FileStorage)));
            includeProperties.Add(r => r.Stories.Select(t => t.StoryTags.Select(q => q.Tag)));
            var item = GetSingleIncluding(storyCategoryId, includeProperties.ToArray());
            return item;
        }
    }
}