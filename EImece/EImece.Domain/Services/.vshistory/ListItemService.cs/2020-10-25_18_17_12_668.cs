using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Services
{
    public class ListItemService : BaseEntityService<ListItem>, IListItemService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IListItemRepository ListItemRepository { get; set; }

        public ListItemService(IListItemRepository repository) : base(repository)
        {
            ListItemRepository = repository;
        }
    

        public void DeleteListItemByListId(int id)
        {
            ListItemRepository.DeleteByWhereCondition(r => r.ListId == id);
        }

        public void SaveListItem(int listId, List<ListItem> listItems)
        {
            DeleteListItemByListId(listId);
            foreach (var item in listItems)
            {
                item.ListId = listId;
                item.CreatedDate = DateTime.Now;
                item.UpdatedDate = DateTime.Now;
                ListItemRepository.SaveOrEdit(item);
            }
        }
    }
}