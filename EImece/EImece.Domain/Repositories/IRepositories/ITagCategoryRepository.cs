using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Models.Enums;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagCategoryRepository : IBaseEntityRepository<TagCategory>, IDisposable
    {
        List<TagCategory> GetTagsByTagType(EImeceLanguage language);
    }
}
