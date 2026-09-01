using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using Microsoft.Extensions.Logging;
namespace EImece.Domain.Repositories
{
    public class ListItemRepository : BaseEntityRepository<ListItem>, IListItemRepository
    {
        public ListItemRepository(IEImeceContext dbContext, ILogger<ListItemRepository> logger) : base(dbContext, logger)
        {
        }
    }
}