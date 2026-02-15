using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using NLog;
using Resources;
using AutoMapper;
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
        protected int CurrentLanguage
        {
            get
            {
                var lang = Thread.CurrentThread.CurrentCulture.ToString();
                return EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            }
        }
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

        [Inject]
        public IMapper Mapper { get; set; }

        public HomeController(ApplicationUserManager userManager)
        {
            this.UserManager = userManager;
        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            var languageCookie = System.Web.HttpContext.Current.Request.Cookies["Language"];
            if (languageCookie != null)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(languageCookie.Value);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(languageCookie.Value);
            }
            base.Initialize(requestContext);
        }
        // GET: Customers/Home
        public ActionResult Index()
        {
            var customer = GetCustomer();
            ViewBag.Title = Resource.CustomerAccount;
            return View(customer);
        }

        private CustomerDto GetCustomer()
        {
            var user = UserManager.FindByName(User.Identity.GetUserName());
            var customer = CustomerService.GetUserId(user.Id);
            customer.Orders = OrderService.GetOrdersByUserId(customer.UserId);
            if (customer.Gender == 0)
            {
                customer.Gender = (int)GenderType.Man;
            }

            return Mapper.Map<CustomerDto>(customer);
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
        public ActionResult Index(CustomerDto customer)
        {
            if (customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customerEntity = Mapper.Map<Customer>(customer);
            bool isValidCustomer = customerEntity != null && customerEntity.isValidCustomer();
            if (isValidCustomer)
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                if (!user.FirstName.Equals(customerEntity.Name, StringComparison.InvariantCultureIgnoreCase) || !user.LastName.Equals(customerEntity.Surname, StringComparison.InvariantCultureIgnoreCase))
                {
                    user.FirstName = customerEntity.Name;
                    user.LastName = customerEntity.Surname;
                    UserManager.Update(user);
                }

                customerEntity.UserId = user.Id;
                customerEntity.Ip = GeneralHelper.GetIpAddress();
                customerEntity = CustomerService.SaveOrEditEntity(customerEntity);
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                return View(Mapper.Map<CustomerDto>(customerEntity));
            }
            else
            {
                InformCustomerToFillOutForm(customerEntity);
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
            else
            {
                if (GeneralHelper.IsGsmNumberNotValid(customer.GsmNumber.ToStr()))
                {
                    ModelState.AddModelError("GsmNumber", Resource.GsmNumberNotValidMessage);
                }
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
           
            ModelState.AddModelError("", Resource.PleaseFillOutMandatoryBelowFields);
        }

        public ActionResult SendMessageToSeller()
        {
            ViewBag.Title = Resource.SendMessageToSeller;
            var customer = GetCustomer();
            var faqs = FaqService.GetActiveBaseEntitiesFromCache(true, null);
            return View(new SendMessageToSellerViewModel() { Customer = customer, Faqs = Mapper.Map<System.Collections.Generic.List<FaqDto>>(faqs) });
        }

        [HttpPost]
        public ActionResult SendSellerMessage(ContactUsFormViewModel contact)
        {
            if (contact == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Sending email.
                contact.ItemType = EImeceItemType.Ticket;
                contact.IPAddress = HttpContext.Request.UserHostAddress;
                RazorEngineHelper.SendMessageToSeller(contact);
                TempData["SuccessMessage"] = Resource.YourMessageHasBeenSentToSeller;
                return RedirectToAction("SendMessageToSeller");
            }
            catch(Exception ex)
            {
                // Handle any errors (e.g., email sending failure)
                TempData["ErrorMessage"] = Resource.EmailSendingFailed; // Add this to your Resource file
                return RedirectToAction("SendMessageToSeller");
            }
        }

        public ActionResult Faq()
        {
            ViewBag.Title = Resource.Faq;
            var customer = GetCustomer();
            var faqs = FaqService.GetActiveBaseEntitiesFromCache(true, CurrentLanguage);
            return View(new SendMessageToSellerViewModel() { Customer = customer, Faqs = Mapper.Map<System.Collections.Generic.List<FaqDto>>(faqs) });
        }

        public ActionResult CustomerOrders(string search = "")
        {
            ViewBag.Title = Resource.CustomerDetail;
            var customer = GetCustomer();
            var user = UserManager.FindByName(User.Identity.GetUserName());
            var orders = OrderService.GetOrdersUserId(user.Id, search).OrderByDescending(r=>r.UpdatedDate).ToList();
            return View(new CustomerOrdersViewModel() { Customer = customer, Orders = Mapper.Map<System.Collections.Generic.List<OrderDto>>(orders) });
        }

        public ActionResult CustomerOrderDetail(int id)
        {
            ViewBag.Title = Resource.CustomerDetail;
            var customer = GetCustomer();
            var order = OrderService.GetOrderById(id);
            return View(new CustomerOrderDetailViewModel() { Customer = customer, Order = Mapper.Map<OrderDto>(order) });
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