using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IListItemService : IBaseEntityService<ListItem>
    {
        void DeleteListItemByListId(int id);
        void SaveListItem(int listId, ICollection<ListItem> listItems);
    }
}
