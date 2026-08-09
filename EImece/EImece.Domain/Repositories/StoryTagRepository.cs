using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
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

        public void DeleteStoryTags(int storyId)
        {
            var storyTags = GetAll().Where(r => r.StoryId == storyId).ToList();
            foreach (var story in storyTags)
            {
                Delete(story);
            }
            Save();
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

        public PaginatedList<StoryTag> GetStoriesByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.Tag);
            includeProperties.Add(r => r.Story);
            includeProperties.Add(r => r.Story.StoryCategory);
            includeProperties.Add(r => r.Story.MainImage);
            includeProperties.Add(r => r.Story.StoryFiles);
            // Do not Include Story.StoryTags here: with AsNoTracking(), EF throws
            // "RelatedEnd ... StoryTag_Story_Target has already been loaded" when the
            // nested StoryTags try to wire Story back onto already-loaded Story entities.
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
            includeProperties.Add(r => r.Story.StoryFiles);
            return await this.PaginateAsync(pageIndex,
                pageSize,
                r => r.Story.Position,
                r => r.TagId == tagId && r.Tag.Lang == lang && r.Story.IsActive,
                cancellationToken,
                includeProperties.ToArray()).ConfigureAwait(false);
        }
    }
}