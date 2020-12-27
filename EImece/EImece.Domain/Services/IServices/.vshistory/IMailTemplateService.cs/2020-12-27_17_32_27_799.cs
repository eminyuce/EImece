using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;

namespace EImece.Domain.Services.IServices
{
    public interface IMailTemplateService : IBaseEntityService<MailTemplate>
    {
        MailTemplate GetMailTemplateByName(string templatename);
        OrderConfirmationEmailRazorTemplate GenerateCompanyGotNewOrderEmailRazorTemplate(int orderId);
    }
}