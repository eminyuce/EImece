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
    public class StoryTagRepository : BaseRepository<StoryTag>, IStoryTagRepository
    {
        public StoryTagRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
       
      

     
    }
}
