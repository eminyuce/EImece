using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductCommentRepository : IBaseEntityRepository<ProductComment>
    {
        List<ProductComment> GetAdminPageList(int productId, string search, int lang);
    }
}