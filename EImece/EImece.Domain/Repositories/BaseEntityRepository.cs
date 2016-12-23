using EImece.Domain.Entities;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;

namespace EImece.Domain.Repositories
{
    public abstract class BaseEntityRepository<T> : BaseRepository<T, int> where T : BaseEntity
    {
        public BaseEntityRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}
