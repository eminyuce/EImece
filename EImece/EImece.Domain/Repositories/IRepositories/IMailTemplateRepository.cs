using EImece.Domain.Entities;
using System;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMailTemplateRepository : IBaseEntityRepository<MailTemplate>, IDisposable
    {
        MailTemplate GetMailTemplateByName(string templatename);
    }
}