using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Models.Enums;

namespace EImece.Domain.Services.IServices
{
    public interface ITagCategoryService : IBaseEntityService<TagCategory>
    {
        List<TagCategory> GetTagsByTagType(EImeceLanguage language);
    }
}
