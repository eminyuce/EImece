using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;

namespace EImece.Domain.Repositories
{
    public class MainPageImageRepository : BaseContentRepository<MainPageImage>, IMainPageImageRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MainPageImageRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}