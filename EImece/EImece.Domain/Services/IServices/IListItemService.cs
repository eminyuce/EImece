using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IListItemService : IBaseEntityService<ListItem>
    {
        void DeleteListItemByListId(int id);

        Task DeleteListItemByListIdAsync(int id);

        void SaveListItem(int listId, List<ListItem> listItems);

        Task SaveListItemAsync(int listId, List<ListItem> listItems);
    }
}