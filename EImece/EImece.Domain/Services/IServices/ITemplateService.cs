using EImece.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ITemplateService : IBaseEntityService<Template>
    {
        /// <summary>Database read (Admin-safe). Not served from cache.</summary>
        Template GetTemplate(int id);

        /// <summary>Database read (Admin-safe). Not served from cache.</summary>
        Task<Template> GetTemplateAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        string GetTemplateXml(int id);
        Task<string> GetTemplateXmlAsync(int id, CancellationToken cancellationToken = default(CancellationToken));
    }
}