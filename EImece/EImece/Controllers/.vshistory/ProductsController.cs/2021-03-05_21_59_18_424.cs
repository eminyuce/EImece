using EImece.Domain;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using Resources;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.ProductsControllerRoutingPrefix)]
    public class ProductsController : BasePaymentController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IProductCommentService productCommentService;
        protected BuyNowModel BuyNowSession
        {
            get
            {
                return (BuyNowModel)Session["BuyNowSession"];
            }
            set
            {
                Session["BuyNowSession"] = value;
            }
        }

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IyzicoService IyzicoService { get; set; }
        [Inject]
        public IAddressService AddressService { get; set; }
        [Inject]
        public ICustomerService CustomerService { get; set; }


        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        public ProductsController(IProductCommentService ProductCommentService)
        {
            this.productCommentService = ProductCommentService;
        }

        public ActionResult Index()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        //       [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        //       public ActionResult Index(int page = 1)
        //       {
        //           var products = ProductService.GetMainPageProducts(page, CurrentLanguage);
        //           return View(products);
        //       }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult AdvancedSearchProducts(String search = "", string filters = "", String page = "")
        {
            var products = ProductService.GetProductsSearchResult(search, filters, page, CurrentLanguage);
            return View(products);
        }
        public ActionResult BuyNow(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var productId = id.GetId();
                var product = ProductService.GetProductDetailViewModelById(productId);
                ViewBag.SeoId = product.Product.GetSeoUrl();
                BuyNowModel buyNowModel = new BuyNowModel();
                buyNowModel.ProductDetailViewModel = product;
                return View(buyNowModel);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Products.BuyNow page");
                return RedirectToAction("InternalServerError", "Error");
            }
        }
      

        [HttpPost]
        public ActionResult BuyNow(String productId, Customer customer)
        {
            bool isValidCustomer = customer != null && customer.isValidCustomer();
            BuyNowModel buyNowModel = new BuyNowModel();
            buyNowModel.ProductId = GeneralHelper.RevertId(productId);
            buyNowModel.ProductDetailViewModel = ProductService.GetProductDetailViewModelById(buyNowModel.ProductId);
            buyNowModel.Customer = customer;

            if (isValidCustomer)
            {
             
                buyNowModel.Customer = customer;
                BuyNowSession.ShippingAddress = SetAddress(customer, buyNowModel.ShippingAddress);
                buyNowModel.ShippingAddress.AddressType = (int)AddressType.ShippingAddress;
                BuyNowSession = buyNowModel;
                ViewBag.CheckoutFormInitialize = IyzicoService.CreateCheckoutFormInitializeBuyNow(buyNowModel);
                return View(buyNowModel);
            }
            else
            {
                InformCustomerToFillOutForm(customer);
                return View(buyNowModel);
            }
               
        }
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Detail(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var productId = id.GetId();
                var product = ProductService.GetProductDetailViewModelById(productId);
                ViewBag.SeoId = product.Product.GetSeoUrl();
                return View(product);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Products.Detail page");
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        [Route(Constants.ProductTagPrefix)]
        public ActionResult Tag(String id, int page = 1, int sorting = 0)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var tagId = id.GetId();
            int pageSize = AppConfig.ProductDefaultRecordPerPage;
            SimiliarProductTagsViewModel products = ProductService.GetProductByTagId(tagId, page, pageSize, CurrentLanguage, (SortingType)sorting);
            products.Page = page;
            products.RecordPerPage = pageSize;
            products.Sorting = (SortingType)sorting;
            products.TagId = id;
            ViewBag.SeoId = products.Tag.GetSeoUrl();
            return View(products);
        }

        [Route(Constants.SearchProductPrefix)]
        public ActionResult SearchProducts(String search, int page = 1, int sorting = 0)
        {
            if (String.IsNullOrEmpty(search))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int pageSize = AppConfig.ProductDefaultRecordPerPage;
            var products = ProductService.SearchProducts(page, pageSize, search, CurrentLanguage, (SortingType)sorting);
            products.RecordPerPage = pageSize;
            products.Page = page;
            products.Sorting = (SortingType)sorting;
            return View(products);
        }

        [HttpPost]
        public ActionResult Review(ProductComment productComment)
        {
            if (productComment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var users = ApplicationDbContext.Users.AsQueryable();
            var user = users.Where(u => u.UserName.Equals(productComment.Email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            productComment.UserId = user == null ? "" : user.Id;
            productComment.CreatedDate = DateTime.Now;
            productComment.UpdatedDate = DateTime.Now;
            productComment.IsActive = false;
            productComment.Position = 1;
            productComment.Lang = CurrentLanguage;
            productCommentService.SaveOrEditEntity(productComment);
            return RedirectToAction("Detail", new { id = productComment.SeoUrl });
        }
    }
}