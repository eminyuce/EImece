using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class ProductsController : BaseController
    {
        // GET: Products
        public ActionResult Index(int page = 1, int lang=1)
        {
            var products = ProductRepository.GetMainPageProducts(page,lang);
            return View(products);
        }
    }
}