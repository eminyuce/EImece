using EImece.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ITemplateService : IBaseEntityService<Template>
    {
        Template GetTemplate(int id);

        Task<Template> GetTemplateAsync(int id, CancellationToken cancellationToken = default(CancellationToken));
    }
}