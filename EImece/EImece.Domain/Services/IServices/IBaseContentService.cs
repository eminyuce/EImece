using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IBaseContentService<T> : IBaseEntityService<T> where T : BaseContent
    {
        List<T> GetActiveBaseContents(bool? isActive, int language);
        new void DeleteBaseEntity(List<string> values);
        T GetBaseContent(int id);
    }
}
