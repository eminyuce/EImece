using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using Microsoft.AspNet.Identity;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Globalization;

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