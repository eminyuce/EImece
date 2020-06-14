using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Repositories
{
    public class SubscriberRepository : BaseEntityRepository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}