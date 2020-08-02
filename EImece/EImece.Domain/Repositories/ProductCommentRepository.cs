using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository.EntityFramework.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class ProductCommentRepository : BaseEntityRepository<ProductComment >, IProductCommentRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProductCommentRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}
