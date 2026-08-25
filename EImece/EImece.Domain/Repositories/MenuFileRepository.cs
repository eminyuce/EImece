using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class MenuFileRepository : BaseEntityRepository<MenuFile>, IMenuFileRepository
    {
        public MenuFileRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<MenuFile>> GetMenuFilesForImageExportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.MenuFiles.AsNoTracking().Include(mf => mf.Menu).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public List<MenuFile> GetMenuFilesForImageExport()
        {
            return EImeceDbContext.MenuFiles.AsNoTracking().Include(mf => mf.Menu).ToList();
        }
    }
}