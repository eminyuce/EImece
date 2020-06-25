using EImece.Domain.ApiRepositories;
using EImece.Domain.Entities;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers.RazorCustomRssTemplate;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
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

        public string ForgotPasswordEmailBody(string email, string callbackUrl)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName("ForgotPassword");
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

            return result;
        }

        public void SendContactUsAboutProductDetailEmail(ContactUsFormViewModel contact)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName("ContactUsAboutProductInfo");
            string groupName = string.Format("{0} | {1} | {2}", "ContactUsFormViewModel", emailTemplate.Name, DateTime.Now.ToString("yyyy-MM-dd hh:mm"));
            emailTemplate.Body = BitlyRepository.ConvertEmailBodyForTracking(emailTemplate.TrackWithBitly, emailTemplate.TrackWithMlnk, emailTemplate.Body, emailTemplate.Name, groupName);

            String companyname = SettingService.GetSettingByKey(Constants.CompanyName);
            var WebSiteCompanyPhoneAndLocation = SettingService.GetSettingByKey("WebSiteCompanyPhoneAndLocation");
            var WebSiteCompanyEmailAddress = SettingService.GetSettingByKey("WebSiteCompanyEmailAddress");

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
                configuration.Namespaces.Add("EImece.Domain.Helpers.RazorCustomRssTemplate");
                configuration.Namespaces.Add("System.Xml");
                configuration.Namespaces.Add("System.Web.Mvc");
                configuration.Namespaces.Add("System.Text");
                configuration.Namespaces.Add("System.Web.Mvc.Html");
                configuration.Namespaces.Add("System.Xml.Linq");
                configuration.Namespaces.Add("System.Linq");
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