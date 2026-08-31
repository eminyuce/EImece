using Microsoft.Extensions.Logging;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;

namespace EImece.Domain.Repositories
{
    public class StoryCategoryRepository : BaseContentRepository<StoryCategory>, IStoryCategoryRepository
    {
        public StoryCategoryRepository(IEImeceContext dbContext, ILogger<StoryCategoryRepository> logger) : base(dbContext, logger) {
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        private static Expression<Func<StoryCategory, StorefrontCategoryDto>> StoryCategoryProjection
        {
            get
            {
                return sc => new StorefrontCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    ShortDescription = sc.Description,
                    Description = sc.Description,
                    MainImageId = sc.MainImageId,
                    Position = sc.Position,
                    Lang = sc.Lang,
                    IsActive = sc.IsActive,
                    IsStoryCategory = true,
                    PageTheme = sc.PageTheme,
                    ProductCount = sc.Stories.Count(s => s.IsActive)
                };
            }
        }

        public async Task<List<StorefrontCategoryDto>> GetStorefrontActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.StoryCategories.AsNoTracking()
                .Where(r => r.IsActive && r.Lang == language && r.Stories.Any(s => s.IsActive))
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate)
                .Select(StoryCategoryProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontCategoryDto> GetStorefrontActiveStoryCategories(int language)
        {
            return EImeceDbContext.StoryCategories.AsNoTracking()
                .Where(r => r.IsActive && r.Lang == language && r.Stories.Any(s => s.IsActive))
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.UpdatedDate)
                .Select(StoryCategoryProjection)
                .ToList();
        }

        public async Task<StorefrontCategoryDto> GetStorefrontStoryCategoryByIdAsync(int storyCategoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.StoryCategories.AsNoTracking()
                .Where(sc => sc.Id == storyCategoryId && sc.IsActive)
                .Select(StoryCategoryProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontCategoryDto GetStorefrontStoryCategoryById(int storyCategoryId)
        {
            return EImeceDbContext.StoryCategories.AsNoTracking()
                .Where(sc => sc.Id == storyCategoryId && sc.IsActive)
                .Select(StoryCategoryProjection)
                .FirstOrDefault();
        }

        #endregion

        public List<StoryCategory> GetActiveStoryCategories(int language)
        {
            // EImeceDbContext.Configuration.LazyLoadingEnabled = false;
            // EImeceDbContext.Database.Log = s => StoryCategoryLogger.Trace(s);
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            // includeProperties.Add(r => r.Stories);
            Expression<Func<StoryCategory, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.Stories.Any();
            Expression<Func<StoryCategory, int>> keySelector = t => t.Position;
            var item = FindAllIncluding(match, keySelector, OrderByType.Descending, null, null, includeProperties.ToArray());
            //var item = FindAll(match,keySelector,OrderByType.Descending, null,null);
            // var item =this.EImeceDbContext.StoryCategories.Where(match).OrderBy(keySelector).ThenByDescending(r => r.UpdatedDate).ToList();
            // EImeceDbContext.Database.Log = s => StoryCategoryLogger.Trace(s);
            return item.ToList();
        }

        public async Task<List<StoryCategory>> GetActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            Expression<Func<StoryCategory, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.Stories.Any();
            Expression<Func<StoryCategory, int>> keySelector = t => t.Position;
            var item = FindAllIncluding(match, keySelector, OrderByType.Descending, null, null, includeProperties.ToArray());
            return await item.ToListAsync(cancellationToken).ConfigureAwait(false);
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