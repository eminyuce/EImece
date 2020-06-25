using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using NLog;
using System;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.ProductsCategoriesControllerRoutingPrefix)]
    public class ProductCategoriesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // GET: ProductCategory
        public ActionResult Index()
        {
            return View();
        }

        [Route("category/{id}")]
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Category(String id)
        {
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var categoryId = id.GetId();

                ProductCategoryViewModel productCategory = ProductCategoryService.GetProductCategoryViewModel(categoryId);
                ViewBag.SeoId = productCategory.ProductCategory.GetSeoUrl();
                return View(productCategory);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " id:" + id);
                return RedirectToAction("InternalServerError", "Error");
            }
        }
    }
}