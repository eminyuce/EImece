using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IProductCommentService : IBaseEntityService<ProductComment>
    {
        List<ProductComment> GetAdminPageList(int? productId, string search, int lang, IList<int> ratings = null, DateTime? startDate = null, DateTime? endDate = null);

        Task<List<ProductComment>> GetAdminPageListAsync(int? productId, string search, int lang, IList<int> ratings = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
