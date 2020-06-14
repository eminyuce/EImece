using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface ITagCategoryService : IBaseEntityService<TagCategory>
    {
        List<TagCategory> GetTagsByTagType(EImeceLanguage language);

        void DeleteTagCategoryById(int tagCategoryId);

        TagCategory GetTagCategoryById(int tagCategoryId);
    }
}