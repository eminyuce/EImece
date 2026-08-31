using Microsoft.Extensions.Logging;
﻿using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ListItemService : BaseEntityService<ListItem>, IListItemService
    {
        private IListItemRepository ListItemRepository { get; set; }

        public ListItemService(IListItemRepository repository, ILogger<ListItemService> logger) : base(repository, logger) {
            ListItemRepository = repository;
        }

        public void DeleteListItemByListId(int id)
        {
            ListItemRepository.DeleteByWhereCondition(r => r.ListId == id);
        }

        public async Task DeleteListItemByListIdAsync(int id)
        {
            await ListItemRepository.DeleteByWhereConditionAsync(r => r.ListId == id).ConfigureAwait(false);
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

        public async Task SaveListItemAsync(int listId, List<ListItem> listItems)
        {
            await DeleteListItemByListIdAsync(listId).ConfigureAwait(false);
            foreach (var item in listItems)
            {
                item.ListId = listId;
                item.CreatedDate = DateTime.Now;
                item.UpdatedDate = DateTime.Now;
                await ListItemRepository.SaveOrEditAsync(item).ConfigureAwait(false);
            }
        }
    }
}