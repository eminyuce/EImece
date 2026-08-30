using EImece.Domain.Entities;
using EImece.Filters;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using Griddly.Mvc;
using Griddly.Mvc.Results;
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
        protected ICustomerService CustomerService { get; }
        protected IOrderService OrderService { get; }
        protected IShoppingCartService ShoppingCartService { get; }

        public CustomersController(
            ISettingService settingService,
            ICustomerService customerService,
            IOrderService orderService,
            IShoppingCartService shoppingCartService)
            : base(settingService)
        {
            CustomerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            OrderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            ShoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            var model = await CustomerService.GetCustomerServicesAsync(search);
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            var model = await CustomerService.GetCustomerServicesAsync(search);
            return new QueryableResult<Customer>(model.AsQueryable());
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

        [HttpGet]
        public async Task<ActionResult> CustomerOrders(CancellationToken cancellationToken, string id, string search = "")
        {
            var orders = await OrderService.GetOrdersUserIdAsync(id, search);
            var customer = await CustomerService.GetUserIdAsync(id);
            orders.ForEach(r => r.Customer = customer);
            ViewBag.Customer = customer;
            return View(orders.OrderByDescending(r => r.UpdatedDate).ToList());
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> CustomerOrdersGrid(CancellationToken cancellationToken, string id, string search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("CustomerOrders", new { id, search });
            }

            var orders = await OrderService.GetOrdersUserIdAsync(id, search);
            var customer = await CustomerService.GetUserIdAsync(id);
            orders.ForEach(r => r.Customer = customer);
            ViewBag.Customer = customer;
            return new QueryableResult<Order>(orders.OrderByDescending(r => r.UpdatedDate).AsQueryable());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        [DeleteAuthorize]

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