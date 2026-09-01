using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using EImece.Web.Areas.Admin.Controllers;
using EImece.Web.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class OrdersController : BaseAdminController
    {
        protected IOrderService OrderService { get; }

        public OrdersController(ISettingService settingService,
            IOrderService orderService, ILogger<OrdersController> logger)
            : base(settingService, logger)
        {
            OrderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        // GET: Admin/BuyNowOrders
        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Order, bool>> whereLambda = r =>
            r.Name.Contains(search)
            || r.OrderNumber.Contains(search)
              || r.Token.Contains(search);
            var orders = await OrderService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(orders);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<Order, bool>> whereLambda = r =>
            r.Name.Contains(search)
            || r.OrderNumber.Contains(search)
              || r.Token.Contains(search);
            var orders = await OrderService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return AdminGridResult(orders);
        }

        public async Task<ActionResult> Details(CancellationToken cancellationToken, int id)
        {
            var order = await OrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            var item = await OrderService.GetSingleAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            try
            {
                await OrderService.DeleteOrderByIdAsync(id);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to delete order:" + ex.StackTrace, item);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }
    }
}