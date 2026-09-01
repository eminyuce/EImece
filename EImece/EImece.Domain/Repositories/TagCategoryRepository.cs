using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class TagCategoryRepository : BaseEntityRepository<TagCategory>, ITagCategoryRepository
    {
        private readonly ILogger<TagCategoryRepository> _logger;

        public TagCategoryRepository(IEImeceContext dbContext, ILogger<TagCategoryRepository> logger) : base(dbContext, logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public TagCategory GetTagCategoryById(int tagCategoryId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Tags.Select(t => t.ProductTags));
            includeProperties.Add(r => r.Tags.Select(t => t.StoryTags));
            var item = GetSingleIncluding(tagCategoryId, includeProperties.ToArray());
            return item;
        }

        public List<TagCategory> GetTagsByTagType(EImeceLanguage language)
        {
            try
            {
                Expression<Func<TagCategory, object>> includeProperty1 = r => r.Tags;
                Expression<Func<TagCategory, bool>> match = r2 => r2.IsActive && r2.Lang == (int)language && r2.Tags.Any();
                Expression<Func<TagCategory, int>> keySelector = t => t.Position;
                Expression<Func<TagCategory, object>>[] includeProperties = { includeProperty1 };
                var result = this.FindAllIncluding(match, keySelector, OrderByType.Ascending, null, null, includeProperties);

                return result.ToList();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                throw;
            }
        }

        public async Task<List<TagCategory>> GetTagsByTagTypeAsync(EImeceLanguage language, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Expression<Func<TagCategory, object>> includeProperty1 = r => r.Tags;
                Expression<Func<TagCategory, bool>> match = r2 => r2.IsActive && r2.Lang == (int)language && r2.Tags.Any();
                Expression<Func<TagCategory, int>> keySelector = t => t.Position;
                Expression<Func<TagCategory, object>>[] includeProperties = { includeProperty1 };
                var result = this.FindAllIncluding(match, keySelector, OrderByType.Ascending, null, null, includeProperties);

                return await result.ToListAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "GetTagsByTagTypeAsync failed.");
                throw new InvalidOperationException("GetTagsByTagTypeAsync failed.", exception);
            }
        }
    }
}