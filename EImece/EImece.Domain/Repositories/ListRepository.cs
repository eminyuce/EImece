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
    public class ListRepository : BaseEntityRepository<List>, IListRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ListRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<List> GetAllListItems()
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ListItems);
            return this.GetAllIncluding(includeProperties.ToArray()).ToList();
        }

        public List GetListById(int id)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ListItems);
            var item = GetSingleIncluding(id, includeProperties.ToArray());
            return item;
        }

        public List GetListByName(string name)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ListItems);
            var item = FindAllIncluding(r => r.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase), r => r.Position, OrderByType.Ascending, null, null, includeProperties.ToArray());
            return item.FirstOrDefault();
        }
    }
}