using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class SubscriberRepository : BaseEntityRepository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }

        public Subscriber GetSubscriberByEmail(string email)
        {
            return this.FindBy(r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
    }
}