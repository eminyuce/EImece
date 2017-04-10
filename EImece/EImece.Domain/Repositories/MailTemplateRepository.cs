using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using NLog;

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
            return GetAll().FirstOrDefault(r => r.Name.Equals(templatename));
        }
    }
}

