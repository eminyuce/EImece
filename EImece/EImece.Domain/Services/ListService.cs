using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Services
{
    public class ListService : BaseEntityService<List>, IListService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IListItemRepository ListItemRepository { get; set; }

        private IListRepository ListRepository { get; set; }

        public ListService(IListRepository repository) : base(repository)
        {
            ListRepository = repository;
        }

        public List GetListById(int id)
        {
            return ListRepository.GetListById(id);
        }

        public void DeleteListById(int id)
        {
            List list = GetListById(id);
            ListItemRepository.DeleteByWhereCondition(r => r.ListId == id);
            DeleteEntity(list);
        }

        public List GetListByName(string name)
        {
            return ListRepository.GetListByName(name);
        }

        public List<List> GetListItems()
        {
            List<List> result = null;
            var cacheKey = String.Format("GetListItems");

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new List<List>();
                result = ListRepository.GetAllListItems();
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheLongSeconds);
            }
            return result;
        }
    }
}