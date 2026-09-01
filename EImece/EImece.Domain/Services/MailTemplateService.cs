using EImece.Domain.Abstractions;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class MailTemplateService : BaseEntityService<MailTemplate>, IMailTemplateService
    {
        private readonly IMailTemplateRepository MailTemplateRepository;
        private readonly IOrderService OrderService;
        private readonly ISettingService SettingService;
        private readonly ISiteUrlProvider SiteUrlProvider;

        public MailTemplateService(IMailTemplateRepository repository,
            IEimeceCacheProvider dataCachingProvider,
            IOrderService orderService,
            ISettingService settingService,
            ISiteUrlProvider siteUrlProvider, ILogger<MailTemplateService> logger)
            : base(repository, dataCachingProvider, logger)
        {
            MailTemplateRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            OrderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            SettingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            SiteUrlProvider = siteUrlProvider ?? throw new ArgumentNullException(nameof(siteUrlProvider));
        }

        [Timed("service.mail_template.get_by_name_sync")]
        public virtual MailTemplate GetMailTemplateByName(string templatename)
        {
            return GetAllMailTemplatesWithCache().FirstOrDefault(r => r.Name.Equals(templatename, StringComparison.InvariantCultureIgnoreCase));
        }

        [Timed("service.mail_template.get_by_name")]
        public virtual async Task<MailTemplate> GetMailTemplateByNameAsync(string templatename)
        {
            var templates = await GetAllMailTemplatesWithCacheAsync().ConfigureAwait(false);
            return templates.FirstOrDefault(r => r.Name.Equals(templatename, StringComparison.InvariantCultureIgnoreCase));
        }

        [Timed("service.mail_template.get_all_with_cache_sync")]
        public virtual List<MailTemplate> GetAllMailTemplatesWithCache()
        {
            var cacheKey = "GetAllMailTemplatesWithCache";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MailTemplateRepository.GetAll().ToList(),
                AppConfig.CacheVeryLongSeconds);
        }

        [Timed("service.mail_template.get_all_with_cache")]
        public virtual async Task<List<MailTemplate>> GetAllMailTemplatesWithCacheAsync()
        {
            var cacheKey = "GetAllMailTemplatesWithCache";
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                async () => await MailTemplateRepository.GetAll().ToListAsync().ConfigureAwait(false),
                AppConfig.CacheVeryLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.mail_template.generate_company_new_order_sync")]
        public virtual CompanyGotNewOrderEmailRazorTemplate GenerateCompanyGotNewOrderEmailRazorTemplate(int orderId)
        {
            var cOrder = OrderService.GetOrderById(orderId);
            var pp = new CompanyGotNewOrderEmailRazorTemplate();

            pp.CompanyAddress = SettingService.GetSettingObjectByKey(Constants.CompanyAddress).SettingValue.Trim();
            pp.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName).SettingValue.Trim();
            pp.CompanyEmailAddress = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyEmailAddress).SettingValue.Trim();
            pp.CompanyPhoneNumber = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyPhoneAndLocation).SettingValue.Trim();
            pp.FinishedOrder = cOrder;
            pp.OrderProducts = cOrder.OrderProducts.ToList();
            string baseurl = GetSiteBaseUrl();
            var url = SiteUrlProvider.GetSiteDomainUrl();
            pp.CompanyWebSiteUrl = url;
            pp.BaseUrl = baseurl;
            pp.AdminPanelUrl = baseurl + "/account/adminlogin/";
            pp.ImgLogoSrc = baseurl + Constants.LogoImagePath;
            return pp;
        }

        [Timed("service.mail_template.generate_company_new_order")]
        public virtual async Task<CompanyGotNewOrderEmailRazorTemplate> GenerateCompanyGotNewOrderEmailRazorTemplateAsync(int orderId)
        {
            var cOrder = await OrderService.GetOrderByIdAsync(orderId).ConfigureAwait(false);
            var pp = new CompanyGotNewOrderEmailRazorTemplate();

            pp.CompanyAddress = (await SettingService.GetSettingObjectByKeyAsync(Constants.CompanyAddress).ConfigureAwait(false)).SettingValue.Trim();
            pp.CompanyName = (await SettingService.GetSettingObjectByKeyAsync(Constants.CompanyName).ConfigureAwait(false)).SettingValue.Trim();
            pp.CompanyEmailAddress = (await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteCompanyEmailAddress).ConfigureAwait(false)).SettingValue.Trim();
            pp.CompanyPhoneNumber = (await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteCompanyPhoneAndLocation).ConfigureAwait(false)).SettingValue.Trim();
            pp.FinishedOrder = cOrder;
            pp.OrderProducts = cOrder.OrderProducts.ToList();
            string baseurl = GetSiteBaseUrl();
            var url = SiteUrlProvider.GetSiteDomainUrl();
            pp.CompanyWebSiteUrl = url;
            pp.BaseUrl = baseurl;
            pp.AdminPanelUrl = baseurl + "/account/adminlogin/";
            pp.ImgLogoSrc = baseurl + Constants.LogoImagePath;
            return pp;
        }

        private string GetSiteBaseUrl()
        {
            return SiteUrlProvider.GetSiteBaseUrl();
        }

        [Timed("service.mail_template.generate_order_confirmation_sync")]
        public virtual OrderConfirmationEmailRazorTemplate GenerateOrderConfirmationEmailRazorTemplate(int orderId)
        {
            var cOrder = OrderService.GetOrderById(orderId);
            var pp = new OrderConfirmationEmailRazorTemplate();

            pp.CompanyAddress = SettingService.GetSettingObjectByKey(Constants.CompanyAddress).SettingValue.Trim();
            pp.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName).SettingValue.Trim();
            pp.CompanyEmailAddress = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyEmailAddress).SettingValue.Trim();
            pp.CompanyPhoneNumber = SettingService.GetSettingObjectByKey(Constants.WebSiteCompanyPhoneAndLocation).SettingValue.Trim();
            pp.FinishedOrder = cOrder;
            pp.OrderProducts = cOrder.OrderProducts.ToList();
            var url = SiteUrlProvider.GetSiteDomainUrl();
            string baseurl = GetSiteBaseUrl();
            pp.CompanyWebSiteUrl = url;
            pp.BaseUrl = baseurl;
            pp.ImgLogoSrc = baseurl + Constants.LogoImagePath;
            return pp;
        }

        [Timed("service.mail_template.generate_order_confirmation")]
        public virtual async Task<OrderConfirmationEmailRazorTemplate> GenerateOrderConfirmationEmailRazorTemplateAsync(int orderId)
        {
            var cOrder = await OrderService.GetOrderByIdAsync(orderId).ConfigureAwait(false);
            var pp = new OrderConfirmationEmailRazorTemplate();

            pp.CompanyAddress = (await SettingService.GetSettingObjectByKeyAsync(Constants.CompanyAddress).ConfigureAwait(false)).SettingValue.Trim();
            pp.CompanyName = (await SettingService.GetSettingObjectByKeyAsync(Constants.CompanyName).ConfigureAwait(false)).SettingValue.Trim();
            pp.CompanyEmailAddress = (await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteCompanyEmailAddress).ConfigureAwait(false)).SettingValue.Trim();
            pp.CompanyPhoneNumber = (await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteCompanyPhoneAndLocation).ConfigureAwait(false)).SettingValue.Trim();
            pp.FinishedOrder = cOrder;
            pp.OrderProducts = cOrder.OrderProducts.ToList();
            var url = SiteUrlProvider.GetSiteDomainUrl();
            string baseurl = GetSiteBaseUrl();
            pp.CompanyWebSiteUrl = url;
            pp.BaseUrl = baseurl;
            pp.ImgLogoSrc = baseurl + Constants.LogoImagePath;
            return pp;
        }
    }
}