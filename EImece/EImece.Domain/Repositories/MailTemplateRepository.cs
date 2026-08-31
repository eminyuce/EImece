using Microsoft.Extensions.Logging;
using EImece.Domain.ApiRepositories;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.DependencyInjection;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class MailTemplateRepository : BaseEntityRepository<MailTemplate>, IMailTemplateRepository
    {
        public MailTemplateRepository(IEImeceContext dbContext, ILogger<MailTemplateRepository> logger) : base(dbContext, logger) {
        }

        public MailTemplate GetMailTemplateByName(string templatename)
        {
            var item = GetAll().FirstOrDefault(r => r.Name.Equals(templatename));

            return item;
        }
    }
}