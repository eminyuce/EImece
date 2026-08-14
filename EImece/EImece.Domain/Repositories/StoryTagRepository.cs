using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class StoryTagRepository : BaseRepository<StoryTag>, IStoryTagRepository
    {
        public StoryTagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<StoryTag> GetStoryTagsByStoryId(int storyId)
        {
            return this.GetAll().Where(r => r.StoryId == storyId).ToList();
        }

        public async Task<List<StoryTag>> GetStoryTagsByStoryIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.GetAll().Where(r => r.StoryId == storyId).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public void DeleteStoryTags(int storyId)
        {
            var storyTags = GetAll().Where(r => r.StoryId == storyId).ToList();
            foreach (var story in storyTags)
            {
                Delete(story);
            }
            Save();
        }

        public async Task DeleteStoryTagsAsync(int storyId)
        {
            var storyTags = await GetAll().Where(r => r.StoryId == storyId).ToListAsync().ConfigureAwait(false);
            foreach (var story in storyTags)
            {
                Delete(story);
            }
            await SaveAsync().ConfigureAwait(false);
        }

        public void SaveStoryTags(int storyId, int[] tags)
        {
            DeleteStoryTags(storyId);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    StoryTag item = new StoryTag();
                    item.StoryId = storyId;
                    item.TagId = tag;
                    this.Add(item);
                }
                Save();
            }
        }

        public async Task SaveStoryTagsAsync(int storyId, int[] tags)
        {
            await DeleteStoryTagsAsync(storyId).ConfigureAwait(false);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    StoryTag item = new StoryTag();
                    item.StoryId = storyId;
                    item.TagId = tag;
                    this.Add(item);
                }
                await SaveAsync().ConfigureAwait(false);
            }
        }

        public PaginatedList<StoryTag> GetStoriesByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Tag);
            includeProperties.Add(r => r.Story);
            includeProperties.Add(r => r.Story.StoryCategory);
            includeProperties.Add(r => r.Story.MainImage);
            return this.Paginate(pageIndex,
                pageSize,
                r => r.Story.Position,
                r => r.TagId == tagId && r.Tag.Lang == lang && r.Story.IsActive,
                includeProperties.ToArray());
        }

        public async Task<PaginatedList<StoryTag>> GetStoriesByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Tag);
            includeProperties.Add(r => r.Story);
            includeProperties.Add(r => r.Story.StoryCategory);
            includeProperties.Add(r => r.Story.MainImage);
            return await this.PaginateAsync(pageIndex,
                pageSize,
                r => r.Story.Position,
                r => r.TagId == tagId && r.Tag.Lang == lang && r.Story.IsActive,
                cancellationToken,
                includeProperties.ToArray()).ConfigureAwait(false);
        }
    }
}