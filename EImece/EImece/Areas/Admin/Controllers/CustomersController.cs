using EImece.Domain.DbContext;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
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
        public new ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IShoppingCartService ShoppingCartService { get; set; }

        public CustomersController(ICustomerService customerService)
        {
            this.CustomerService = customerService;
        }

        public ActionResult Index(String search = "")
        {
            var model = CustomerService.GetCustomerServices(search);
            return View(model);
        }

        [HttpGet, ActionName("ExportExcel")]
        public ActionResult ExportExcel(string format = "excel", string search = "")
        {
            var customers = CustomerService.GetCustomerServices(search ?? "");
            var result = from r in customers
                         select new
                         {
                             r.Name,
                             r.Surname,
                             r.Email,
                             Phone = r.GsmNumber,
                             r.Gender,
                             Address = r.RegistrationAddress,
                             OrderCount = r.Orders != null ? r.Orders.Count : 0,
                             r.CreatedDate
                         };

            return DownloadFile(result, string.Format("customers-{0}", GetCurrentLanguage), format);
        }

        public ActionResult CustomerOrders(string id, string search = "")
        {
            var orders = OrderService.GetOrdersUserId(id, search);
            var customer = CustomerService.GetUserId(id);
            orders.ForEach(r => r.Customer = customer);
            ViewBag.Customer = customer;
            return View(orders.OrderByDescending(r => r.UpdatedDate).ToList());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        public ActionResult DeleteConfirmed(string id)
        {
            UsersService.DeleteUser(id);
            CustomerService.DeleteByUserId(id);
            OrderService.DeleteByUserId(id);
            SetSuccessMessage();
            return RedirectToAction("Index");
        }

        public ActionResult CustomerBaskets()
        {
            var baskets = ShoppingCartService.GetAll().OrderByDescending(r => r.CreatedDate).ToList();
            return View(baskets);
        }

        public ActionResult DeleteAllShoppingCartSessions()
        {
            var baskets = ShoppingCartService.GetAll();
            foreach (var item in baskets)
            {
                ShoppingCartService.DeleteById(item.Id);
            }
            SetSuccessMessage();
            return RedirectToAction("CustomerBaskets");
        }
    }
}