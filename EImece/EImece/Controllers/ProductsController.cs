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
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.ProductsControllerRoutingPrefix)]
    public class ProductsController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IProductCommentService productCommentService;

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
            Logger.Info("ProductsController constructor called. Initializing ProductCommentService.");
            this.productCommentService = ProductCommentService;
        }

        public ActionResult Index()
        {
            Logger.Info("Entering Index action.");
            Logger.Info("Returning BadRequest status.");
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // Uncomment and add logs if needed
        // [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        // public ActionResult Index(int page = 1)
        // {
        //     Logger.Info($"Entering Index action with page: {page}");
        //     var products = ProductService.GetMainPageProducts(page, CurrentLanguage);
        //     Logger.Info($"Retrieved {products?.Records?.Count ?? 0} products for page: {page}, language: {CurrentLanguage}");
        //     Logger.Info("Returning Index view.");
        //     return View(products);
        // }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult AdvancedSearchProducts(String search = "", string filters = "", String page = "")
        {
            Logger.Info($"Entering AdvancedSearchProducts with search: '{search}', filters: '{filters}', page: '{page}'");
            var products = ProductService.GetProductsSearchResult(search, filters, page, CurrentLanguage);
            Logger.Info($"Retrieved {products.Products.Count} products for search: '{search}', language: {CurrentLanguage}");
            Logger.Info("Returning AdvancedSearchProducts view.");
            return View(products);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Detail(String id, int page = 0)
        {
            Logger.Info($"Entering Detail action with id: '{id}'");
            if (String.IsNullOrEmpty(id))
            {
                Logger.Error("Product ID is null or empty.");
                Logger.Info("Redirecting to BadRequest error page.");
                return RedirectToAction("BadRequest", "Error");
            }
            try
            {
                var productId = id.GetId();
                Logger.Info($"Parsed product ID: {productId}");
                var product = ProductService.GetProductDetailViewModelById(productId);
                string fullPath = Request.Path;

                Logger.Info($"Retrieved product details for ID: {productId}, Name: {product?.Product?.Name}, IsActive: {product?.Product?.IsActive}");

                if (!product.Product.IsActive)
                {
                    Logger.Info($"Product with ID: {productId} is inactive. Redirecting to NotFound error page.");
                    return RedirectToAction("NotFound", "Error");
                }
                ViewBag.SeoId = product.Product.GetSeoUrl();
                product.Page = page;
                product.RecordPerPage = AppConfig.ProductCommentsRecordPerPage;
                product.SeoId = product.Product.GetSeoUrl();
                SetCurrentCulture(product.Product);

                Logger.Info($"Set culture and SEO ID: {ViewBag.SeoId} for product ID: {productId}");
                Logger.Info("Returning Detail view.");
                return View(product);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Exception in Detail action for id: '{id}'. Message: {e.Message}");
                Logger.Info("Redirecting to InternalServerError error page.");
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        [Route(Constants.ProductTagPrefix)]
        public ActionResult Tag(String id, int page = 1, int sorting = 0)
        {
            Logger.Info($"Entering Tag action with id: '{id}', page: {page}, sorting: {sorting}");
            if (String.IsNullOrEmpty(id))
            {
                Logger.Error("Tag ID is null or empty.");
                Logger.Info("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var tagId = id.GetId();
            Logger.Info($"Parsed tag ID: {tagId}");
            int pageSize = AppConfig.ProductDefaultRecordPerPage;
            Logger.Info($"Using page size: {pageSize}");

            SimiliarProductTagsViewModel products = ProductService.GetProductByTagId(tagId, page, pageSize, CurrentLanguage, (SortingType)sorting);
            Logger.Info($"Retrieved products for tag ID: {tagId}, page: {page}, language: {CurrentLanguage}");

            products.Page = page;
            products.RecordPerPage = pageSize;
            products.Sorting = (SortingType)sorting;
            products.TagId = id;
            ViewBag.SeoId = products.Tag.GetSeoUrl();
            Logger.Info($"Set model properties: Page={page}, RecordPerPage={pageSize}, Sorting={(SortingType)sorting}, TagId={id}, SeoId={ViewBag.SeoId}");

            SetCurrentCulture(products.Tag);
            Logger.Info("Set current culture based on tag.");
            Logger.Info("Returning Tag view.");
            return View(products);
        }

        [Route(Constants.SearchProductPrefix)]
        public ActionResult SearchProducts(String search, int page = 1, int sorting = 0)
        {
            Logger.Info($"Entering SearchProducts with search: '{search}', page: {page}, sorting: {sorting}");
            if (String.IsNullOrEmpty(search))
            {
                Logger.Error("Search term is null or empty.");
                Logger.Info("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int pageSize = AppConfig.ProductDefaultRecordPerPage;
            Logger.Info($"Using page size: {pageSize}");

            var products = ProductService.SearchProducts(page, pageSize, search, CurrentLanguage, (SortingType)sorting);
            Logger.Info($"Retrieved {products.Products.Count} products for search: '{search}', page: {page}, language: {CurrentLanguage}");

            products.RecordPerPage = pageSize;
            products.Page = page;
            products.Sorting = (SortingType)sorting;
            Logger.Info($"Set model properties: RecordPerPage={pageSize}, Page={page}, Sorting={(SortingType)sorting}");

            Logger.Info("Returning SearchProducts view.");
            return View(products);
        }

        [HttpPost]
        public ActionResult Review(ProductComment productComment)
        {
            Logger.Info($"Entering Review POST action with productComment Email: {productComment?.Email}, ProductId: {productComment?.ProductId}");
            if (productComment == null)
            {
                Logger.Error("ProductComment is null.");
                Logger.Info("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Logger.Info($"Looking up user by email: {productComment.Email}");
            var users = ApplicationDbContext.Users.AsQueryable();
            var user = users.Where(u => u.UserName.Equals(productComment.Email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            Logger.Info($"User found: {(user != null ? $"ID: {user.Id}" : "None")}");

            productComment.UserId = user == null ? "" : user.Id;
            productComment.CreatedDate = DateTime.Now;
            productComment.UpdatedDate = DateTime.Now;
            productComment.IsActive = false;
            productComment.Position = 1;
            productComment.Lang = CurrentLanguage;
            Logger.Info($"Set productComment properties: UserId={productComment.UserId}, CreatedDate={productComment.CreatedDate}, Lang={productComment.Lang}");

            productCommentService.SaveOrEditEntity(productComment);
            Logger.Info($"Saved product comment with ID: {productComment.Id}");

            Logger.Info($"Redirecting to Detail action with SeoUrl: {productComment.SeoUrl}");
            return RedirectToAction("Detail", new { id = productComment.SeoUrl });
        }
    }
}