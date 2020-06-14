using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Repositories
{
    public class StoryFileRepository : BaseEntityRepository<StoryFile>, IStoryFileRepository
    {
        public StoryFileRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}