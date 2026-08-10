using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class OrdersController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                Logger.Error(ex, "Unable to delete order:" + ex.StackTrace, item);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }
    }
}