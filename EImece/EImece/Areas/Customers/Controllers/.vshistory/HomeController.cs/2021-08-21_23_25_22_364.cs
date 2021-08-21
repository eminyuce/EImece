using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using NLog;
using Resources;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;
using static EImece.Controllers.ManageController;
using System.Threading;
using System.Globalization;

namespace EImece.Areas.Customers.Controllers
{
    [AuthorizationAttribute(Roles = Domain.Constants.CustomerRole)]
    public class HomeController : Controller
    {
        private static readonly Logger HomeLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public IFaqService FaqService { get; set; }

        [Inject]
        public ISubscriberService SubsciberService { get; set; }

        [Inject]
        public ISettingService SettingService { get; set; }

        [Inject]
        public ApplicationSignInManager SignInManager { get; set; }

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public RazorEngineHelper RazorEngineHelper { get; set; }

        public HomeController(ApplicationUserManager userManager)
        {
            this.UserManager = userManager;
        }

        // GET: Customers/Home
        public ActionResult Index()
        {
            SetLanguage();
            Customer customer = GetCustomer();
            ViewBag.Title = Resource.CustomerAccount;
            return View(customer);
        }

        private void SetLanguage()
        {
            var cultureName = Session[EImece.Domain.Constants.LanguageSession].ToStr();
            if (String.IsNullOrEmpty(cultureName))
                return;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
        }

        private Customer GetCustomer()
        {
            ApplicationUser user;
            Customer customer;
            user = UserManager.FindByName(User.Identity.GetUserName());
            customer = CustomerService.GetUserId(user.Id);
            customer.Orders = OrderService.GetOrdersByUserId(customer.UserId);
            if (customer.Gender == 0)
            {
                customer.Gender = (int)GenderType.Man;
            }
            return customer;
        }

        public ActionResult WebSiteAddressInfo(bool isMobilePage = false)
        {
            var item = new SettingLayoutViewModel();
            item.isMobilePage = isMobilePage;
            item.WebSiteCompanyPhoneAndLocation = SettingService.GetSettingObjectByKey(Domain.Constants.WebSiteCompanyPhoneAndLocation);
            item.WebSiteCompanyEmailAddress = SettingService.GetSettingObjectByKey(Domain.Constants.WebSiteCompanyEmailAddress);
            return PartialView("_WebSiteAddressInfo", item);
        }

        [HttpPost]
        public ActionResult Index(Customer customer)
        {
            if (customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            bool isValidCustomer = customer != null && customer.isValidCustomer();
            if (isValidCustomer)
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                if (!user.FirstName.Equals(customer.Name, StringComparison.InvariantCultureIgnoreCase) || !user.LastName.Equals(customer.Surname, StringComparison.InvariantCultureIgnoreCase))
                {
                    user.FirstName = customer.Name;
                    user.LastName = customer.Surname;
                    UserManager.Update(user);
                }

                customer.UserId = user.Id;
                customer.Ip = GeneralHelper.GetIpAddress();
                customer = CustomerService.SaveOrEditEntity(customer);
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                return View(customer);
            }
            else
            {
                InformCustomerToFillOutForm(customer);
                return View(customer);
            }
        }

        private void InformCustomerToFillOutForm(Customer customer)
        {
            if (String.IsNullOrEmpty(customer.Name))
            {
                ModelState.AddModelError("Name", Resource.MandatoryField);
            }
            if (String.IsNullOrEmpty(customer.Surname))
            {
                ModelState.AddModelError("Surname", Resource.MandatoryField);
            }
            if (String.IsNullOrEmpty(customer.GsmNumber))
            {
                ModelState.AddModelError("GsmNumber", Resource.MandatoryField);
            }
            if (String.IsNullOrEmpty(customer.City))
            {
                ModelState.AddModelError("City", Resource.MandatoryField);
            }
            if (String.IsNullOrEmpty(customer.Town))
            {
                ModelState.AddModelError("Town", Resource.MandatoryField);
            }
            if (String.IsNullOrEmpty(customer.ZipCode))
            {
                ModelState.AddModelError("ZipCode", Resource.MandatoryField);
            }
            if (String.IsNullOrEmpty(customer.Country))
            {
                ModelState.AddModelError("Country", Resource.MandatoryField);
            }
            if (String.IsNullOrEmpty(customer.District))
            {
                ModelState.AddModelError("District", Resource.MandatoryField);
            }
            if (String.IsNullOrEmpty(customer.Street))
            {
                ModelState.AddModelError("Street", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.IdentityNumber))
            {
                ModelState.AddModelError("IdentityNumber", Resource.WhyNeedIdentityNumber);
            }
            ModelState.AddModelError("", Resource.PleaseFillOutMandatoryBelowFields);
        }

        public ActionResult SendMessageToSeller()
        {
            ViewBag.Title = Resource.SendMessageToSeller;
            var customer = GetCustomer();
            var faqs = FaqService.GetActiveBaseEntitiesFromCache(true, null);
            return View(new SendMessageToSellerViewModel() { Customer = customer, Faqs = faqs });
        }

        public ActionResult SendSellerMessage(ContactUsFormViewModel contact)
        {
            if (contact == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Sending email.
            contact.ItemType = EImeceItemType.Ticket;
            RazorEngineHelper.SendMessageToSeller(contact);
            return RedirectToAction("SendMessageToSeller");
        }

        public ActionResult Faq()
        {
            ViewBag.Title = Resource.Faq;
            var customer = GetCustomer();
            var faqs = FaqService.GetActiveBaseEntitiesFromCache(true, null);
            return View(new SendMessageToSellerViewModel() { Customer = customer, Faqs = faqs });
        }

        public ActionResult CustomerOrders(string search = "")
        {
            ViewBag.Title = Resource.CustomerDetail;
            var customer = GetCustomer();
            var user = UserManager.FindByName(User.Identity.GetUserName());
            var orders = OrderService.GetOrdersUserId(user.Id, search).OrderByDescending(r=>r.UpdatedDate).ToList();
            return View(new CustomerOrdersViewModel() { Customer = customer, Orders = orders });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home", new { @area = "" });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            ViewBag.Title = Resource.Password;
            ViewBag.Customer = GetCustomer();
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}