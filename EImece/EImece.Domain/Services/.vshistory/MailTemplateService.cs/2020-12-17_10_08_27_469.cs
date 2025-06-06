﻿using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;

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
            cOrder.Customer = CustomerService.GetUserId("44a72377-7a04-49ec-b8bb-40b9140deddc");
            var pp = new OrderConfirmationEmailRazorTemplate();
            pp.CompanyAddress = SettingService.GetSettingObjectByKey(Constants.CompanyAddress).SettingValue.Trim();
            pp.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName).SettingValue.Trim();
            pp.CompanyEmailAddress = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyEmailAddress).SettingValue.Trim();
            pp.CompanyPhoneNumber = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyPhoneAndLocation).SettingValue.Trim();
            pp.FinishedOrder = cOrder;
            pp.CompanyWebSiteUrl = "";
            pp.ImgLogoSrc = "";
            return pp;
        }
    }
}