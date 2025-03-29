using EImece.Domain.Entities;
using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class OrdersController : BaseAdminController
    {
        // GET: Admin/BuyNowOrders
        public ActionResult Index(String search = "")
        {
            Expression<Func<Order, bool>> whereLambda = r =>
            r.Name.Contains(search)
            || r.OrderNumber.Contains(search)
            || r.OrderNumber.Contains(search)
              || r.Token.Contains(search);
            var orders = OrderService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(orders);
        }

        public ActionResult Details(int id)
        {
            var order = OrderService.GetOrderById(id); // Fetch order by ID
            return View(order);
        }
    }
}