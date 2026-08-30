using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using EImece.Domain.Factories.IFactories;

namespace EImece.Domain.Services
{
    public class MailTemplateService : BaseEntityService<MailTemplate>, IMailTemplateService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IMailTemplateRepository MailTemplateRepository;
        private readonly IOrderService OrderService;
        private readonly ISettingService SettingService;
        private readonly IHttpContextFactory HttpContextFactory;

        public MailTemplateService(
            IMailTemplateRepository repository,
            IEimeceCacheProvider dataCachingProvider,
            IOrderService orderService,
            ISettingService settingService,
            IHttpContextFactory httpContextFactory)
            : base(repository, dataCachingProvider)
        {
            MailTemplateRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            OrderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            SettingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            HttpContextFactory = httpContextFactory ?? throw new ArgumentNullException(nameof(httpContextFactory));
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
                () => this.GetAll(),
                AppConfig.CacheLongSeconds);
        }

        [Timed("service.mail_template.get_all_with_cache")]
        public virtual async Task<List<MailTemplate>> GetAllMailTemplatesWithCacheAsync()
        {
            var cacheKey = "GetAllMailTemplatesWithCache" + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => this.GetAllAsync(),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
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
            // FIX: injected abstraction instead of static HttpContext.Current.
            var mailRequest = HttpContextFactory.Create().Request;
            var builder = new UriBuilder(AppConfig.HttpProtocol, mailRequest.Url.Host, mailRequest.Url.Port);
            var url = builder.Uri.ToString().TrimEnd('/');
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
            var mailRequest = HttpContextFactory.Create().Request;
            var builder = new UriBuilder(AppConfig.HttpProtocol, mailRequest.Url.Host, mailRequest.Url.Port);
            var url = builder.Uri.ToString().TrimEnd('/');
            pp.CompanyWebSiteUrl = url;
            pp.BaseUrl = baseurl;
            pp.AdminPanelUrl = baseurl + "/account/adminlogin/";
            pp.ImgLogoSrc = baseurl + Constants.LogoImagePath;
            return pp;
        }

        private string GetSiteBaseUrl()
        {
            // FIX: injected abstraction instead of static HttpContext.Current.
            var Request = HttpContextFactory.Create().Request;
            var baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
            return baseurl;
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
            // FIX: injected abstraction instead of static HttpContext.Current.
            var confirmRequest = HttpContextFactory.Create().Request;
            var builder = new UriBuilder(AppConfig.HttpProtocol, confirmRequest.Url.Host, confirmRequest.Url.Port);
            var url = builder.Uri.ToString().TrimEnd('/');
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
            var confirmRequest = HttpContextFactory.Create().Request;
            var builder = new UriBuilder(AppConfig.HttpProtocol, confirmRequest.Url.Host, confirmRequest.Url.Port);
            var url = builder.Uri.ToString().TrimEnd('/');
            string baseurl = GetSiteBaseUrl();
            pp.CompanyWebSiteUrl = url;
            pp.BaseUrl = baseurl;
            pp.ImgLogoSrc = baseurl + Constants.LogoImagePath;
            return pp;
        }
    }
}