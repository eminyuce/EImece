using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IProductCommentService : IBaseEntityService<ProductComment>
    {
        List<ProductComment> GetAdminPageList(int productId, string search, int lang);
    }
}