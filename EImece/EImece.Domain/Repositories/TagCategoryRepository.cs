using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class TagCategoryRepository : BaseEntityRepository<TagCategory>, ITagCategoryRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TagCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
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
                Expression<Func<TagCategory, bool>> match = r2 => r2.IsActive && r2.Lang == (int)language && r2.Tags.Count() > 0;
                Expression<Func<TagCategory, int>> keySelector = t => t.Position;
                Expression<Func<TagCategory, object>>[] includeProperties = { includeProperty1 };
                var result = this.FindAllIncluding(match, keySelector, OrderByType.Ascending, null, null, includeProperties);

                return result.ToList();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, exception.Message);
                throw;
            }
        }
    }
}