using EImece.Domain.DbContext;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class CustomersController : BaseAdminController
    {
        [Inject]
        public UsersService UsersService { get; set; }

        [Inject]
        public ApplicationSignInManager SignInManager { get; set; }

        [Inject]
        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        public ActionResult Index(String search = "")
        {
            var model = CustomerService.GetCustomerServices(search);
            return View(model);
        }

        public ActionResult CustomerOrders(string id, string search = "")
        {
            var orders = OrderService.GetOrdersUserId(id, search);
            ViewBag.Customer = CustomerService.GetUserId(id);
            return View(orders);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        public ActionResult DeleteConfirmed(string id)
        {
            UsersService.DeleteUser(id);
            CustomerService.DeleteByUserId(id);
            OrderService.DeleteByUserId(id);
            return RedirectToAction("Index");
        }
    }
}