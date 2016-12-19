using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Models.Enums;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagCategoryRepository : IBaseRepository<TagCategory, int>, IDisposable
    {
        List<TagCategory> GetTagsByTagType(EImeceTagType products, EImeceLanguage language);
    }
}
