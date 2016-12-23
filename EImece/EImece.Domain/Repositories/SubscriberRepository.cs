using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.GenericRepositories;

namespace EImece.Domain.Repositories
{
    public class SubscriberRepository : BaseEntityRepository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

      
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


  
        public List<Subscriber> GetActiveBaseEntities(bool? isActive)
        {
            return BaseEntityRepository.GetActiveBaseEntities(this, isActive);
        }
    }
}
