using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IListItemService : IBaseEntityService<ListItem>
    {
        void DeleteListItemByListId(int id);

        void SaveListItem(int listId, List<ListItem> listItems);
    }
}