using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ListService : BaseEntityService<List>, IListService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IListItemRepository ListItemRepository;
        private readonly IListRepository ListRepository;

        public ListService(
            IListRepository repository,
            IEimeceCacheProvider dataCachingProvider,
            IListItemRepository listItemRepository)
            : base(repository, dataCachingProvider)
        {
            ListRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            ListItemRepository = listItemRepository ?? throw new ArgumentNullException(nameof(listItemRepository));
        }

        public List GetListById(int id)
        {
            return ListRepository.GetListById(id);
        }

        public async Task<List> GetListByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ListRepository.GetListByIdAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public void DeleteListById(int id)
        {
            List list = GetListById(id);
            ListItemRepository.DeleteByWhereCondition(r => r.ListId == id);
            DeleteEntity(list);
        }

        public async Task DeleteListByIdAsync(int id)
        {
            List list = await GetListByIdAsync(id).ConfigureAwait(false);
            await ListItemRepository.DeleteByWhereConditionAsync(r => r.ListId == id).ConfigureAwait(false);
            await DeleteEntityAsync(list).ConfigureAwait(false);
        }

        public List GetListByName(string name)
        {
            return ListRepository.GetListByName(name);
        }

        public List<List> GetListItems()
        {
            var cacheKey = String.Format("GetListItems");

            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => ListRepository.GetAllListItems(),
                AppConfig.CacheLongSeconds);
        }
    }
}