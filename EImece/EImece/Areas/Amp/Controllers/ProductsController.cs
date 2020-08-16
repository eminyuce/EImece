using EImece.Controllers;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Services.IServices;
using Ninject;
using System.Web.Mvc;

namespace EImece.Areas.Amp.Controllers
{
    public class ProductsController : BaseController
    {
        [Inject]
        public IProductService ProductService { get; set; }

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