using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class TagRepository : BaseEntityRepository<Tag>, ITagRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        public async Task<List<StorefrontTagDto>> GetStorefrontProductTagsAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Tags.AsNoTracking()
                .Where(r => r.Lang == language && r.IsActive && r.TagCategory != null && r.TagCategory.IsActive)
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.Id)
                .Select(t => new StorefrontTagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    TagCategoryId = t.TagCategoryId,
                    TagCategoryName = t.TagCategory != null ? t.TagCategory.Name : string.Empty,
                    Position = t.Position,
                    Lang = t.Lang,
                    IsActive = t.IsActive
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontTagDto> GetStorefrontProductTags(int language)
        {
            return EImeceDbContext.Tags.AsNoTracking()
                .Where(r => r.Lang == language && r.IsActive && r.TagCategory != null && r.TagCategory.IsActive)
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.Id)
                .Select(t => new StorefrontTagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    TagCategoryId = t.TagCategoryId,
                    TagCategoryName = t.TagCategory != null ? t.TagCategory.Name : string.Empty,
                    Position = t.Position,
                    Lang = t.Lang,
                    IsActive = t.IsActive
                })
                .ToList();
        }

        public async Task<List<StorefrontTagDto>> GetStorefrontTagsWithStoryCountsAsync(int language, int minStoryCount = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Tags.AsNoTracking()
                .Where(t => t.IsActive && t.Lang == language)
                .Select(t => new
                {
                    Tag = t,
                    StoryCount = t.StoryTags.Count(st => st.Story != null && st.Story.IsActive)
                })
                .Where(x => x.StoryCount >= minStoryCount)
                .OrderByDescending(x => x.StoryCount)
                .ThenBy(x => x.Tag.Name)
                .Select(x => new StorefrontTagDto
                {
                    Id = x.Tag.Id,
                    Name = x.Tag.Name,
                    TagCategoryId = x.Tag.TagCategoryId,
                    TagCategoryName = x.Tag.TagCategory != null ? x.Tag.TagCategory.Name : string.Empty,
                    Position = x.Tag.Position,
                    Lang = x.Tag.Lang,
                    IsActive = x.Tag.IsActive,
                    ItemCount = x.StoryCount
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontTagDto> GetStorefrontTagsWithStoryCounts(int language, int minStoryCount = 1)
        {
            return EImeceDbContext.Tags.AsNoTracking()
                .Where(t => t.IsActive && t.Lang == language)
                .Select(t => new
                {
                    Tag = t,
                    StoryCount = t.StoryTags.Count(st => st.Story != null && st.Story.IsActive)
                })
                .Where(x => x.StoryCount >= minStoryCount)
                .OrderByDescending(x => x.StoryCount)
                .ThenBy(x => x.Tag.Name)
                .Select(x => new StorefrontTagDto
                {
                    Id = x.Tag.Id,
                    Name = x.Tag.Name,
                    TagCategoryId = x.Tag.TagCategoryId,
                    TagCategoryName = x.Tag.TagCategory != null ? x.Tag.TagCategory.Name : string.Empty,
                    Position = x.Tag.Position,
                    Lang = x.Tag.Lang,
                    IsActive = x.Tag.IsActive,
                    ItemCount = x.StoryCount
                })
                .ToList();
        }

        public async Task<List<StorefrontTagDto>> GetStorefrontTagsWithEntityCountsAsync(int language, int minEntityCount = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Tags.AsNoTracking()
                .Where(t => t.IsActive && t.Lang == language)
                .Select(t => new
                {
                    Tag = t,
                    EntityCount = t.ProductTags.Count(pt => pt.Product != null && pt.Product.IsActive)
                                + t.StoryTags.Count(st => st.Story != null && st.Story.IsActive)
                })
                .Where(x => x.EntityCount >= minEntityCount)
                .OrderBy(x => x.Tag.Position)
                .ThenByDescending(x => x.Tag.Id)
                .Select(x => new StorefrontTagDto
                {
                    Id = x.Tag.Id,
                    Name = x.Tag.Name,
                    TagCategoryId = x.Tag.TagCategoryId,
                    TagCategoryName = x.Tag.TagCategory != null ? x.Tag.TagCategory.Name : string.Empty,
                    Position = x.Tag.Position,
                    Lang = x.Tag.Lang,
                    IsActive = x.Tag.IsActive,
                    ItemCount = x.EntityCount
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontTagDto> GetStorefrontTagsWithEntityCounts(int language, int minEntityCount = 1)
        {
            return EImeceDbContext.Tags.AsNoTracking()
                .Where(t => t.IsActive && t.Lang == language)
                .Select(t => new
                {
                    Tag = t,
                    EntityCount = t.ProductTags.Count(pt => pt.Product != null && pt.Product.IsActive)
                                + t.StoryTags.Count(st => st.Story != null && st.Story.IsActive)
                })
                .Where(x => x.EntityCount >= minEntityCount)
                .OrderBy(x => x.Tag.Position)
                .ThenByDescending(x => x.Tag.Id)
                .Select(x => new StorefrontTagDto
                {
                    Id = x.Tag.Id,
                    Name = x.Tag.Name,
                    TagCategoryId = x.Tag.TagCategoryId,
                    TagCategoryName = x.Tag.TagCategory != null ? x.Tag.TagCategory.Name : string.Empty,
                    Position = x.Tag.Position,
                    Lang = x.Tag.Lang,
                    IsActive = x.Tag.IsActive,
                    ItemCount = x.EntityCount
                })
                .ToList();
        }

        public async Task<StorefrontTagDto> GetStorefrontTagByIdAsync(int tagId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Tags.AsNoTracking()
                .Where(t => t.Id == tagId && t.IsActive)
                .Select(t => new StorefrontTagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    TagCategoryId = t.TagCategoryId,
                    TagCategoryName = t.TagCategory != null ? t.TagCategory.Name : string.Empty,
                    Position = t.Position,
                    Lang = t.Lang,
                    IsActive = t.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontTagDto GetStorefrontTagById(int tagId)
        {
            return EImeceDbContext.Tags.AsNoTracking()
                .Where(t => t.Id == tagId && t.IsActive)
                .Select(t => new StorefrontTagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    TagCategoryId = t.TagCategoryId,
                    TagCategoryName = t.TagCategory != null ? t.TagCategory.Name : string.Empty,
                    Position = t.Position,
                    Lang = t.Lang,
                    IsActive = t.IsActive
                })
                .FirstOrDefault();
        }

        #endregion

        public List<Tag> GetAdminPageList(string search, int language)
        {
            Expression<Func<Tag, object>> includeProperty2 = r => r.TagCategory;
            Expression<Func<Tag, object>>[] includeProperties = { includeProperty2 };
            var tags = GetAllIncluding(includeProperties).Where(r => r.Lang == language);
            if (!String.IsNullOrEmpty(search))
            {
                tags = tags.Where(r => r.Name.ToLower().Contains(search.Trim().ToLower()));
            }
            var result = tags.OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToList();

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
            var result = await tags.OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id).ToListAsync().ConfigureAwait(false);

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

        public async Task<List<Tag>> GetProductTagsAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            Expression<Func<Tag, object>>[] includeProperties = {
                r => r.ProductTags,
                r => r.TagCategory
            };

            var tags = GetAllIncluding(includeProperties)
                .Where(r => r.Lang == language && r.IsActive && r.TagCategory.IsActive);

            return await tags
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
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

        public async Task<List<Tag>> GetTagsWithStoryCountsAsync(int language, int minStoryCount = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            var rows = await GetAll()
                .Where(t => t.IsActive && t.Lang == language)
                .Select(t => new
                {
                    Tag = t,
                    StoryCount = t.StoryTags.Count(st => st.Story != null && st.Story.IsActive)
                })
                .Where(x => x.StoryCount >= minStoryCount)
                .OrderByDescending(x => x.StoryCount)
                .ThenBy(x => x.Tag.Name)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var row in rows)
            {
                row.Tag.ItemCount = row.StoryCount;
            }

            return rows.Select(r => r.Tag).ToList();
        }
    }
}