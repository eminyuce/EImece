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
    public class ListRepository : BaseEntityRepository<List>, IListRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ListRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }

        public List GetListById(int id)
        {
            var includeProperties = GetIncludePropertyExpressionList<List>();
            includeProperties.Add(r => r.ListItems);
            var item = GetSingleIncluding(id, includeProperties.ToArray());
            return item;
        }
    }
}
