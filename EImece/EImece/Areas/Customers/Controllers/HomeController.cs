using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using System.Web.Mvc;

namespace EImece.Areas.Customers.Controllers
{
    [AuthorizeRoles(Domain.Constants.CustomerRole)]
    public class HomeController : Controller
    {
        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }
        [Inject]
        public ICustomerService CustomerService { get; set; }
        [Inject]
        public IOrderService OrderService { get; set; }
        public ApplicationUserManager UserManager { get; set; }
        public HomeController(ApplicationUserManager userManager)
        {
            this.UserManager = userManager;
        }
        // GET: Customers/Home
        public ActionResult Index()
        {
            return View();
        } 
        public ActionResult SendMessageToSeller()
        {
            return View();
        }
        public ActionResult CustomerOrders(string search = "")
        {
            var user = UserManager.FindByName(User.Identity.GetUserName());
            var orders = OrderService.GetOrdersUserId(user.Id, search);
            return View(orders);
        }
        public ActionResult OrderDetail(string id)
        {
            var user = UserManager.FindByName(User.Identity.GetUserName());
            var order = OrderService.GetByOrderGuid(id);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home", new { @area = "" });
        }
    }
}