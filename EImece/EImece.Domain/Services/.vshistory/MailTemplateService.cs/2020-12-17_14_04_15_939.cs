using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Web;

namespace EImece.Domain.Services
{
    public class MailTemplateService : BaseEntityService<MailTemplate>, IMailTemplateService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IMailTemplateRepository MailTemplateRepository { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public ISettingService SettingService { get; set; }

        public MailTemplateService(IMailTemplateRepository repository) : base(repository)
        {
            MailTemplateRepository = repository;
        }

        public MailTemplate GetMailTemplateByName(string templatename)
        {
            return MailTemplateRepository.GetMailTemplateByName(templatename);
        }

        public OrderConfirmationEmailRazorTemplate GenerateOrderConfirmationEmailRazorTemplate(int orderId)
        {
            var cOrder = OrderService.GetOrderById(orderId);
            var pp = new OrderConfirmationEmailRazorTemplate();
            pp.CompanyAddress = SettingService.GetSettingObjectByKey(Constants.CompanyAddress).SettingValue.Trim();
            pp.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName).SettingValue.Trim();
            pp.CompanyEmailAddress = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyEmailAddress).SettingValue.Trim();
            pp.CompanyPhoneNumber = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyPhoneAndLocation).SettingValue.Trim();
            pp.FinishedOrder = cOrder;
            pp.OrderProducts = cOrder.OrderProducts.ToList();
            var builder = new UriBuilder(HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port);
            var url = builder.Uri.ToString().TrimEnd('/');
            pp.CompanyWebSiteUrl = url;
            pp.ImgLogoSrc = url + Constants.LogoImagePath;
            return pp;
        }
    }
}