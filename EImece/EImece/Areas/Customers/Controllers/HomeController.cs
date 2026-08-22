using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using EImece.Domain.DependencyInjection;
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
        public AutoMapper.IMapper Mapper { get; set; }

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
        public IIdentityManager IdentityManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IRazorEngineHelper RazorEngineHelper { get; set; }

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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // MVC 5 action filters cannot await. GetSettingByKey is LazyCache-backed (async twin
            // exists for request actions); this gate only runs after a cache miss hits the DB.
            if (!SettingService.GetSettingByKey(Domain.Constants.IsProductPriceEnable).ToBool(true))
            {
                filterContext.Result = new RedirectResult("~/");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: Customers/Home
        public async Task<ActionResult> Index()
        {
            CustomerDto customer = await GetCustomerAsync();
            await FillCustomerSummaryInViewBag();
            ViewBag.Title = Resource.CustomerAccount;
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(CustomerDto customer)
        {
            if (customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            bool isValidCustomer = customer != null && customer.isValidCustomer();
            if (isValidCustomer)
            {
                var user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
                if (!user.FirstName.Equals(customer.Name, StringComparison.InvariantCultureIgnoreCase) || !user.LastName.Equals(customer.Surname, StringComparison.InvariantCultureIgnoreCase))
                {
                    user.FirstName = customer.Name;
                    user.LastName = customer.Surname;
                    await UserManager.UpdateAsync(user);
                }

                var customerEntity = await CustomerService.GetUserIdAsync(user.Id) ?? new Customer();
                customerEntity.UserId = user.Id;
                customerEntity.Name = customer.Name;
                customerEntity.Surname = customer.Surname;
                customerEntity.GsmNumber = customer.GsmNumber;
                customerEntity.Email = customer.Email;
                customerEntity.City = customer.City;
                customerEntity.Town = customer.Town;
                customerEntity.District = customer.District;
                customerEntity.Street = customer.Street;
                customerEntity.ZipCode = customer.ZipCode;
                customerEntity.Country = customer.Country;
                customerEntity.Gender = customer.Gender;
                customerEntity.IdentityNumber = customer.IdentityNumber;
                customerEntity.Description = customer.Description;
                customerEntity.Ip = GeneralHelper.GetIpAddress();
                customerEntity.IsActive = true;
                customerEntity = await CustomerService.SaveOrEditEntityAsync(customerEntity);
                var updatedDto = Mapper.Map<CustomerDto>(customerEntity);
                await FillCustomerSummaryInViewBag();
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                return View(updatedDto);
            }
            else
            {
                InformCustomerToFillOutForm(customer);
                await FillCustomerSummaryInViewBag();
                return View(customer);
            }
        }

        private async Task<CustomerDto> GetCustomerAsync()
        {
            var userId = User.Identity.GetUserId();
            ApplicationUser user = null;
            var customerDto = await CustomerService.GetStorefrontCustomerProfileByUserIdAsync(userId);
            if (customerDto == null)
            {
                user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
                customerDto = new CustomerDto
                {
                    UserId = userId,
                    Email = user.Email,
                    Name = user.FirstName,
                    Surname = user.LastName
                };
            }
            if (customerDto.Gender == 0)
            {
                customerDto.Gender = (int)GenderType.Man;
            }
            return customerDto;
        }

        private async Task<EImece.Domain.Models.DTOs.Storefront.CustomerSummaryDto> BuildCustomerSummaryAsync()
        {
            var userId = User.Identity.GetUserId();
            var summary = await CustomerService.GetStorefrontCustomerSummaryByUserIdAsync(userId) ??
                          new EImece.Domain.Models.DTOs.Storefront.CustomerSummaryDto { UserId = userId };
            if (string.IsNullOrEmpty(summary.Name) || string.IsNullOrEmpty(summary.Email))
            {
                var user = await UserManager.FindByNameAsync(User.Identity.GetUserName());
                if (string.IsNullOrEmpty(summary.Name)) summary.Name = user.FirstName;
                if (string.IsNullOrEmpty(summary.Surname)) summary.Surname = user.LastName;
                if (string.IsNullOrEmpty(summary.Email)) summary.Email = user.Email;
            }
            if (summary.CreatedDate == default(DateTime))
            {
                summary.CreatedDate = DateTime.Now;
            }
            summary.UserId = userId;
            var stats = await OrderService.GetStorefrontOrderStatsByUserIdAsync(userId);
            summary.TotalOrderCount = stats.TotalOrderCount;
            summary.TotalPaid = stats.TotalPaid;
            return summary;
        }

        private async Task FillCustomerSummaryInViewBag()
        {
            ViewBag.CustomerSummary = await BuildCustomerSummaryAsync();
        }

        // Must stay synchronous: invoked via Html.Action child requests (MVC does not support async child actions).
        public ActionResult WebSiteAddressInfo(bool isMobilePage = false)
        {
            var item = new SettingLayoutViewModel();
            item.isMobilePage = isMobilePage;
            item.WebSiteCompanyPhoneAndLocation = SettingService.GetSettingValueDtoByKey(Domain.Constants.WebSiteCompanyPhoneAndLocation);
            item.WebSiteCompanyEmailAddress = SettingService.GetSettingValueDtoByKey(Domain.Constants.WebSiteCompanyEmailAddress);
            return PartialView("_WebSiteAddressInfo", item);
        }

        private void InformCustomerToFillOutForm(CustomerDto customer)
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

        public async Task<ActionResult> SendMessageToSeller()
        {
            ViewBag.Title = Resource.SendMessageToSeller;
            var customer = await BuildCustomerSummaryAsync();
            var faqs = await FaqService.GetStorefrontFaqSummariesAsync(CurrentLanguage);
            return View(new SendMessageToSellerViewModel() { Customer = customer, Faqs = faqs });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendSellerMessage(ContactUsFormViewModel contact)
        {
            if (contact == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                contact.ItemType = EImeceItemType.Ticket;
                contact.IPAddress = HttpContext.Request.UserHostAddress;
                await RazorEngineHelper.SendMessageToSellerAsync(contact);
                TempData["SuccessMessage"] = Resource.YourMessageHasBeenSentToSeller;
                return RedirectToAction("SendMessageToSeller");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resource.EmailSendingFailed;
                return RedirectToAction("SendMessageToSeller");
            }
        }

        public async Task<ActionResult> Faq()
        {
            ViewBag.Title = Resource.Faq;
            var customer = await BuildCustomerSummaryAsync();
            var faqs = await FaqService.GetStorefrontFaqSummariesAsync(CurrentLanguage);
            return View(new SendMessageToSellerViewModel() { Customer = customer, Faqs = faqs });
        }

        public async Task<ActionResult> CustomerOrders(string search = "")
        {
            ViewBag.Title = Resource.CustomerDetail;
            var customer = await BuildCustomerSummaryAsync();
            var userId = User.Identity.GetUserId();
            var orders = (await OrderService.GetStorefrontOrderListByUserIdAsync(userId, search)).OrderByDescending(r=>r.CreatedDate).ToList();
            return View(new CustomerOrdersViewModel() { Customer = customer, Orders = orders });
        }

        public async Task<ActionResult> CustomerOrderDetail(int id)
        {
            ViewBag.Title = Resource.CustomerDetail;
            var customer = await BuildCustomerSummaryAsync();
            var order = await OrderService.GetStorefrontOrderByIdAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(new CustomerOrderDetailViewModel() { Customer = customer, Order = order });
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
        public async Task<ActionResult> ChangePassword()
        {
            ViewBag.Title = Resource.Password;
            ViewBag.CustomerSummary = await BuildCustomerSummaryAsync();
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