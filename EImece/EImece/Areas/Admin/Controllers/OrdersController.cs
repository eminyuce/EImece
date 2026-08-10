using EImece.Domain.Entities;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class OrdersController : BaseAdminController
    {
        // GET: Admin/BuyNowOrders
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Order, bool>> whereLambda = r =>
            r.Name.Contains(search)
            || r.OrderNumber.Contains(search)
              || r.Token.Contains(search);
            var orders = await OrderService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(orders);
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
    }
}