using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Ninject;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public abstract class BasePaymentController : BaseController
    {
        private static readonly Logger BasePaymentLogger = LogManager.GetCurrentClassLogger();

        protected void InformCustomerToFillOutForm(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Name.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Name", Resource.PleaseEnterYourName);
            }
            if (string.IsNullOrEmpty(customer.Surname.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Surname", Resource.PleaseEnterYourSurname);
            }
            if (string.IsNullOrEmpty(customer.GsmNumber.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.GsmNumber", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.Email.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Email", Resource.PleaseEnterYourEmail);
            }
            if (string.IsNullOrEmpty(customer.City.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.City", Resource.PleaseEnterYourCity);
            }
            if (string.IsNullOrEmpty(customer.Town.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Town", Resource.PleaseEnterYourTown);
            }
            if (string.IsNullOrEmpty(customer.Country.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Country", Resource.PleaseEnterYourCountry);
            }
            if (string.IsNullOrEmpty(customer.District.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.District", Resource.PleaseEnterYourDistrict);
            }
            if (string.IsNullOrEmpty(customer.Street.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.Street", Resource.PleaseEnterYourStreet);
            }
            if (string.IsNullOrEmpty(customer.IdentityNumber.ToStr().Trim()))
            {
                ModelState.AddModelError("customer.IdentityNumber", Resource.MandatoryField);
            }
            ModelState.AddModelError("", Resource.PleaseFillOutMandatoryBelowFields);
        }

        protected Domain.Entities.Address SetAddress(Customer customer, Domain.Entities.Address address)
        {
            if (address == null)
            {
                address = new Domain.Entities.Address();
            }
            address.Street = customer.Street;
            address.District = customer.District;
            address.City = customer.City;
            address.Country = customer.Country;
            address.ZipCode = customer.ZipCode;
            address.Description = customer.RegistrationAddress;
            address.Name = customer.FullName;
            address.CreatedDate = DateTime.Now;
            address.UpdatedDate = DateTime.Now;
            address.IsActive = true;
            address.Position = 1;
            address.Lang = CurrentLanguage;
            return address;
        }

        private void SendEmails(Order order)
        {
            try
            {
                var emailTemplate = RazorEngineHelper.OrderConfirmationEmail(order.Id);
                EmailSender.SendRenderedEmailTemplateToCustomer(SettingService.GetEmailAccount(), emailTemplate);
            }
            catch (Exception e)
            {
                PaymentLogger.Error(e, "OrderConfirmationEmail exception");
            }

            try
            {
                var emailTemplate = RazorEngineHelper.CompanyGotNewOrderEmail(order.Id);
                EmailSender.SendRenderedEmailTemplateToAdminUsers(SettingService.GetEmailAccount(), emailTemplate);
            }
            catch (Exception e)
            {
                BasePaymentLogger.Error(e, "CompanyGotNewOrderEmail exception");
            }
        }
    }
}