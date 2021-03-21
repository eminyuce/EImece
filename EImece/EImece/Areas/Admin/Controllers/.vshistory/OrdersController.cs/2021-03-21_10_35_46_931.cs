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
    public class BuyNowOrdersController : BaseAdminController
    {

        // GET: Admin/BuyNowOrders
        public ActionResult Index()
        {
            var orders = OrderService.GetAll();
            return View();
        }
    }
}