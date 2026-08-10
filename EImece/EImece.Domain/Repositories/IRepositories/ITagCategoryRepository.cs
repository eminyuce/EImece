using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagCategoryRepository : IBaseEntityRepository<TagCategory>
    {
        List<TagCategory> GetTagsByTagType(EImeceLanguage language);

        Task<List<TagCategory>> GetTagsByTagTypeAsync(EImeceLanguage language, CancellationToken cancellationToken = default(CancellationToken));

        TagCategory GetTagCategoryById(int tagCategoryId);
    }
}