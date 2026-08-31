using Microsoft.Extensions.Logging;
﻿using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
namespace EImece.Domain.Repositories
{
    public class ListItemRepository : BaseEntityRepository<ListItem>, IListItemRepository
    {
        public ListItemRepository(IEImeceContext dbContext, ILogger<ListItemRepository> logger) : base(dbContext, logger) {
        }
    }
}