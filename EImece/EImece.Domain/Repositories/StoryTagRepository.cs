using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;

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
    }
}
