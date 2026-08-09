using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITemplateRepository : IBaseEntityRepository<Template>
    {
        List<Template> GetAllActiveTemplates();

        Task<List<Template>> GetAllActiveTemplatesAsync(CancellationToken cancellationToken = default(CancellationToken));

        List<Template> GetAllTemplates();
    }
}