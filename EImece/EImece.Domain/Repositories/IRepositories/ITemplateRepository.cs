using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITemplateRepository : IBaseEntityRepository<Template>, IDisposable
    {
        List<Template> GetAllActiveTemplates();

        List<Template> GetAllTemplates();
    }
}