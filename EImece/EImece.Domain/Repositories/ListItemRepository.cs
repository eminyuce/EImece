using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;

namespace EImece.Domain.Repositories
{
    public class ListItemRepository : BaseEntityRepository<ListItem>, IListItemRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ListItemRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }
    }
}
