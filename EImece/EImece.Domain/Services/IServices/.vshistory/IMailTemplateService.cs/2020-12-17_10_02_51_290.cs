using EImece.Domain.Entities;

namespace EImece.Domain.Services.IServices
{
    public interface IMailTemplateService : IBaseEntityService<MailTemplate>
    {
        MailTemplate GetMailTemplateByName(string templatename);
          OrderConfirmationEmailRazorTemplate GenerateOrderConfirmationEmailRazorTemplate(int orderId)
    }
}