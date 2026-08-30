using EImece.Domain.ApiRepositories;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.DependencyInjection;
using NLog;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class MailTemplateRepository : BaseEntityRepository<MailTemplate>, IMailTemplateRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MailTemplateRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public MailTemplate GetMailTemplateByName(string templatename)
        {
            var item = GetAll().FirstOrDefault(r => r.Name.Equals(templatename));

            return item;
        }
    }
}