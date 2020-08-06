using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagCategoryRepository : IBaseEntityRepository<TagCategory>
    {
        List<TagCategory> GetTagsByTagType(EImeceLanguage language);

        TagCategory GetTagCategoryById(int tagCategoryId);
    }
}