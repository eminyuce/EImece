using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Repositories
{
    public class MenuFileRepository : BaseEntityRepository<MenuFile>, IMenuFileRepository
    {
        public MenuFileRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}