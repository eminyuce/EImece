using EImece.Domain.ApiRepositories;
using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers.RazorCustomRssTemplate;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Dynamic;
using System.IO;

namespace EImece.Domain.Helpers.EmailHelper
{
    public class RazorEngineHelper
    {
        private static readonly Logger RazorEngineLogger = LogManager.GetCurrentClassLogger();

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
                throw new ArgumentException("ConfirmYourAccount email template does not exists");
            }
            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);

            var Request = HttpContext.Create().Request;
            var baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";

            var model = new
            {
                Email = email,
                callbackUrl = callbackUrl,
                Name = name
            };

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string result = Engine.Razor.RunCompile(template, templateKey, null, model);

            return new Tuple<string, string>(emailTemplate.Subject, result);
        }

        public Tuple<string, string> ForgotPasswordEmailBody(string email, string callbackUrl)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.ForgotPasswordMailTemplate);
            if (emailTemplate == null)
            {
                return new Tuple<string, string>("", "");
            }
            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);

            var Request = HttpContext.Create().Request;
            var baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";

            var model = new
            {
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

        public Tuple<string, string,Customer> OrderConfirmationEmail(int orderId)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.OrderConfirmationEmailMailTemplate);
            if (emailTemplate == null)
            {
                return new Tuple<string, string, Customer>("", "",null);
            }

            var model = MailTemplateService.GenerateOrderConfirmationEmailRazorTemplate(orderId);
            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string result = GetRenderOutput2(template,  model);
            return new Tuple<string, string, Customer>(emailTemplate.Subject, result, model.FinishedOrder.Customer);
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
            var webSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string subject = Engine.Razor.RunCompile(emailTemplate.Subject, templateKey, null, contact);
            string body = Engine.Razor.RunCompile(template, subject + "" + GeneralHelper.GetHashString(template), null, contact);
            EmailSender.SendEmail(SettingService.GetEmailAccount(),
                subject,
                body,
                webSiteCompanyEmailAddress,
                companyname,
                webSiteCompanyEmailAddress,
                companyname);
        }

        public void SendContactUsAboutProductDetailEmail(ContactUsFormViewModel contact)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.ContactUsAboutProductInfoMailTemplate);
            if (emailTemplate == null)
            {
                throw new ArgumentException("NO email template is defined for " + Constants.ContactUsAboutProductInfoMailTemplate);
            }
            string groupName = string.Format("{0} | {1} | {2}", "ContactUsFormViewModel", emailTemplate.Name, DateTime.Now.ToString("yyyy-MM-dd hh:mm"));
            emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk, emailTemplate.Body, emailTemplate.Name, groupName);

            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);
            var WebSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);

            var Request = HttpContext.Create().Request;
            var baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";

            dynamic model = new ExpandoObject();
            model.ContactUs = contact;
            model.CompanyName = companyname;
            model.ProductPageLink = baseurl;

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string body = Engine.Razor.RunCompile(template, templateKey, null, (object)model);
            EmailSender.SendEmail(SettingService.GetEmailAccount(),
                emailTemplate.Subject,
                body,
                WebSiteCompanyEmailAddress,
                companyname,
                WebSiteCompanyEmailAddress,
                companyname);
        }
        public void SendContactUsForCommunication(ContactUsFormViewModel contact)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName(Constants.ContactUsForCommunication);
            if (emailTemplate == null)
            {
                throw new ArgumentException("NO email template is defined for " + Constants.ContactUsForCommunication);
            }
            string groupName = string.Format("{0} | {1} | {2}", "ContactUsForCommunication", emailTemplate.Name, DateTime.Now.ToString("yyyy-MM-dd hh:mm"));
            emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk,
                emailTemplate.Body, 
                emailTemplate.Name,
                groupName);

            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);
            var WebSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);

            var Request = HttpContext.Create().Request;
            var baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";

            dynamic model = contact;

            string template = emailTemplate.Body;
            string templateKey = emailTemplate.Subject + "" + GeneralHelper.GetHashString(template);
            string body = Engine.Razor.RunCompile(template, templateKey, null, contact);
            var settingEmailAccount = SettingService.GetEmailAccount();

            RazorEngineLogger.Info("settingEmailAccount:" +
                settingEmailAccount+ 
                " body" + body+
                " subject:" + emailTemplate.Subject + 
                " WebSiteCompanyEmailAddress" + WebSiteCompanyEmailAddress +
                " companyname:" + companyname);
            EmailSender.SendEmail(settingEmailAccount,
                emailTemplate.Subject,
                body,
                WebSiteCompanyEmailAddress,
                companyname,
                WebSiteCompanyEmailAddress,
                companyname);
        }
        public RazorRenderResult GetRenderOutput2(String razorTemplate, OrderConfirmationEmailRazorTemplate razorEngineModel = null)
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
                    var runner = service.CompileRunner<OrderConfirmationEmailRazorTemplate>(result.Source);
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