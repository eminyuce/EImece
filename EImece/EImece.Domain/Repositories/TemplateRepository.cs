using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
using System.Linq;

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

        public List<Template> GetAllTemplates()
        {
            return GetAll().ToList();
        }
    }
}