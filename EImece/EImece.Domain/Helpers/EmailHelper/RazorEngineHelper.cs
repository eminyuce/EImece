using EImece.Domain.Abstractions;
using EImece.Domain.ApiRepositories;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Services.IServices;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers.EmailHelper
{
    public class RazorEngineHelper : IRazorEngineHelper
    {
        private readonly IMailTemplateService _mailTemplateService;
        private readonly ISettingService _settingService;
        private readonly ISiteUrlProvider _siteUrlProvider;
        private readonly IEmailSender _emailSender;
        private readonly BitlyRepository _bitlyRepository;
        private readonly IRazorTemplateEngine _razorTemplateEngine;

        public IMailTemplateService MailTemplateService => _mailTemplateService;
        public ISettingService SettingService => _settingService;
        public ISiteUrlProvider SiteUrlProvider => _siteUrlProvider;
        public IEmailSender EmailSender => _emailSender;
        public BitlyRepository BitlyRepository => _bitlyRepository;
        public IRazorTemplateEngine RazorTemplateEngine => _razorTemplateEngine;

        public RazorEngineHelper(
            IMailTemplateService mailTemplateService,
            ISettingService settingService,
            ISiteUrlProvider siteUrlProvider,
            IEmailSender emailSender,
            BitlyRepository bitlyRepository,
            IRazorTemplateEngine razorTemplateEngine)
        {
            _mailTemplateService = mailTemplateService ?? throw new ArgumentNullException(nameof(mailTemplateService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _siteUrlProvider = siteUrlProvider ?? throw new ArgumentNullException(nameof(siteUrlProvider));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _bitlyRepository = bitlyRepository ?? throw new ArgumentNullException(nameof(bitlyRepository));
            _razorTemplateEngine = razorTemplateEngine ?? throw new ArgumentNullException(nameof(razorTemplateEngine));
        }

        [Timed("service.email.confirm_account_sync")]
        public virtual Tuple<string, string> ConfirmYourAccountEmailBody(string email, string name, string callbackUrl)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.ConfirmYourAccountMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.ConfirmYourAccountMailTemplate}");
            }

            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);

            string baseurl = GetSiteBaseUrl();

            var model = new
            {
                WebSiteIconUrl = baseurl + Constants.LogoImagePath,
                Email = email,
                callbackUrl = callbackUrl,
                Name = name,
                companyname = companyname
            };

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string result = Engine.Razor.RunCompile(template, templateKey, null, model);

            return new Tuple<string, string>(emailTemplate.Subject, result);
        }

        [Timed("service.email.confirm_account", "Time taken to render confirmation email body")]
        public virtual async Task<Tuple<string, string>> ConfirmYourAccountEmailBodyAsync(string email, string name, string callbackUrl)
        {
            // Capture before awaits — ConfigureAwait(false) clears HttpContext.Current.
            string baseurl = GetSiteBaseUrl();

            MailTemplate emailTemplate = await MailTemplateService.GetMailTemplateByNameAsync(Constants.ConfirmYourAccountMailTemplate).ConfigureAwait(false);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.ConfirmYourAccountMailTemplate}");
            }

            String companyname = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);

            var model = new
            {
                WebSiteIconUrl = baseurl + Constants.LogoImagePath,
                Email = email,
                callbackUrl = callbackUrl,
                Name = name,
                companyname = companyname
            };

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string result = Engine.Razor.RunCompile(template, templateKey, null, model);

            return new Tuple<string, string>(emailTemplate.Subject, result);
        }

        private string GetSiteBaseUrl()
        {
            var baseUrl = _siteUrlProvider?.GetSiteBaseUrl();
            if (!string.IsNullOrEmpty(baseUrl))
            {
                return baseUrl;
            }

            baseUrl = EntityExtension.GetAbsoluteApplicationBaseUrl();
            if (!string.IsNullOrEmpty(baseUrl))
            {
                return baseUrl;
            }

            var scheme = string.IsNullOrEmpty(AppConfig.HttpProtocol) ? "http" : AppConfig.HttpProtocol;
            var domain = string.IsNullOrEmpty(AppConfig.Domain) ? "localhost" : AppConfig.Domain.TrimEnd('/');
            return $"{scheme}://{domain}";
        }

        [Timed("service.email.forgot_password_sync")]
        public virtual Tuple<string, string> ForgotPasswordEmailBody(string email, string callbackUrl)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.ForgotPasswordMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.ForgotPasswordMailTemplate}");
            }

            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);

            string baseurl = GetSiteBaseUrl();
            var model = new
            {
                WebSiteIconUrl = baseurl + Constants.LogoImagePath,
                Email = email,
                ForgotPasswordLink = callbackUrl,
                CompanyName = companyname,
                CompanyPageLink = baseurl
            };

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string result = Engine.Razor.RunCompile(template, templateKey, null, model);

            return new Tuple<string, string>(emailTemplate.Subject, result);
        }

        [Timed("service.email.forgot_password", "Time taken to render forgot password email body")]
        public virtual async Task<Tuple<string, string>> ForgotPasswordEmailBodyAsync(string email, string callbackUrl)
        {
            string baseurl = GetSiteBaseUrl();

            MailTemplate emailTemplate = await MailTemplateService.GetMailTemplateByNameAsync(Constants.ForgotPasswordMailTemplate).ConfigureAwait(false);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.ForgotPasswordMailTemplate}");
            }

            String companyname = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);

            var model = new
            {
                WebSiteIconUrl = baseurl + Constants.LogoImagePath,
                Email = email,
                ForgotPasswordLink = callbackUrl,
                CompanyName = companyname,
                CompanyPageLink = baseurl
            };

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string result = Engine.Razor.RunCompile(template, templateKey, null, model);

            return new Tuple<string, string>(emailTemplate.Subject, result);
        }

        [Timed("service.email.company_new_order_sync")]
        public virtual Tuple<string, RazorRenderResult, Customer> CompanyGotNewOrderEmail(int orderId)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.CompanyGotNewOrderEmailMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.CompanyGotNewOrderEmailMailTemplate}");
            }

            // Mail template modelini hazırlıyoruz
            CompanyGotNewOrderEmailRazorTemplate model = MailTemplateService.GenerateCompanyGotNewOrderEmailRazorTemplate(orderId);

            // Konu başlığını dinamik olarak ayarlıyoruz
            var modelSubject = new
            {
                OrderNumber = model.FinishedOrder.OrderNumber
            };

            // Şablonun kendisi
            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(template);

            // RazorEngine kullanarak template'i render ediyoruz
            var result = GetRenderOutputByRazorEngineModel(template, model);

            // Konu başlığını Razor ile render ediyoruz
            string subject = Engine.Razor.RunCompile(emailTemplate.Subject, templateKey, null, modelSubject);

            // Sonuç olarak: Konu, şablonun render edilmiş sonucu ve müşteri bilgisi döndürülüyor
            return new Tuple<string, RazorRenderResult, Customer>(subject, result, model.FinishedOrder.Customer);
        }

        [Timed("service.email.company_new_order", "Time taken to render company new order email")]
        public virtual async Task<Tuple<string, RazorRenderResult, Customer>> CompanyGotNewOrderEmailAsync(int orderId)
        {
            MailTemplate emailTemplate = await MailTemplateService.GetMailTemplateByNameAsync(Constants.CompanyGotNewOrderEmailMailTemplate).ConfigureAwait(false);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.CompanyGotNewOrderEmailMailTemplate}");
            }

            CompanyGotNewOrderEmailRazorTemplate model = await MailTemplateService.GenerateCompanyGotNewOrderEmailRazorTemplateAsync(orderId).ConfigureAwait(false);

            var modelSubject = new
            {
                OrderNumber = model.FinishedOrder.OrderNumber
            };

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(template);

            var result = GetRenderOutputByRazorEngineModel(template, model);

            string subject = Engine.Razor.RunCompile(emailTemplate.Subject, templateKey, null, modelSubject);

            return new Tuple<string, RazorRenderResult, Customer>(subject, result, model.FinishedOrder.Customer);
        }

        [Timed("service.email.order_confirmation_sync")]
        public virtual Tuple<string, RazorRenderResult, Customer> OrderConfirmationEmail(int orderId)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.OrderConfirmationEmailMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.OrderConfirmationEmailMailTemplate}");
            }

            OrderConfirmationEmailRazorTemplate model = MailTemplateService.GenerateOrderConfirmationEmailRazorTemplate(orderId);
            string template = emailTemplate.Body;
            var result = GetRenderOutputByRazorEngineModel(template, model);
            return new Tuple<string, RazorRenderResult, Customer>(emailTemplate.Subject, result, model.FinishedOrder.Customer);
        }

        [Timed("service.email.order_confirmation", "Time taken to render order confirmation email")]
        public virtual async Task<Tuple<string, RazorRenderResult, Customer>> OrderConfirmationEmailAsync(int orderId)
        {
            MailTemplate emailTemplate = await MailTemplateService.GetMailTemplateByNameAsync(Constants.OrderConfirmationEmailMailTemplate).ConfigureAwait(false);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.OrderConfirmationEmailMailTemplate}");
            }

            OrderConfirmationEmailRazorTemplate model = await MailTemplateService.GenerateOrderConfirmationEmailRazorTemplateAsync(orderId).ConfigureAwait(false);
            string template = emailTemplate.Body;
            var result = GetRenderOutputByRazorEngineModel(template, model);
            return new Tuple<string, RazorRenderResult, Customer>(emailTemplate.Subject, result, model.FinishedOrder.Customer);
        }

        [Timed("service.email.send_message_to_seller_sync")]
        public virtual void SendMessageToSeller(ContactUsFormViewModel contact)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.SendMessageToSellerMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException("NO email template is defined for " + Constants.SendMessageToSellerMailTemplate);
            }

            if (emailTemplate.TrackWithBitly || emailTemplate.TrackWithMlnk)
            {
                string groupName = string.Format("{0} | {1}", "SendMessageToSeller", emailTemplate.Name);
                emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk, emailTemplate.Body, emailTemplate.Name, groupName);
            }

            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);
            var adminUserName = SettingService.GetSettingByKey(Constants.AdminUserName);

            string template = emailTemplate.Body;
            var subjectResult = RazorTemplateEngine.GetRenderOutputDynamic(emailTemplate.Subject, contact);
            var bodyResult = RazorTemplateEngine.GetRenderOutputDynamic(template, contact);

            string subject = !string.IsNullOrEmpty(subjectResult.Result) ? subjectResult.Result : emailTemplate.Subject;
            string body = !string.IsNullOrEmpty(bodyResult.Result) ? bodyResult.Result : template;

            EmailSender.SendEmailInBackground(SettingService.GetEmailAccount(),
                subject,
                body,
                adminUserName,
                companyname,
                adminUserName,
                companyname);
        }

        [Timed("service.email.send_message_to_seller", "Time taken to prepare and send message to seller email")]
        public virtual async Task SendMessageToSellerAsync(ContactUsFormViewModel contact)
        {
            MailTemplate emailTemplate = await MailTemplateService.GetMailTemplateByNameAsync(Constants.SendMessageToSellerMailTemplate).ConfigureAwait(false);
            if (emailTemplate == null)
            {
                throw new ArgumentException("NO email template is defined for " + Constants.SendMessageToSellerMailTemplate);
            }

            if (emailTemplate.TrackWithBitly || emailTemplate.TrackWithMlnk)
            {
                string groupName = string.Format("{0} | {1}", "SendMessageToSeller", emailTemplate.Name);
                emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk, emailTemplate.Body, emailTemplate.Name, groupName);
            }

            string companyname = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            var adminUserName = await SettingService.GetSettingByKeyAsync(Constants.AdminUserName).ConfigureAwait(false);

            string template = emailTemplate.Body;
            var subjectResult = RazorTemplateEngine.GetRenderOutputDynamic(emailTemplate.Subject, contact);
            var bodyResult = RazorTemplateEngine.GetRenderOutputDynamic(template, contact);

            string subject = !string.IsNullOrEmpty(subjectResult.Result) ? subjectResult.Result : emailTemplate.Subject;
            string body = !string.IsNullOrEmpty(bodyResult.Result) ? bodyResult.Result : template;

            // SendEmailInBackground queues the SMTP work; await only the settings/template I/O above.
            EmailSender.SendEmailInBackground(await SettingService.GetEmailAccountAsync().ConfigureAwait(false),
                subject,
                body,
                adminUserName,
                companyname,
                adminUserName,
                companyname);
        }

        [Timed("service.email.send_contact_us_product_detail_sync")]
        public virtual void SendContactUsAboutProductDetailEmail(ContactUsFormViewModel contact)
        {
            // E-posta şablonunu al
            var emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.ContactUsAboutProductInfoMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.ContactUsAboutProductInfoMailTemplate}");
            }

            // E-posta Takibi için Güncelleme
            string groupName = $"ContactUsFormViewModel | {emailTemplate.Name} | {DateTime.Now:yyyy-MM-dd HH:mm}";
            emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(
                emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk,
                emailTemplate.Body, emailTemplate.Name, groupName
            );

            // Şirket ve Yönetici bilgileri
            string companyName = SettingService.GetSettingByKey(Constants.CompanyName);
            string adminUserName = SettingService.GetSettingByKey(Constants.AdminUserName);

            // Web sitesi URL'sini al
            string baseurl = GetSiteBaseUrl();

            // Razor Template için model oluştur
            var model = new
            {
                ContactUs = contact,
                CompanyName = companyName,
                BaseUrl = baseurl,
                ProductPageLink = baseurl,
                WebSiteIconUrl = $"{baseurl}/images/logo.jpg"
            };

            // Şablonu işle
            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(emailTemplate.Body);
            string body = Engine.Razor.RunCompile(emailTemplate.Body, templateKey, null, model);

            // E-posta gönder
            EmailSender.SendEmailInBackground(
                SettingService.GetEmailAccount(),
                emailTemplate.Subject,
                body,
                adminUserName,
                companyName,
                adminUserName,
                companyName
            );
        }

        [Timed("service.email.send_contact_us_product_detail", "Time taken to prepare and send product detail inquiry email")]
        public virtual async Task SendContactUsAboutProductDetailEmailAsync(ContactUsFormViewModel contact)
        {
            var emailTemplate = await MailTemplateService.GetMailTemplateByNameAsync(Constants.ContactUsAboutProductInfoMailTemplate).ConfigureAwait(false);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.ContactUsAboutProductInfoMailTemplate}");
            }

            string groupName = $"ContactUsFormViewModel | {emailTemplate.Name} | {DateTime.Now:yyyy-MM-dd HH:mm}";
            emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(
                emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk,
                emailTemplate.Body, emailTemplate.Name, groupName
            );

            string companyName = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            string adminUserName = await SettingService.GetSettingByKeyAsync(Constants.AdminUserName).ConfigureAwait(false);

            string baseurl = GetSiteBaseUrl();

            var model = new
            {
                ContactUs = contact,
                CompanyName = companyName,
                BaseUrl = baseurl,
                ProductPageLink = baseurl,
                WebSiteIconUrl = $"{baseurl}/images/logo.jpg"
            };

            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(emailTemplate.Body);
            string body = Engine.Razor.RunCompile(emailTemplate.Body, templateKey, null, model);

            EmailSender.SendEmailInBackground(
                await SettingService.GetEmailAccountAsync().ConfigureAwait(false),
                emailTemplate.Subject,
                body,
                adminUserName,
                companyName,
                adminUserName,
                companyName
            );
        }

        [Timed("service.email.send_contact_us_communication_sync")]
        public virtual void SendContactUsForCommunication(ContactUsFormViewModel contact)
        {
            // E-posta şablonunu al
            var emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.ContactUsForCommunication);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.ContactUsForCommunication}");
            }

            // E-posta Takibi için Güncelleme
            string groupName = $"ContactUsForCommunication | {emailTemplate.Name} | {DateTime.Now:yyyy-MM-dd HH:mm}";
            emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(
                emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk,
                emailTemplate.Body, emailTemplate.Name, groupName
            );

            // Şirket ve Yönetici bilgileri
            string companyName = SettingService.GetSettingByKey(Constants.CompanyName);
            string adminUserName = SettingService.GetSettingByKey(Constants.AdminUserName);
            string webSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);

            // Web sitesi URL'sini al
            string baseurl = GetSiteBaseUrl();

            // Razor Template için model oluştur
            var model = new
            {
                ContactUs = contact,
                CompanyName = companyName,
                BaseUrl = baseurl,
                WebSiteIconUrl = $"{baseurl}/images/logo.jpg",
                AdminPageUrl = $"{baseurl}/account/adminlogin/"
            };

            // Şablonu işle
            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(emailTemplate.Body);
            string body = Engine.Razor.RunCompile(emailTemplate.Body, templateKey, null, model);

            // E-posta gönder
            EmailSender.SendEmailInBackground(
                SettingService.GetEmailAccount(),
                emailTemplate.Subject,
                body,
                adminUserName,
                companyName,
                webSiteCompanyEmailAddress,
                companyName
            );
        }

        [Timed("service.email.send_contact_us_communication", "Time taken to prepare and send communication inquiry email")]
        public virtual async Task SendContactUsForCommunicationAsync(ContactUsFormViewModel contact)
        {
            var emailTemplate = await MailTemplateService.GetMailTemplateByNameAsync(Constants.ContactUsForCommunication).ConfigureAwait(false);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.ContactUsForCommunication}");
            }

            string groupName = $"ContactUsForCommunication | {emailTemplate.Name} | {DateTime.Now:yyyy-MM-dd HH:mm}";
            emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(
                emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk,
                emailTemplate.Body, emailTemplate.Name, groupName
            );

            string companyName = await SettingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
            string adminUserName = await SettingService.GetSettingByKeyAsync(Constants.AdminUserName).ConfigureAwait(false);
            string webSiteCompanyEmailAddress = await SettingService.GetSettingByKeyAsync(Constants.WebSiteCompanyEmailAddress).ConfigureAwait(false);

            string baseurl = GetSiteBaseUrl();

            var model = new
            {
                ContactUs = contact,
                CompanyName = companyName,
                BaseUrl = baseurl,
                WebSiteIconUrl = $"{baseurl}/images/logo.jpg",
                AdminPageUrl = $"{baseurl}/account/adminlogin/"
            };

            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(emailTemplate.Body);
            string body = Engine.Razor.RunCompile(emailTemplate.Body, templateKey, null, model);

            EmailSender.SendEmailInBackground(
                await SettingService.GetEmailAccountAsync().ConfigureAwait(false),
                emailTemplate.Subject,
                body,
                adminUserName,
                companyName,
                webSiteCompanyEmailAddress,
                companyName
            );
        }


        [Timed("service.email.generate_rss_template", "Time taken to generate RSS email template")]
        public virtual string GenerateRssEmailTemplate(MailTemplate rssTemplate)
        {
            if (rssTemplate == null)
            {
                throw new ArgumentNullException(nameof(rssTemplate));
            }

            if (string.IsNullOrWhiteSpace(rssTemplate.Body))
            {
                return string.Empty;
            }

            // Web sitesi URL'sini al
            string baseurl = GetSiteBaseUrl();

            // Razor Template için model oluştur
            RazorEngineModel razorEngineModel = new RazorEngineModel();
            razorEngineModel["CompanyName"] = SettingService.GetSettingByKey(Constants.CompanyName);
            razorEngineModel["CompanyAddress"] = SettingService.GetSettingByKey(Constants.CompanyAddress);
            razorEngineModel["WebSiteCompanyEmailAddress"] = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);
            razorEngineModel["BaseUrl"] = baseurl;
            razorEngineModel["WebSiteIconUrl"] = string.Format("{0}/images/logo.jpg", baseurl);

            // Rich-text editors may store HTML-encoded Razor; decode once before compiling.
            string templateBody = System.Net.WebUtility.HtmlDecode(rssTemplate.Body) ?? rssTemplate.Body;

            // Şablonu işle
            var result = GetRenderOutput(templateBody, razorEngineModel);
            if (result == null)
            {
                return templateBody;
            }

            if (result.RazorErrors != null && result.RazorErrors.IsNotEmpty())
            {
                string errorList = string.Join(Environment.NewLine, result.RazorErrors.Select(e => e.ToString()));
                throw new ArgumentException("RazorEngine error:" + errorList);
            }

            if (result.GeneralError != null)
            {
                throw new ArgumentException("RazorEngine error: " + result.GeneralError.Message, result.GeneralError);
            }

            if (result.templateCompilationException != null)
            {
                throw new ArgumentException("RazorEngine compilation error: " + result.templateCompilationException.Message, result.templateCompilationException);
            }

            // Prefer rendered output; if empty, return the (decoded) source so download still works.
            return string.IsNullOrEmpty(result.Result) ? templateBody : result.Result;
        }

        [Timed("service.razor_engine.render_by_model", "Time taken to render template by RazorEngine model")]
        public virtual RazorRenderResult GetRenderOutputByRazorEngineModel<T>(String razorTemplate, T razorEngineModel) where T : RazorTemplateModel
        {
            // FIX: delegate to the singleton engine. Templates are compiled once and cached by
            // content hash instead of recompiling (and leaking a dynamic assembly) on every call.
            return RazorTemplateEngine.GetRenderOutputByModel(razorTemplate, razorEngineModel);
        }

        [Timed("service.razor_engine.render_output", "Time taken to render template output")]
        public virtual RazorRenderResult GetRenderOutput(String razorTemplate, RazorEngineModel razorEngineModel = null)
        {
            var result = new RazorRenderResult();

            if (String.IsNullOrEmpty(razorTemplate))
            {
                return result;
            }
            try
            {
                // FIX: single shared, compile-once engine (no per-call RazorEngineService.Create,
                // no Debug=true, no dynamic-assembly leak).
                return RazorTemplateEngine.GetRenderOutput(razorTemplate, razorEngineModel);
            }
            catch (TemplateCompilationException ex)
            {
                result.templateCompilationException = ex;
            }
            catch (Exception ex)
            {
                result.GeneralError = ex;
            }
            return result;
        }
    }
}