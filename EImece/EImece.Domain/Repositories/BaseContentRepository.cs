using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{

    public abstract class BaseContentRepository<T> : BaseEntityRepository<T> where T : BaseContent
    {
        public BaseContentRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }
    }
}
