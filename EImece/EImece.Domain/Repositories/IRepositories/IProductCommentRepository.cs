using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductCommentRepository : IBaseEntityRepository<ProductComment>
    {
        List<ProductComment> GetAdminPageList(int productId, string search, int lang);

        Task<List<ProductComment>> GetAdminPageListAsync(int productId, string search, int lang, CancellationToken cancellationToken = default(CancellationToken));
    }
}