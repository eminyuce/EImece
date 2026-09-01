using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class MailTemplateTestService : IMailTemplateTestService
    {
        private readonly ILogger<MailTemplateTestService> _logger;

        private readonly IMailTemplateService MailTemplateService;
        private readonly ISettingService SettingService;
        private readonly IEmailSender EmailSender;
        private readonly IRazorTemplateEngine RazorTemplateEngine;

        public MailTemplateTestService(IMailTemplateService mailTemplateService,
            ISettingService settingService,
            IEmailSender emailSender,
            IRazorTemplateEngine razorTemplateEngine, ILogger<MailTemplateTestService> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            MailTemplateService = mailTemplateService ?? throw new ArgumentNullException(nameof(mailTemplateService));
            SettingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            EmailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            RazorTemplateEngine = razorTemplateEngine ?? throw new ArgumentNullException(nameof(razorTemplateEngine));
        }

        public async Task<MailTemplateTestPreview> InspectAsync(SendMailTemplateTestRequest request, string defaultRecipientEmail)
        {
            var template = await ResolveTemplateAsync(request).ConfigureAwait(false);
            var context = await BuildDummyDataContextAsync(defaultRecipientEmail).ConfigureAwait(false);
            var usage = MailTemplateModelInspector.Analyze(template.Subject, template.Body);

            return new MailTemplateTestPreview
            {
                Id = template.Id,
                Name = template.Name,
                Subject = template.Subject,
                Properties = MailTemplateModelInspector.BuildProperties(usage.PropertyPaths, context, usage.CollectionItemPaths)
            };
        }

        public async Task<MailTemplateTestRenderResult> PreviewAsync(SendMailTemplateTestRequest request)
        {
            var template = await ResolveTemplateAsync(request).ConfigureAwait(false);
            var modelData = request != null ? request.ModelData : null;
            return RenderTemplate(template.Subject, template.Body, modelData);
        }

        public async Task<MailTemplateTestSendResult> SendTestEmailAsync(SendMailTemplateTestRequest request)
        {
            if (request == null)
            {
                return Fail("İstek boş olamaz.");
            }

            var recipient = (request.RecipientEmail ?? string.Empty).Trim();
            if (!IsValidEmail(recipient))
            {
                return Fail("Geçerli bir alıcı e-posta adresi girin.");
            }

            if (!AppConfig.IsSmtpClientEnabled)
            {
                return Fail("SMTP gönderimi yapılandırma dosyasında kapalı (IsSmtpClientEnabled=false).");
            }

            EmailAccount emailAccount;
            try
            {
                emailAccount = await SettingService.GetEmailAccountAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load SMTP account for mail template test.");
                return Fail("SMTP hesabı okunamadı. Sistem Ayarları > SMTP sekmesini kontrol edin.");
            }

            if (emailAccount == null || string.IsNullOrWhiteSpace(emailAccount.Host))
            {
                return Fail("SMTP sunucusu tanımlı değil. Lütfen Sistem Ayarları > SMTP sekmesinden SMTP bilgilerini kaydedin.");
            }

            var fromAddress = string.IsNullOrWhiteSpace(emailAccount.Email) ? emailAccount.Username : emailAccount.Email;
            if (string.IsNullOrWhiteSpace(fromAddress) || !IsValidEmail(fromAddress))
            {
                return Fail("Gönderen e-posta adresi tanımlı değil. Lütfen Sistem Ayarları > SMTP sekmesini kontrol edin.");
            }

            MailTemplate template;
            try
            {
                template = await ResolveTemplateAsync(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }

            var subjectTemplate = string.IsNullOrWhiteSpace(request.SubjectOverride)
                ? template.Subject
                : request.SubjectOverride;

            var render = RenderTemplate(subjectTemplate, template.Body, request.ModelData);
            if (!render.Success)
            {
                return Fail(render.ErrorMessage);
            }

            var subject = PrefixTestSubject(render.Subject);
            var fromName = string.IsNullOrWhiteSpace(emailAccount.DisplayName) ? fromAddress : emailAccount.DisplayName;

            try
            {
                EmailSender.SendEmail(
                    emailAccount,
                    subject,
                    render.Body,
                    fromAddress,
                    fromName,
                    recipient,
                    recipient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email for MailTemplate Id={0} to {1}", template.Id, recipient);
                return Fail("E-posta gönderilemedi: " + ex.ToFormattedString());
            }

            return new MailTemplateTestSendResult
            {
                Success = true,
                Subject = subject,
                Message = "Test e-postası " + recipient + " adresine gönderildi."
            };
        }

        private async Task<MailTemplate> ResolveTemplateAsync(SendMailTemplateTestRequest request)
        {
            MailTemplate stored = null;
            if (request != null && request.Id > 0)
            {
                stored = await MailTemplateService.GetSingleAsync(request.Id).ConfigureAwait(false);
            }

            var hasUnsavedBody = request != null && !string.IsNullOrWhiteSpace(request.Body);
            var hasUnsavedSubject = request != null && !string.IsNullOrWhiteSpace(request.Subject);

            if (stored == null && !hasUnsavedBody)
            {
                throw new ArgumentException("E-posta şablonu bulunamadı.");
            }

            var template = stored != null
                ? new MailTemplate
                {
                    Id = stored.Id,
                    Name = stored.Name,
                    Subject = stored.Subject,
                    Body = stored.Body
                }
                : new MailTemplate
                {
                    Id = request.Id,
                    Name = "Taslak"
                };

            if (hasUnsavedSubject)
            {
                template.Subject = request.Subject;
            }

            if (hasUnsavedBody)
            {
                template.Body = request.Body;
            }

            if (string.IsNullOrWhiteSpace(template.Body))
            {
                throw new ArgumentException("Şablon içeriği boş. Önce şablonu kaydedin veya içerik girin.");
            }

            return template;
        }

        private MailTemplateTestRenderResult RenderTemplate(string subject, string body, IDictionary<string, string> modelData)
        {
            if (RazorTemplateEngine == null)
            {
                return MailTemplateModelInspector.Render(subject, body, modelData);
            }

            var model = MailTemplateModelInspector.BuildDynamicModel(modelData);
            var subjectDecoded = WebUtility.HtmlDecode(subject ?? string.Empty) ?? string.Empty;
            var bodyDecoded = WebUtility.HtmlDecode(body ?? string.Empty) ?? string.Empty;
            var subjectRender = RazorTemplateEngine.GetRenderOutputDynamic(subjectDecoded, model);
            var bodyRender = RazorTemplateEngine.GetRenderOutputDynamic(bodyDecoded, model);
            return MailTemplateModelInspector.FromRazorResult(subjectDecoded, subjectRender, bodyRender);
        }

        private async Task<MailTemplateDummyDataContext> BuildDummyDataContextAsync(string defaultRecipientEmail)
        {
            var context = MailTemplateDummyDataContext.CreateDefaults();
            var baseUrl = EntityExtension.GetAbsoluteApplicationBaseUrl();
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                context.BaseUrl = baseUrl.TrimEnd('/');
            }

            context.CompanyName = await GetSettingOrDefaultAsync(Constants.CompanyName, context.CompanyName).ConfigureAwait(false);
            context.CompanyEmail = await GetSettingOrDefaultAsync(Constants.WebSiteCompanyEmailAddress, context.CompanyEmail).ConfigureAwait(false);
            context.CompanyAddress = await GetSettingOrDefaultAsync(Constants.CompanyAddress, context.CompanyAddress).ConfigureAwait(false);
            context.CompanyPhone = await GetSettingOrDefaultAsync(Constants.WebSiteCompanyPhoneAndLocation, context.CompanyPhone).ConfigureAwait(false);
            context.LogoUrl = context.BaseUrl + Constants.LogoImagePath;
            context.RecipientEmail = FirstNonEmpty(defaultRecipientEmail, context.CompanyEmail, context.RecipientEmail);
            return context;
        }

        private async Task<string> GetSettingOrDefaultAsync(string key, string fallback)
        {
            try
            {
                var value = await SettingService.GetSettingByKeyAsync(key).ConfigureAwait(false);
                return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private static MailTemplateTestSendResult Fail(string message)
        {
            return new MailTemplateTestSendResult
            {
                Success = false,
                Message = message
            };
        }

        private static string PrefixTestSubject(string subject)
        {
            var value = string.IsNullOrWhiteSpace(subject) ? "Test E-posta" : subject.Trim();
            if (value.StartsWith("[TEST]", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            return "[TEST] " + value;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var address = new MailAddress(email);
                return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }
    }
}
