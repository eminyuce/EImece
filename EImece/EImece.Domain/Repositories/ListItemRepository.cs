using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;

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