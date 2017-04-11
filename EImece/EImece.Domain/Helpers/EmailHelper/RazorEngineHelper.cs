using EImece.Domain.Entities;
using EImece.Domain.Services;
using Ninject;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Templating;
using System.Security.Principal;
using EImece.Domain.Services.IServices;
using System.Web;
using EImece.Domain.Factories.IFactories;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.EmailHelper
{
    public class RazorEngineHelper
    {
        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }
        [Inject]
        public ISettingService SettingService { get; set; }

        [Inject]
        //public HttpContextBase HttpContextBase { get; set; }
        public IHttpContextFactory HttpContext { get; set; }


        public string ForgotPasswordEmailBody(string email,string callbackUrl)
        {
            MailTemplate emailTemplate = MailTemplateService.GetMailTemplateByName("ForgotPassword");
            String companyname = SettingService.GetSettingByKey(Settings.CompanyName);

            var Request = HttpContext.Create().Request;
            var baseurl =  Request.Url.Scheme + "://" + Request.Url.Authority +   Request.ApplicationPath.TrimEnd('/') + "/";

            var model = new {
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
    }
}
