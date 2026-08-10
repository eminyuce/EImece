using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ITagCategoryService : IBaseEntityService<TagCategory>
    {
        List<TagCategory> GetTagsByTagType(EImeceLanguage language);

        Task<List<TagCategory>> GetTagsByTagTypeAsync(EImeceLanguage language, CancellationToken cancellationToken = default(CancellationToken));

        void DeleteTagCategoryById(int tagCategoryId);

        Task DeleteTagCategoryByIdAsync(int tagCategoryId);

        TagCategory GetTagCategoryById(int tagCategoryId);
    }
}