﻿using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void SaveListItem(int listId, ICollection<ListItem> listItems)
        {
            DeleteListItemByListId(listId);
            foreach (var item in listItems)
            {
                ListItemRepository.Add(item);
            }
            ListItemRepository.Save();
        }
    }
}