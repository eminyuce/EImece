using EImece.Domain.Entities;
using EImece.Domain.Helpers.RazorCustomRssTemplate;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.FrontModels;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers.EmailHelper
{
    public interface IRazorEngineHelper
    {
        Tuple<string, string> ConfirmYourAccountEmailBody(string email, string name, string callbackUrl);

        Task<Tuple<string, string>> ConfirmYourAccountEmailBodyAsync(string email, string name, string callbackUrl);

        Tuple<string, string> ForgotPasswordEmailBody(string email, string callbackUrl);

        Task<Tuple<string, string>> ForgotPasswordEmailBodyAsync(string email, string callbackUrl);

        Tuple<string, RazorRenderResult, Customer> CompanyGotNewOrderEmail(int orderId);

        Task<Tuple<string, RazorRenderResult, Customer>> CompanyGotNewOrderEmailAsync(int orderId);

        Tuple<string, RazorRenderResult, Customer> OrderConfirmationEmail(int orderId);

        Task<Tuple<string, RazorRenderResult, Customer>> OrderConfirmationEmailAsync(int orderId);

        void SendMessageToSeller(ContactUsFormViewModel contact);

        Task SendMessageToSellerAsync(ContactUsFormViewModel contact);

        void SendContactUsAboutProductDetailEmail(ContactUsFormViewModel contact);

        Task SendContactUsAboutProductDetailEmailAsync(ContactUsFormViewModel contact);

        void SendContactUsForCommunication(ContactUsFormViewModel contact);

        Task SendContactUsForCommunicationAsync(ContactUsFormViewModel contact);

        string GenerateRssEmailTemplate(MailTemplate rssTemplate);

        RazorRenderResult GetRenderOutputByRazorEngineModel<T>(string razorTemplate, T razorEngineModel) where T : RazorTemplateModel;

        RazorRenderResult GetRenderOutput(string razorTemplate, RazorEngineModel razorEngineModel = null);
    }
}
