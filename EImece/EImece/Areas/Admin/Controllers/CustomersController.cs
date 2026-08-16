using EImece.Domain.DbContext;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            var model = await CustomerService.GetCustomerServicesAsync(search);
            return View(model);
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcel(CancellationToken cancellationToken, string format = "excel", string search = "")
        {
            var customers = await CustomerService.GetCustomerServicesAsync(search ?? "");
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

        public async Task<ActionResult> CustomerOrders(CancellationToken cancellationToken, string id, string search = "")
        {
            var orders = await OrderService.GetOrdersUserIdAsync(id, search);
            var customer = await CustomerService.GetUserIdAsync(id);
            orders.ForEach(r => r.Customer = customer);
            ViewBag.Customer = customer;
            return View(orders.OrderByDescending(r => r.UpdatedDate).ToList());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, string id)
        {
            await CustomerService.DeleteCustomersAsync(new List<string> { id }, User?.Identity?.GetUserId());
            SetSuccessMessage();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> CustomerBaskets(CancellationToken cancellationToken)
        {
            var baskets = (await ShoppingCartService.GetAllAsync()).OrderByDescending(r => r.CreatedDate).ToList();
            return View(baskets);
        }

        public async Task<ActionResult> DeleteAllShoppingCartSessions(CancellationToken cancellationToken)
        {
            var baskets = await ShoppingCartService.GetAllAsync();
            foreach (var item in baskets)
            {
                await ShoppingCartService.DeleteByIdAsync(item.Id);
            }
            SetSuccessMessage();
            return RedirectToAction("CustomerBaskets");
        }
    }
}