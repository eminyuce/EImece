using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class TagRepository : BaseEntityRepository<Tag>, ITagRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<Tag> GetAdminPageList(string search, int language)
        {
            Expression<Func<Tag, object>> includeProperty2 = r => r.TagCategory;
            Expression<Func<Tag, object>>[] includeProperties = { includeProperty2 };
            var tags = GetAllIncluding(includeProperties).Where(r => r.Lang == language);
            if (!String.IsNullOrEmpty(search))
            {
                tags = tags.Where(r => r.Name.ToLower().Contains(search.Trim().ToLower()));
            }
            var result = tags.OrderBy(r => r.Position).ThenByDescending(r => r.Id).ToList();

            return result;
        }

        public Tag GetTagById(int tagId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.StoryTags);
            return GetSingleIncluding(tagId, includeProperties.ToArray());
        }
    }
}