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
    public class TemplateRepository : BaseEntityRepository<Template>, ITemplateRepository
    {
        public TemplateRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<Template> GetAllActiveTemplates()
        {
            return GetAll().Where(t => t.IsActive).ToList();
        }

        public async Task<List<Template>> GetAllActiveTemplatesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAll().Where(t => t.IsActive).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public List<Template> GetAllTemplates()
        {
            return GetAll().ToList();
        }

        public string GetTemplateXml(int id)
        {
            return EImeceDbContext.Templates.AsNoTracking()
                .Where(t => t.Id == id && t.IsActive)
                .Select(t => t.TemplateXml)
                .FirstOrDefault();
        }

        public async Task<string> GetTemplateXmlAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Templates.AsNoTracking()
                .Where(t => t.Id == id && t.IsActive)
                .Select(t => t.TemplateXml)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}