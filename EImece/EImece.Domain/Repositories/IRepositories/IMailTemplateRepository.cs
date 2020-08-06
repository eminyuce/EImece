using EImece.Domain.Entities;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMailTemplateRepository : IBaseEntityRepository<MailTemplate>
    {
        MailTemplate GetMailTemplateByName(string templatename);
    }
}