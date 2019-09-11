using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITemplateRepository : IBaseEntityRepository<Template>, IDisposable
    {
        List<Template> GetAllActiveTemplates();
        List<Template> GetAllTemplates();

    }
}
