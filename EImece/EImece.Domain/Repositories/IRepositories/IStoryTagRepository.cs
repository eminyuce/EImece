using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericRepository;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryTagRepository : IBaseRepository<StoryTag>, IDisposable
    {
        List<StoryTag> GetStoryTagsByStoryId(int storyId);
        void SaveStoryTags(int storyId, int[] tags);
        PaginatedList<StoryTag> GetStoriesByTagId(int tagId, int pageIndex, int pageSize, int lang);
    }
}
