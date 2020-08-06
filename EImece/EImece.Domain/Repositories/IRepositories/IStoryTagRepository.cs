using EImece.Domain.Entities;
using GenericRepository;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryTagRepository : IBaseRepository<StoryTag>
    {
        List<StoryTag> GetStoryTagsByStoryId(int storyId);

        void SaveStoryTags(int storyId, int[] tags);

        PaginatedList<StoryTag> GetStoriesByTagId(int tagId, int pageIndex, int pageSize, int lang);
    }
}