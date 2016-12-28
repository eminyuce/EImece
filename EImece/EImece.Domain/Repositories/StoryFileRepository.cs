using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.GenericRepositories;

namespace EImece.Domain.Repositories
{
    public class StoryFileRepository : BaseEntityRepository<StoryFile>, IStoryFileRepository
    {
        public StoryFileRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        
    }
}
