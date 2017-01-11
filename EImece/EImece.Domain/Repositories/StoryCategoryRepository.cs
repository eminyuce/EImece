using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;

namespace EImece.Domain.Repositories
{
    public class StoryCategoryRepository : BaseContentRepository<StoryCategory>, IStoryCategoryRepository
    {
        public StoryCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

         
       
    }
}
