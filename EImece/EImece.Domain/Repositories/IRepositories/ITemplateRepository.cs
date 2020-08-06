using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITemplateRepository : IBaseEntityRepository<Template>
    {
        List<Template> GetAllActiveTemplates();

        List<Template> GetAllTemplates();
    }
}