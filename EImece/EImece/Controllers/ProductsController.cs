using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using EImece.Filters;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.ProductsControllerRoutingPrefix)]
    public class ProductsController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string ErrorKey = "Error";
        private readonly IProductCommentService productCommentService;

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IAddressService AddressService { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IUsersService UsersService { get; set; }

        public ProductsController(IProductCommentService ProductCommentService)
        {
            Logger.Info("ProductsController constructor called. Initializing ProductCommentService.");
            this.productCommentService = ProductCommentService;
        }

        /// <summary>
        /// Product listing page. The action returns Task&lt;ActionResult&gt; and awaits the service, so
        /// the request thread goes back to the ASP.NET thread pool while SQL Server runs the COUNT
        /// and the page query, instead of sitting blocked inside ToList().
        /// </summary>
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, int page = 1)
        {
            Logger.Info($"Entering Index action with page: {page}, language: {CurrentLanguage}");

            if (page < 1)
            {
                Logger.Error($"Invalid page number: {page}. Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // No ConfigureAwait(false) here on purpose: the continuation needs HttpContext.Current
            // and the request culture to render the view. ConfigureAwait(false) belongs in the
            // service and repository layers, which is where it is used.
            var products = await ProductService.GetMainPageProductsAsync(page, CurrentLanguage, cancellationToken);

            products.Page = page;
            products.RecordPerPage = AppConfig.RecordPerPage;

            Logger.Info($"Retrieved {products.Products.Count} of {products.Products.TotalCount} products for page: {page}");
            Logger.Debug("Returning Index view.");
            return View(products);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        [RateLimit("search", DefaultLimit = 30, DefaultWindowMinutes = 1)]
        public async Task<ActionResult> AdvancedSearchProducts(CancellationToken cancellationToken, String search = "", string filters = "", String page = "")
        {
            Logger.Info($"Entering AdvancedSearchProducts with search: '{search}', filters: '{filters}', page: '{page}'");
            var products = await ProductService.GetProductsSearchResultAsync(search, filters, page, CurrentLanguage, cancellationToken);
            Logger.Info($"Retrieved {products.Products.Count} products for search: '{search}', language: {CurrentLanguage}");
            Logger.Info("Returning AdvancedSearchProducts view.");
            return View(products);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Detail(CancellationToken cancellationToken, String id, int page = 0)
        {
            Logger.Info($"Entering Detail action with id: '{id}'");
            if (String.IsNullOrEmpty(id))
            {
                Logger.Error("Product ID is null or empty.");
                Logger.Info("Redirecting to BadRequest error page.");
                return RedirectToAction("BadRequest", ErrorKey);
            }
            try
            {
                var productId = id.GetId();
                Logger.Info($"Parsed product ID: {productId}");
                var product = await ProductService.GetProductDetailViewModelByIdAsync(productId, cancellationToken);
                string fullPath = Request.Path;

                Logger.Info($"Retrieved product details for ID: {productId}, Name: {product?.ProductDto?.Name}, IsActive: {product?.ProductDto?.IsActive}");

                if (product == null || product.ProductDto == null)
                {
                    Logger.Info($"Product with ID: {productId} was not found in database. Returning 404 NotFound.");
                    return HttpNotFoundView();
                }
                if (!product.ProductDto.IsActive)
                {
                    Logger.Info($"Product with ID: {productId} is inactive. Returning 410 Gone.");
                    return HttpGoneView(Resources.Resource.NotFoundText);
                }
                if (product.ProductDto.ProductCategoryId <= 0)
                {
                    Logger.Info($"ProductCategory for product ID: {productId} is invalid. Returning 404 NotFound.");
                    return HttpNotFoundView();
                }
                ViewBag.SeoId = product.ProductDto.SeoUrl;
                product.Page = page;
                product.RecordPerPage = AppConfig.ProductCommentsRecordPerPage;
                product.SeoId = product.ProductDto.SeoUrl;
                SetCurrentCulture(product.ProductDto.Lang);

                Logger.Info($"Set culture and SEO ID: {ViewBag.SeoId} for product ID: {productId}");
                Logger.Info("Returning Detail view.");
                return View(product);
            }
            catch (ArgumentNullException ex)
            {
                Logger.Info(ex, "Product not found for id '{0}'. Returning 404 NotFound.", id);
                return HttpNotFoundView();
            }
            catch (Exception e)
            {
                return HandleUnexpectedError(e, $"Exception in Detail action for id: '{id}'. Message: {e.Message}");
            }
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        [Route(Constants.ProductTagPrefix)]
        public async Task<ActionResult> Tag(CancellationToken cancellationToken, String id, int page = 1, int sorting = 0)
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

            SimiliarProductTagsViewModel products = await ProductService.GetProductByTagIdAsync(tagId, page, pageSize, CurrentLanguage, (SortingType)sorting, cancellationToken);
            Logger.Info($"Retrieved products for tag ID: {tagId}, page: {page}, language: {CurrentLanguage}");

            products.Page = page;
            products.RecordPerPage = pageSize;
            products.Sorting = (SortingType)sorting;
            products.TagId = id;
            ViewBag.SeoId = products.Tag.GetSeoUrl();
            Logger.Info($"Set model properties: Page={page}, RecordPerPage={pageSize}, Sorting={(SortingType)sorting}, TagId={id}, SeoId={ViewBag.SeoId}");

            SetCurrentCulture(products.Tag.Lang);
            Logger.Info("Set current culture based on tag.");
            Logger.Info("Returning Tag view.");
            return View(products);
        }

        [Route(Constants.SearchProductPrefix)]
        [Route("search")]
        [Route("~/products/searchproducts")]
        [Route("~/products/search")]
        [Route("~/products/advancedsearch")]
        [Route("~/products/advancedsearchproducts")]
        [RateLimit("search", DefaultLimit = 30, DefaultWindowMinutes = 1)]
        public async Task<ActionResult> SearchProducts(String search, CancellationToken cancellationToken, int page = 1, int sorting = 0)
        {
            Logger.Info($"Entering SearchProducts with search: '{search}', page: {page}, sorting: {sorting}");
            search = search?.Trim() ?? string.Empty;
            int pageSize = AppConfig.ProductDefaultRecordPerPage;
            Logger.Info($"Using page size: {pageSize}");

            var products = await ProductService.SearchProductsAsync(page, pageSize, search, CurrentLanguage, (SortingType)sorting, cancellationToken);
            Logger.Info($"Retrieved {products.Products.Count} products for search: '{search}', page: {page}, language: {CurrentLanguage}");

            products.RecordPerPage = pageSize;
            products.Page = page;
            products.Sorting = (SortingType)sorting;
            Logger.Info($"Set model properties: RecordPerPage={pageSize}, Page={page}, Sorting={(SortingType)sorting}");

            Logger.Info("Returning SearchProducts view.");
            return View("SearchProducts", products);
        }

        [HttpPost]
        [ValidateCaptcha(Prefix = "ProductReview")]
        public async Task<ActionResult> Review(ProductCommentDto productComment)
        {
            Logger.Info($"Entering Review POST action with productComment Email: {productComment?.Email}, ProductId: {productComment?.ProductId}");
            if (productComment == null)
            {
                Logger.Error("ProductComment is null.");
                Logger.Info("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
                var message = firstError ?? CaptchaService.GetErrorMessage();
                Logger.Error($"Product review validation failed: {message}");
                TempData["ReviewFormError"] = message;
                TempData["RecaptchaError"] = message;
                Logger.Info($"Redirecting to Detail action with SeoUrl: {productComment.SeoUrl}");
                return RedirectToAction("Detail", new { id = productComment.SeoUrl });
            }

            Logger.Info($"Looking up user by email: {productComment.Email}");

            // Compared with == rather than String.Equals(StringComparison): EF6 cannot translate the
            // StringComparison overloads, and the database collation is already case-insensitive.
            var email = productComment.Email.ToStr().Trim();
            var user = await UsersService.GetUserByEmailAsync(email);
            Logger.Info($"User found: {(user != null ? $"ID: {user.Id}" : "None")}");

            var entity = new ProductComment
            {
                ProductId = productComment.ProductId,
                Name = productComment.Name,
                Email = productComment.Email,
                Subject = productComment.Subject,
                Rating = productComment.Rating,
                Review = productComment.Review,
                UserId = user == null ? "" : user.Id,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                IsActive = false,
                Position = 1,
                Lang = CurrentLanguage
            };

            await productCommentService.SaveOrEditEntityAsync(entity);
            Logger.Info($"Saved product comment with ID: {entity.Id}");

            Logger.Info($"Redirecting to Detail action with SeoUrl: {productComment.SeoUrl}");
            return RedirectToAction("Detail", new { id = productComment.SeoUrl });
        }
    }
}