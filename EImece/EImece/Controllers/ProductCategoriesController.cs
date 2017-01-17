using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class ProductCategoriesController : BaseController
    {
        // GET: ProductCategory
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Category(String id)
        {
            var categoryId = id.GetId();

            var productCategory = ProductCategoryService.GetProductCategory(categoryId);
            return View(productCategory);
        }
    }
}