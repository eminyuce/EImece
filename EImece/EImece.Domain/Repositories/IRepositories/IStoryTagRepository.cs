using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryTagRepository : IBaseRepository<StoryTag>
    {
        List<StoryTag> GetStoryTagsByStoryId(int storyId);

        Task<List<StoryTag>> GetStoryTagsByStoryIdAsync(int storyId, CancellationToken cancellationToken = default(CancellationToken));

        void SaveStoryTags(int storyId, int[] tags);

        Task SaveStoryTagsAsync(int storyId, int[] tags);

        PaginatedList<StoryTag> GetStoriesByTagId(int tagId, int pageIndex, int pageSize, int lang);

        Task<PaginatedList<StoryTag>> GetStoriesByTagIdAsync(int tagId, int pageIndex, int pageSize, int lang, CancellationToken cancellationToken = default(CancellationToken));
    }
}