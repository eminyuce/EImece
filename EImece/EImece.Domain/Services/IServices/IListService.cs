using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IListService : IBaseEntityService<List>
    {
        void DeleteListById(int id);
        List GetListById(int id);
    }
}
