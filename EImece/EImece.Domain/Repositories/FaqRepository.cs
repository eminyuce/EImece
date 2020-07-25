using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class FaqRepository : BaseEntityRepository<Faq>, IFaqRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public FaqRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }
    }
}
