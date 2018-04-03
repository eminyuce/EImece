using EImece.Domain;
using EImece.Domain.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.Extensions;

namespace EImece.Areas.Amp.Controllers
{
    public class ProductsController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }
        // GET: Amp/Products
        public ActionResult Detail(string id)
        {
            var productId = id.GetId();
            var product = ProductService.GetProductById(productId);
            ViewBag.SeoId = product.Product.GetSeoUrl();

            return View(product);
        }
    }
}