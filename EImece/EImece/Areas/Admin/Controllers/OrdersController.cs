using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class BuyNowOrdersController : BaseAdminController
    {
        // GET: Admin/BuyNowOrders
        public ActionResult Index()
        {
            var orders = OrderService.GetAll();
            return View(orders);
        }
    }
}