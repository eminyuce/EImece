using EImece.Domain.DbContext;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Linq;
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
            var user = ApplicationDbContext.Users.First(u => u.Id == id);
            var orders = OrderService.GetOrdersUserId(user.Id, search);
            ViewBag.Customer = CustomerService.GetUserId(user.Id);
            return View(orders);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        public ActionResult DeleteConfirmed(string id)
        {
            var user = ApplicationDbContext.Users.First(u => u.Id == id);
            ApplicationDbContext.Users.Remove(user);
            ApplicationDbContext.SaveChanges();
            CustomerService.DeleteByUserId(user.Id);
            OrderService.DeleteByUserId(user.Id);
            return RedirectToAction("Index");
        }
    }
}