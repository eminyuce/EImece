using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Repositories.IRepositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class ListRepository : BaseEntityRepository<List>, IListRepository
    {
        public ListRepository(IEImeceContext dbContext, ILogger<ListRepository> logger) : base(dbContext, logger)
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

        public async Task<List> GetListByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ListItems);
            return await GetSingleIncludingAsync(id, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);
        }

        public List GetListByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ListItems);
            // EF6 cannot translate StringComparison overloads; compare lowercased strings in SQL instead.
            var nameLower = name.Trim().ToLower();
            var item = FindAllIncluding(
                r => r.Name != null && r.Name.ToLower() == nameLower,
                r => r.Position,
                OrderByType.Ascending,
                null,
                null,
                includeProperties.ToArray());
            return item.FirstOrDefault();
        }
    }
}