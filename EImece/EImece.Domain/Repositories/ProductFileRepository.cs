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
    public class ProductFileRepository : BaseEntityRepository<ProductFile>, IProductFileRepository
    {
        public ProductFileRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<ProductFile>> GetProductFilesForImageExportAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ProductFiles.AsNoTracking().Include(pf => pf.Product).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public List<ProductFile> GetProductFilesForImageExport()
        {
            return EImeceDbContext.ProductFiles.AsNoTracking().Include(pf => pf.Product).ToList();
        }
    }
}