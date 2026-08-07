using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        public async Task<List<Tag>> GetAdminPageListAsync(string search, int language)
        {
            Expression<Func<Tag, object>> includeProperty2 = r => r.TagCategory;
            Expression<Func<Tag, object>>[] includeProperties = { includeProperty2 };
            var tags = GetAllIncluding(includeProperties).Where(r => r.Lang == language);
            if (!String.IsNullOrEmpty(search))
            {
                tags = tags.Where(r => r.Name.ToLower().Contains(search.Trim().ToLower()));
            }
            var result = await tags.OrderBy(r => r.Position).ThenByDescending(r => r.Id).ToListAsync().ConfigureAwait(false);

            return result;
        }

        public List<Tag> GetProductTags(int language)
        {
            // Include navigation properties: ProductTags and TagCategory
            Expression<Func<Tag, object>>[] includeProperties = {
                r => r.ProductTags,
                r => r.TagCategory
            };

            // Get all tags with includes, then filter by language and active status
            var tags = GetAllIncluding(includeProperties)
                .Where(r => r.Lang == language && r.IsActive && r.TagCategory.IsActive);

            // Sort and return
            return tags
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.Id)
                .ToList();
        }

        public Tag GetTagById(int tagId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ProductTags);
            includeProperties.Add(r => r.StoryTags);
            return GetSingleIncluding(tagId, includeProperties.ToArray());
        }

        public List<Tag> GetTagsWithEntityCounts(int language, int minEntityCount = 1)
        {
            var rows = GetAll()
                .Where(t => t.IsActive && t.Lang == language)
                .Select(t => new
                {
                    Tag = t,
                    EntityCount =
                        t.ProductTags.Count(pt => pt.Product != null && pt.Product.IsActive)
                        + t.StoryTags.Count(st => st.Story != null && st.Story.IsActive)
                })
                .Where(x => x.EntityCount >= minEntityCount)
                .OrderBy(x => x.Tag.Position)
                .ThenByDescending(x => x.Tag.Id)
                .ToList();

            foreach (var row in rows)
            {
                row.Tag.ItemCount = row.EntityCount;
            }

            return rows.Select(r => r.Tag).ToList();
        }

        public List<Tag> GetTagsWithStoryCounts(int language, int minStoryCount = 1)
        {
            var rows = GetAll()
                .Where(t => t.IsActive && t.Lang == language)
                .Select(t => new
                {
                    Tag = t,
                    StoryCount = t.StoryTags.Count(st => st.Story != null && st.Story.IsActive)
                })
                .Where(x => x.StoryCount >= minStoryCount)
                .OrderByDescending(x => x.StoryCount)
                .ThenBy(x => x.Tag.Name)
                .ToList();

            foreach (var row in rows)
            {
                row.Tag.ItemCount = row.StoryCount;
            }

            return rows.Select(r => r.Tag).ToList();
        }
    }
}