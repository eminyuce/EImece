using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Observability.Telemetry;
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
    public class StoryRepository : BaseContentRepository<Story>, IStoryRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IEImeceContext dbContext;

        public StoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
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

        public async Task<List<Story>> GetAdminPageListAsync(int categoryId, string search, int lang)
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

            return await stories.ToListAsync().ConfigureAwait(false);
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

        public async Task<List<Story>> GetFeaturedStoriesAsync(int take, int language, int excludedStoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.StoryTags.Select(r1 => r1.Tag));
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.StoryCategory);
            Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.Lang == language && r2.IsFeaturedStory && r2.Id != excludedStoryId;
            Expression<Func<Story, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Ascending, take, 0, includeProperties.ToArray());

            return await result.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Story GetNextStory(int currentStoryId, int language)
        {
            var currentStory = dbContext.Stories.FirstOrDefault(s => s.Id == currentStoryId && s.Lang == language);
            if (currentStory == null) return null;

            // Match listing order: Position ASC, UpdatedDate DESC
            return dbContext.Stories
                .Include(s => s.StoryCategory)
                .Include(s => s.MainImage)
                .Where(s => s.Id != currentStoryId &&
                            s.Lang == language &&
                            s.IsActive &&
                            s.StoryCategoryId == currentStory.StoryCategoryId &&
                            (s.Position > currentStory.Position ||
                            (s.Position == currentStory.Position && s.UpdatedDate < currentStory.UpdatedDate)))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .FirstOrDefault();
        }

        public async Task<Story> GetNextStoryAsync(int currentStoryId, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentStory = await dbContext.Stories.FirstOrDefaultAsync(s => s.Id == currentStoryId && s.Lang == language, cancellationToken).ConfigureAwait(false);
            if (currentStory == null) return null;

            // Match listing order: Position ASC, UpdatedDate DESC
            return await dbContext.Stories
                .Include(s => s.StoryCategory)
                .Include(s => s.MainImage)
                .Where(s => s.Id != currentStoryId &&
                            s.Lang == language &&
                            s.IsActive &&
                            s.StoryCategoryId == currentStory.StoryCategoryId &&
                            (s.Position > currentStory.Position ||
                            (s.Position == currentStory.Position && s.UpdatedDate < currentStory.UpdatedDate)))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public Story GetPreviousStory(int currentStoryId, int language)
        {
            var currentStory = dbContext.Stories.FirstOrDefault(s => s.Id == currentStoryId && s.Lang == language);
            if (currentStory == null) return null;

            // Reverse of listing order so the first result is the adjacent previous story
            return dbContext.Stories
                .Include(s => s.StoryCategory)
                .Include(s => s.MainImage)
                .Where(s => s.Id != currentStoryId &&
                            s.Lang == language &&
                            s.IsActive &&
                            s.StoryCategoryId == currentStory.StoryCategoryId &&
                            (s.Position < currentStory.Position ||
                            (s.Position == currentStory.Position && s.UpdatedDate > currentStory.UpdatedDate)))
                .OrderByDescending(s => s.Position)
                .ThenBy(s => s.UpdatedDate)
                .FirstOrDefault();
        }

        public async Task<Story> GetPreviousStoryAsync(int currentStoryId, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentStory = await dbContext.Stories.FirstOrDefaultAsync(s => s.Id == currentStoryId && s.Lang == language, cancellationToken).ConfigureAwait(false);
            if (currentStory == null) return null;

            // Reverse of listing order so the first result is the adjacent previous story
            return await dbContext.Stories
                .Include(s => s.StoryCategory)
                .Include(s => s.MainImage)
                .Where(s => s.Id != currentStoryId &&
                            s.Lang == language &&
                            s.IsActive &&
                            s.StoryCategoryId == currentStory.StoryCategoryId &&
                            (s.Position < currentStory.Position ||
                            (s.Position == currentStory.Position && s.UpdatedDate > currentStory.UpdatedDate)))
                .OrderByDescending(s => s.Position)
                .ThenBy(s => s.UpdatedDate)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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

        public async Task<PaginatedList<Story>> GetMainPageStoriesAsync(int page, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken))
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
                return await this.PaginateDescendingAsync(page, pageSize, keySelector, match, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GetMainPageStoriesAsync failed.");
                throw new InvalidOperationException("GetMainPageStoriesAsync failed.", exception);
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

        public async Task<List<Story>> GetRelatedStoriesAsync(int[] tagIdList, int take, int lang, int excludedStoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.StoryTags.Select(r1 => r1.Tag));
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.StoryCategory);
            Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.Lang == lang && r2.StoryTags.Any(t => tagIdList.Contains(t.TagId)) && r2.Id != excludedStoryId;
            Expression<Func<Story, int>> keySelector = t => t.Position;
            var result = FindAllIncluding(match, keySelector, OrderByType.Ascending, take, 0, includeProperties.ToArray());

            return await result.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public PaginatedList<Story> GetStoriesByStoryCategoryId(int storyCategoryId, int language, int pageIndex, int pageSize)
        {
            try
            {
                var includeProperties = GetIncludePropertyExpressionList();
                includeProperties.Add(r => r.StoryCategory);
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

        public async Task<PaginatedList<Story>> GetStoriesByStoryCategoryIdAsync(int storyCategoryId, int language, int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var includeProperties = GetIncludePropertyExpressionList();
                includeProperties.Add(r => r.StoryCategory);
                includeProperties.Add(r => r.MainImage);
                includeProperties.Add(r => r.StoryFiles);
                includeProperties.Add(r => r.StoryTags.Select(q => q.Tag));
                Expression<Func<Story, bool>> match = r2 => r2.IsActive && r2.StoryCategoryId == storyCategoryId && r2.MainPage && r2.Lang == language;
                Expression<Func<Story, int>> keySelector = t => t.Position;
                return await this.PaginateDescendingAsync(pageIndex, pageSize, keySelector, match, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GetStoriesByStoryCategoryIdAsync failed.");
                throw new InvalidOperationException("GetStoriesByStoryCategoryIdAsync failed.", exception);
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

        public async Task<Story> GetStoryByIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.StoryCategory);
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.StoryFiles.Select(t => t.FileStorage));
            includeProperties.Add(r => r.StoryTags.Select(q => q.Tag));
            return await GetSingleIncludingAsync(storyId, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);
        }

        #region Storefront Read Implementations (LINQ Projection, AsNoTracking, Main Entity Activation)

        private static Expression<Func<Story, StorefrontStoryCardDto>> StoryCardProjection
        {
            get
            {
                return s => new StorefrontStoryCardDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ShortDescription = s.ShortDescription,
                    StoryCategoryId = s.StoryCategoryId,
                    StoryCategoryName = s.StoryCategory != null ? s.StoryCategory.Name : string.Empty,
                    MainImageId = s.MainImageId,
                    Position = s.Position,
                    Lang = s.Lang,
                    IsActive = s.IsActive,
                    MainPage = s.MainPage,
                    IsFeaturedStory = s.IsFeaturedStory,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate,
                    AuthorName = s.AuthorName
                };
            }
        }

        [Timed("repo.story.get_detail")]

        public virtual async Task<StorefrontStoryDetailDto> GetStorefrontStoryDetailByIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.Id == storyId && s.IsActive)
                .Select(s => new StorefrontStoryDetailDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ShortDescription = s.ShortDescription,
                    Description = s.Description,
                    MetaKeywords = s.MetaKeywords,
                    StoryCategoryId = s.StoryCategoryId,
                    StoryCategoryName = s.StoryCategory != null ? s.StoryCategory.Name : string.Empty,
                    MainImageId = s.MainImageId,
                    Position = s.Position,
                    Lang = s.Lang,
                    IsActive = s.IsActive,
                    MainPage = s.MainPage,
                    IsFeaturedStory = s.IsFeaturedStory,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate,
                    StoryFiles = s.StoryFiles
                        .Where(sf => sf.FileStorage != null && sf.FileStorage.IsActive)
                        .OrderBy(sf => sf.Position)
                        .Select(sf => new StorefrontProductFileDto
                        {
                            Id = sf.Id,
                            ProductId = sf.StoryId,
                            FileStorageId = sf.FileStorageId,
                            FileName = sf.FileStorage.FileName,
                            Title = sf.FileStorage.Name,
                            Description = sf.FileStorage.FileName,
                            Width = sf.FileStorage.Width,
                            Height = sf.FileStorage.Height,
                            Position = sf.Position,
                            IsActive = sf.FileStorage.IsActive
                        }).ToList(),
                    StoryTags = s.StoryTags
                        .Where(st => st.Tag != null && st.Tag.IsActive)
                        .OrderBy(st => st.Tag.Position)
                        .Select(st => new StorefrontTagDto
                        {
                            Id = st.Tag.Id,
                            Name = st.Tag.Name,
                            TagCategoryId = st.Tag.TagCategoryId,
                            TagCategoryName = st.Tag.TagCategory != null ? st.Tag.TagCategory.Name : string.Empty,
                            Position = st.Tag.Position,
                            Lang = st.Tag.Lang,
                            IsActive = st.Tag.IsActive
                        }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.story.get_detail_sync")]
        public virtual StorefrontStoryDetailDto GetStorefrontStoryDetailById(int storyId)
        {
            return EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.Id == storyId && s.IsActive)
                .Select(s => new StorefrontStoryDetailDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ShortDescription = s.ShortDescription,
                    Description = s.Description,
                    MetaKeywords = s.MetaKeywords,
                    StoryCategoryId = s.StoryCategoryId,
                    StoryCategoryName = s.StoryCategory != null ? s.StoryCategory.Name : string.Empty,
                    MainImageId = s.MainImageId,
                    Position = s.Position,
                    Lang = s.Lang,
                    IsActive = s.IsActive,
                    MainPage = s.MainPage,
                    IsFeaturedStory = s.IsFeaturedStory,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate,
                    StoryFiles = s.StoryFiles
                        .Where(sf => sf.FileStorage != null && sf.FileStorage.IsActive)
                        .OrderBy(sf => sf.Position)
                        .Select(sf => new StorefrontProductFileDto
                        {
                            Id = sf.Id,
                            ProductId = sf.StoryId,
                            FileStorageId = sf.FileStorageId,
                            FileName = sf.FileStorage.FileName,
                            Title = sf.FileStorage.Name,
                            Description = sf.FileStorage.FileName,
                            Width = sf.FileStorage.Width,
                            Height = sf.FileStorage.Height,
                            Position = sf.Position,
                            IsActive = sf.FileStorage.IsActive
                        }).ToList(),
                    StoryTags = s.StoryTags
                        .Where(st => st.Tag != null && st.Tag.IsActive)
                        .OrderBy(st => st.Tag.Position)
                        .Select(st => new StorefrontTagDto
                        {
                            Id = st.Tag.Id,
                            Name = st.Tag.Name,
                            TagCategoryId = st.Tag.TagCategoryId,
                            TagCategoryName = st.Tag.TagCategory != null ? st.Tag.TagCategory.Name : string.Empty,
                            Position = st.Tag.Position,
                            Lang = st.Tag.Lang,
                            IsActive = st.Tag.IsActive
                        }).ToList()
                })
                .FirstOrDefault();
        }

        [Timed("repo.story.get_featured")]

        public virtual async Task<List<StorefrontStoryCardDto>> GetStorefrontFeaturedStoriesAsync(int take, int language, int excludedStoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.Lang == language && s.IsFeaturedStory && s.Id != excludedStoryId &&
                            (s.StoryCategory == null || s.StoryCategory.IsActive))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.story.get_featured_sync")]
        public virtual List<StorefrontStoryCardDto> GetStorefrontFeaturedStories(int take, int language, int excludedStoryId)
        {
            return EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.Lang == language && s.IsFeaturedStory && s.Id != excludedStoryId &&
                            (s.StoryCategory == null || s.StoryCategory.IsActive))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .Take(take)
                .ToList();
        }

        [Timed("repo.story.get_latest", "Time taken to get storefront latest stories from DB")]
        public virtual async Task<List<StorefrontStoryCardDto>> GetStorefrontLatestStoriesAsync(int take, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.MainPage && s.Lang == language &&
                            (s.StoryCategory == null || s.StoryCategory.IsActive))
                .OrderByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.story.get_latest_sync")]
        public virtual List<StorefrontStoryCardDto> GetStorefrontLatestStories(int take, int language)
        {
            return EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.MainPage && s.Lang == language &&
                            (s.StoryCategory == null || s.StoryCategory.IsActive))
                .OrderByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .Take(take)
                .ToList();
        }

        [Timed("repo.story.get_main_page")]

        public virtual async Task<PaginatedList<StorefrontStoryCardDto>> GetStorefrontMainPageStoriesAsync(int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.MainPage && s.Lang == language &&
                            (s.StoryCategory == null || s.StoryCategory.IsActive))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection);

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedList<StorefrontStoryCardDto>(items, pageIndex, pageSize, total);
        }

        [Timed("repo.story.get_main_page_sync")]
        public virtual PaginatedList<StorefrontStoryCardDto> GetStorefrontMainPageStories(int pageIndex, int pageSize, int language)
        {
            var query = EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.MainPage && s.Lang == language &&
                            (s.StoryCategory == null || s.StoryCategory.IsActive))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection);

            var total = query.Count();
            var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<StorefrontStoryCardDto>(items, pageIndex, pageSize, total);
        }

        [Timed("repo.story.get_by_category", "Time taken to get storefront stories by category from DB")]
        public virtual async Task<PaginatedList<StorefrontStoryCardDto>> GetStorefrontStoriesByCategoryIdAsync(int storyCategoryId, int language, int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.StoryCategoryId == storyCategoryId && s.Lang == language &&
                            (s.StoryCategory == null || s.StoryCategory.IsActive))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection);

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedList<StorefrontStoryCardDto>(items, pageIndex, pageSize, total);
        }

        [Timed("repo.story.get_by_category_sync")]
        public virtual PaginatedList<StorefrontStoryCardDto> GetStorefrontStoriesByCategoryId(int storyCategoryId, int language, int pageIndex, int pageSize)
        {
            var query = EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.StoryCategoryId == storyCategoryId && s.Lang == language &&
                            (s.StoryCategory == null || s.StoryCategory.IsActive))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection);

            var total = query.Count();
            var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<StorefrontStoryCardDto>(items, pageIndex, pageSize, total);
        }

        public async Task<PaginatedList<StorefrontStoryCardDto>> GetStorefrontStoriesByTagIdAsync(int tagId, int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.Lang == language &&
                            s.StoryTags.Any(st => st.TagId == tagId && st.Tag != null && st.Tag.IsActive))
                .OrderBy(s => s.Position);

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(StoryCardProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return new PaginatedList<StorefrontStoryCardDto>(items, pageIndex, pageSize, total);
        }

        public PaginatedList<StorefrontStoryCardDto> GetStorefrontStoriesByTagId(int tagId, int pageIndex, int pageSize, int language)
        {
            var query = EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.Lang == language &&
                            s.StoryTags.Any(st => st.TagId == tagId && st.Tag != null && st.Tag.IsActive))
                .OrderBy(s => s.Position);

            var total = query.Count();
            var items = query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(StoryCardProjection)
                .ToList();
            return new PaginatedList<StorefrontStoryCardDto>(items, pageIndex, pageSize, total);
        }

        [Timed("repo.story.get_related")]

        public virtual async Task<List<StorefrontStoryCardDto>> GetStorefrontRelatedStoriesAsync(int[] tagIdList, int take, int language, int excludedStoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tagIdList == null || tagIdList.Length == 0) return new List<StorefrontStoryCardDto>();

            return await EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.Lang == language && s.Id != excludedStoryId &&
                            s.StoryTags.Any(st => st.Tag != null && st.Tag.IsActive && tagIdList.Contains(st.TagId)))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .Take(take)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        [Timed("repo.story.get_related_sync")]
        public virtual List<StorefrontStoryCardDto> GetStorefrontRelatedStories(int[] tagIdList, int take, int language, int excludedStoryId)
        {
            if (tagIdList == null || tagIdList.Length == 0) return new List<StorefrontStoryCardDto>();

            return EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.IsActive && s.Lang == language && s.Id != excludedStoryId &&
                            s.StoryTags.Any(st => st.Tag != null && st.Tag.IsActive && tagIdList.Contains(st.TagId)))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .Take(take)
                .ToList();
        }

        public async Task<StorefrontStoryCardDto> GetStorefrontNextStoryAsync(int currentStoryId, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentStory = await EImeceDbContext.Stories.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == currentStoryId && s.Lang == language, cancellationToken)
                .ConfigureAwait(false);
            if (currentStory == null) return null;

            return await EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.Id != currentStoryId &&
                            s.Lang == language &&
                            s.IsActive &&
                            s.StoryCategoryId == currentStory.StoryCategoryId &&
                            (s.Position > currentStory.Position ||
                            (s.Position == currentStory.Position && s.UpdatedDate < currentStory.UpdatedDate)))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontStoryCardDto GetStorefrontNextStory(int currentStoryId, int language)
        {
            var currentStory = EImeceDbContext.Stories.AsNoTracking()
                .FirstOrDefault(s => s.Id == currentStoryId && s.Lang == language);
            if (currentStory == null) return null;

            return EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.Id != currentStoryId &&
                            s.Lang == language &&
                            s.IsActive &&
                            s.StoryCategoryId == currentStory.StoryCategoryId &&
                            (s.Position > currentStory.Position ||
                            (s.Position == currentStory.Position && s.UpdatedDate < currentStory.UpdatedDate)))
                .OrderBy(s => s.Position)
                .ThenByDescending(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .FirstOrDefault();
        }

        public async Task<StorefrontStoryCardDto> GetStorefrontPreviousStoryAsync(int currentStoryId, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentStory = await EImeceDbContext.Stories.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == currentStoryId && s.Lang == language, cancellationToken)
                .ConfigureAwait(false);
            if (currentStory == null) return null;

            return await EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.Id != currentStoryId &&
                            s.Lang == language &&
                            s.IsActive &&
                            s.StoryCategoryId == currentStory.StoryCategoryId &&
                            (s.Position < currentStory.Position ||
                            (s.Position == currentStory.Position && s.UpdatedDate > currentStory.UpdatedDate)))
                .OrderByDescending(s => s.Position)
                .ThenBy(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontStoryCardDto GetStorefrontPreviousStory(int currentStoryId, int language)
        {
            var currentStory = EImeceDbContext.Stories.AsNoTracking()
                .FirstOrDefault(s => s.Id == currentStoryId && s.Lang == language);
            if (currentStory == null) return null;

            return EImeceDbContext.Stories.AsNoTracking()
                .Where(s => s.Id != currentStoryId &&
                            s.Lang == language &&
                            s.IsActive &&
                            s.StoryCategoryId == currentStory.StoryCategoryId &&
                            (s.Position < currentStory.Position ||
                            (s.Position == currentStory.Position && s.UpdatedDate > currentStory.UpdatedDate)))
                .OrderByDescending(s => s.Position)
                .ThenBy(s => s.UpdatedDate)
                .Select(StoryCardProjection)
                .FirstOrDefault();
        }

        #endregion
    }
}
