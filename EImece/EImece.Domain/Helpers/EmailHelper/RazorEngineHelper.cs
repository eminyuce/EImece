using EImece.Domain.ApiRepositories;
using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers.RazorCustomRssTemplate;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.IO;
using System.Linq;

namespace EImece.Domain.Helpers.EmailHelper
{
    public class RazorEngineHelper
    {
        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        [Inject]
        public ISettingService SettingService { get; set; }

        [Inject]
        public IHttpContextFactory HttpContext { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }

        [Inject]
        public BitlyRepository BitlyRepository { get; set; }

        public Tuple<string, string> ConfirmYourAccountEmailBody(string email, string name, string callbackUrl)
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
                WebSiteIconUrl = baseurl + "/images/logo.jpg",
                Email = email,
                callbackUrl = callbackUrl,
                Name = name,
                companyname = companyname
            };

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template) + "_" + GetUniqueId();
            string result = Engine.Razor.RunCompile(template, templateKey, null, model);

            return new Tuple<string, string>(emailTemplate.Subject, result);
        }

        private string GetSiteBaseUrl()
        {
            var Request = HttpContext.Create().Request;
            var baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
            return baseurl;
        }

        public Tuple<string, string> ForgotPasswordEmailBody(string email, string callbackUrl)
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
                WebSiteIconUrl = baseurl + "/images/logo.jpg",
                Email = email,
                ForgotPasswordLink = callbackUrl,
                CompanyName = companyname,
                CompanyPageLink = baseurl
            };

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template) + "_" + GetUniqueId();
            string result = Engine.Razor.RunCompile(template, templateKey, null, model);

            return new Tuple<string, string>(emailTemplate.Subject, result);
        }

        public Tuple<string, RazorRenderResult, Customer> CompanyGotNewOrderEmail(int orderId)
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
            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(template) + "_" + GetUniqueId();

            // RazorEngine kullanarak template'i render ediyoruz
            var result = GetRenderOutputByRazorEngineModel(template, model);

            // Konu başlığını Razor ile render ediyoruz
            string subject = Engine.Razor.RunCompile(emailTemplate.Subject, templateKey, null, modelSubject);

            // Sonuç olarak: Konu, şablonun render edilmiş sonucu ve müşteri bilgisi döndürülüyor
            return new Tuple<string, RazorRenderResult, Customer>(subject, result, model.FinishedOrder.Customer);
        }

        public Tuple<string, RazorRenderResult, Customer> OrderConfirmationEmail(int orderId)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.OrderConfirmationEmailMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException($"E-posta şablonu bulunamadı: {Constants.OrderConfirmationEmailMailTemplate}");
            }

            OrderConfirmationEmailRazorTemplate model = MailTemplateService.GenerateOrderConfirmationEmailRazorTemplate(orderId);
            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template) + "_" + GetUniqueId();
            var result = GetRenderOutputByRazorEngineModel(template, model);
            return new Tuple<string, RazorRenderResult, Customer>(emailTemplate.Subject, result, model.FinishedOrder.Customer);
        }

        public void SendMessageToSeller(ContactUsFormViewModel contact)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.SendMessageToSellerMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException("NO email template is defined for " + Constants.SendMessageToSellerMailTemplate);
            }

            string groupName = string.Format("{0} | {1} | {2}", "SendMessageToSeller", emailTemplate.Name, DateTime.Now.ToString("yyyy-MM-dd hh:mm"));
            emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk, emailTemplate.Body, emailTemplate.Name, groupName);

            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);
            var adminUserName = SettingService.GetSettingByKey(Constants.AdminUserName);

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "_" + GeneralHelper.GetHashString(template) + "_" + GetUniqueId();

            string subject = Engine.Razor.RunCompile(emailTemplate.Subject, templateKey, null, contact);
            string body = Engine.Razor.RunCompile(template, templateKey + "_body", null, contact); // Use different key for body

            EmailSender.SendEmail(SettingService.GetEmailAccount(),
                subject,
                body,
                adminUserName,
                companyname,
                adminUserName,
                companyname);
        }

        private static string GetUniqueId()
        {
            // Generate a unique key by adding a timestamp or random identifier
            return DateTime.Now.Ticks.ToString();
            // or use Guid.NewGuid().ToString() for even more uniqueness
        }

        public void SendContactUsAboutProductDetailEmail(ContactUsFormViewModel contact)
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
            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(emailTemplate.Body) + "_" + GetUniqueId();
            string body = Engine.Razor.RunCompile(emailTemplate.Body, templateKey, null, model);

            // E-posta gönder
            EmailSender.SendEmail(
                SettingService.GetEmailAccount(),
                emailTemplate.Subject,
                body,
                adminUserName,
                companyName,
                adminUserName,
                companyName
            );
        }

        public void SendContactUsForCommunication(ContactUsFormViewModel contact)
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
            string templateKey = emailTemplate.Subject + GeneralHelper.GetHashString(emailTemplate.Body) + "_" + GetUniqueId();
            string body = Engine.Razor.RunCompile(emailTemplate.Body, templateKey, null, model);

            // E-posta gönder
            EmailSender.SendEmail(
                SettingService.GetEmailAccount(),
                emailTemplate.Subject,
                body,
                adminUserName,
                companyName,
                webSiteCompanyEmailAddress,
                companyName
            );
        }


        public string GenerateRssEmailTemplate(int id)
        {
            // E-posta şablonunu al
            var rssTemplate = MailTemplateService.GetSingle(id);
             
            // Şirket ve Yönetici bilgileri
            string companyName = SettingService.GetSettingByKey(Constants.CompanyName);
            string adminUserName = SettingService.GetSettingByKey(Constants.AdminUserName);
            string webSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);

            // Web sitesi URL'sini al
            string baseurl = GetSiteBaseUrl();

            // Razor Template için model oluştur
           
            RazorEngineModel razorEngineModel = new RazorEngineModel();
            razorEngineModel["CompanyName"] = companyName;
            razorEngineModel["BaseUrl"] = baseurl;
            razorEngineModel["WebSiteIconUrl"] = $"{baseurl}/images/logo.jpg";
         

            // Şablonu işle
            var result = GetRenderOutput(rssTemplate.Body, razorEngineModel);
            if(result.RazorErrors.IsNotEmpty())
            {
                string errorList = string.Join(Environment.NewLine, result.RazorErrors.Select(e => e.ToString()));

                throw new ArgumentException($"RazorEngine error:"+ errorList);
            }
            return result.Result;
        }

        public RazorRenderResult GetRenderOutputByRazorEngineModel<T>(String razorTemplate, T razorEngineModel) where T : RazorTemplateModel
        {
            var result = new RazorRenderResult();

            if (String.IsNullOrEmpty(razorTemplate))
            {
                return result;
            }
            try
            {
                result.Source = razorTemplate;

                var configuration = new TemplateServiceConfiguration { Debug = true };
                configuration.Namespaces.Add("EImece.Domain.Helpers");
                configuration.Namespaces.Add("EImece.Domain.Entities");
                configuration.Namespaces.Add("EImece.Domain.Models.FrontModels");
                configuration.Namespaces.Add("EImece.Domain.Helpers.RazorCustomRssTemplate");
                configuration.Namespaces.Add("System.Xml");
                configuration.Namespaces.Add("System.Web.Mvc");
                configuration.Namespaces.Add("System.Text");
                configuration.Namespaces.Add("System.Web.Mvc.Html");
                configuration.Namespaces.Add("System.Xml.Linq");
                configuration.Namespaces.Add("System.Linq");
                configuration.Namespaces.Add("Resources");
                configuration.Namespaces.Add("System.ServiceModel.Syndication");
                configuration.BaseTemplateType = typeof(VBCustomTemplateBase<>);

                using (var service = RazorEngineService.Create(configuration))
                {
                    using (var writer = new StringWriter())
                    {
                        var runner = service.CompileRunner<T>(result.Source);
                        runner.Run(razorEngineModel, writer);
                        result.Result = writer.ToString();
                    }
                }
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

        public RazorRenderResult GetRenderOutput(String razorTemplate, RazorEngineModel razorEngineModel = null)
        {
            var result = new RazorRenderResult();

            if (String.IsNullOrEmpty(razorTemplate))
            {
                return result;
            }
            try
            {
                result.Source = razorTemplate;

                var configuration = new TemplateServiceConfiguration { Debug = true };
                configuration.Namespaces.Add("EImece.Domain.Helpers");
                configuration.Namespaces.Add("EImece.Domain.Entities");
                configuration.Namespaces.Add("EImece.Domain.Models.FrontModels");
                configuration.Namespaces.Add("EImece.Domain.Helpers.RazorCustomRssTemplate");
                configuration.Namespaces.Add("System.Xml");
                configuration.Namespaces.Add("System.Web.Mvc");
                configuration.Namespaces.Add("System.Text");
                configuration.Namespaces.Add("System.Web.Mvc.Html");
                configuration.Namespaces.Add("System.Xml.Linq");
                configuration.Namespaces.Add("System.Linq");
                configuration.Namespaces.Add("Resources");
                configuration.Namespaces.Add("System.ServiceModel.Syndication");
                configuration.BaseTemplateType = typeof(VBCustomTemplateBase<>);

                using (var service = RazorEngineService.Create(configuration))
                using (var writer = new StringWriter())
                {
                    var runner = service.CompileRunner<RazorEngineModel>(result.Source);
                    razorEngineModel = razorEngineModel == null ? new RazorEngineModel() : razorEngineModel;
                    runner.Run(razorEngineModel, writer);
                    result.Result = writer.ToString();
                }
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