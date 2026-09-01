using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using EImece.Web.Controllers;
using EImece.Web.Filters;
using EImece.Web.Services;
using Microsoft.Extensions.Logging;
using System;
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
        private const string ErrorKey = "Error";
        private readonly IProductCommentService productCommentService;
        private readonly IProductService ProductService;
        private readonly IAddressService AddressService;
        private readonly ICustomerService CustomerService;
        private readonly IUsersService UsersService;

        public ProductsController(ISettingService settingService,
            AutoMapper.IMapper mapper,
            IProductCommentService productCommentService,
            IProductService productService,
            IAddressService addressService,
            ICustomerService customerService,
            IUsersService usersService, ILogger<ProductsController> logger)
            : base(settingService, mapper, logger)
        {
            Logger.LogDebug("ProductsController constructor called. Initializing dependencies.");
            this.productCommentService = productCommentService ?? throw new ArgumentNullException(nameof(productCommentService));
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
            AddressService = addressService ?? throw new ArgumentNullException(nameof(addressService));
            CustomerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            UsersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        }

        /// <summary>
        /// Product listing page. The action returns Task&lt;ActionResult&gt; and awaits the service, so
        /// the request thread goes back to the ASP.NET thread pool while SQL Server runs the COUNT
        /// and the page query, instead of sitting blocked inside ToList().
        /// </summary>
        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, int page = 1)
        {
            Logger.LogDebug($"Entering Index action with page: {page}, language: {CurrentLanguage}");

            if (page < 1)
            {
                Logger.LogError($"Invalid page number: {page}. Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // No ConfigureAwait(false) here on purpose: the continuation needs HttpContext.Current
            // and the request culture to render the view. ConfigureAwait(false) belongs in the
            // service and repository layers, which is where it is used.
            var products = await ProductService.GetMainPageProductsAsync(page, CurrentLanguage, cancellationToken);

            products.Page = page;
            products.RecordPerPage = AppConfig.RecordPerPage;

            Logger.LogDebug($"Retrieved {products.Products.Count} of {products.Products.TotalCount} products for page: {page}");
            Logger.LogDebug("Returning Index view.");
            return View(products);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        [RateLimit("search", DefaultLimit = 30, DefaultWindowMinutes = 1)]
        public async Task<ActionResult> AdvancedSearchProducts(CancellationToken cancellationToken, String search = "", string filters = "", String page = "")
        {
            Logger.LogDebug($"Entering AdvancedSearchProducts with search: '{search}', filters: '{filters}', page: '{page}'");
            var products = await ProductService.GetProductsSearchResultAsync(search, filters, page, CurrentLanguage, cancellationToken);
            Logger.LogDebug($"Retrieved {products.Products.Count} products for search: '{search}', language: {CurrentLanguage}");
            Logger.LogDebug("Returning AdvancedSearchProducts view.");
            return View(products);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Detail(CancellationToken cancellationToken, String id, int page = 0)
        {
            Logger.LogDebug($"Entering Detail action with id: '{id}'");
            if (String.IsNullOrEmpty(id))
            {
                Logger.LogError("Product ID is null or empty.");
                Logger.LogDebug("Redirecting to BadRequest error page.");
                return RedirectToAction("BadRequest", ErrorKey);
            }
            try
            {
                var productId = id.GetId();
                Logger.LogDebug($"Parsed product ID: {productId}");
                var product = await ProductService.GetProductDetailViewModelByIdAsync(productId, cancellationToken);
                string fullPath = Request.Path;

                Logger.LogDebug($"Retrieved product details for ID: {productId}, Name: {product?.ProductDto?.Name}, IsActive: {product?.ProductDto?.IsActive}");

                if (product == null || product.ProductDto == null)
                {
                    Logger.LogInformation($"Product with ID: {productId} was not found in database. Returning 404 NotFound.");
                    return HttpNotFoundView();
                }
                if (!product.ProductDto.IsActive)
                {
                    Logger.LogInformation($"Product with ID: {productId} is inactive. Returning 410 Gone.");
                    return HttpGoneView(Resources.Resource.NotFoundText);
                }
                if (product.ProductDto.ProductCategoryId <= 0)
                {
                    Logger.LogInformation($"ProductCategory for product ID: {productId} is invalid. Returning 404 NotFound.");
                    return HttpNotFoundView();
                }
                ViewBag.SeoId = product.ProductDto.SeoUrl;
                product.Page = page;
                product.RecordPerPage = AppConfig.ProductCommentsRecordPerPage;
                product.SeoId = product.ProductDto.SeoUrl;

                Logger.LogDebug($"Set SEO ID: {ViewBag.SeoId} for product ID: {productId}");
                Logger.LogDebug("Returning Detail view.");
                return View(product);
            }
            catch (ArgumentNullException ex)
            {
                Logger.LogInformation(ex, "Product not found for id '{0}'. Returning 404 NotFound.", id);
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
            Logger.LogDebug($"Entering Tag action with id: '{id}', page: {page}, sorting: {sorting}");
            if (String.IsNullOrEmpty(id))
            {
                Logger.LogError("Tag ID is null or empty.");
                Logger.LogDebug("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var tagId = id.GetId();
            Logger.LogDebug($"Parsed tag ID: {tagId}");
            int pageSize = AppConfig.ProductDefaultRecordPerPage;
            Logger.LogDebug($"Using page size: {pageSize}");

            SimiliarProductTagsViewModel products = await ProductService.GetProductByTagIdAsync(tagId, page, pageSize, CurrentLanguage, (SortingType)sorting, cancellationToken);
            Logger.LogDebug($"Retrieved products for tag ID: {tagId}, page: {page}, language: {CurrentLanguage}");

            products.Page = page;
            products.RecordPerPage = pageSize;
            products.Sorting = (SortingType)sorting;
            products.TagId = id;
            ViewBag.SeoId = products.Tag.GetSeoUrl();
            Logger.LogDebug($"Set model properties: Page={page}, RecordPerPage={pageSize}, Sorting={(SortingType)sorting}, TagId={id}, SeoId={ViewBag.SeoId}");
            Logger.LogDebug("Returning Tag view.");
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
            Logger.LogDebug($"Entering SearchProducts with search: '{search}', page: {page}, sorting: {sorting}");
            search = search?.Trim() ?? string.Empty;
            int pageSize = AppConfig.ProductDefaultRecordPerPage;
            Logger.LogDebug($"Using page size: {pageSize}");

            var products = await ProductService.SearchProductsAsync(page, pageSize, search, CurrentLanguage, (SortingType)sorting, cancellationToken);
            Logger.LogDebug($"Retrieved {products.Products.Count} products for search: '{search}', page: {page}, language: {CurrentLanguage}");

            products.RecordPerPage = pageSize;
            products.Page = page;
            products.Sorting = (SortingType)sorting;
            Logger.LogDebug($"Set model properties: RecordPerPage={pageSize}, Page={page}, Sorting={(SortingType)sorting}");

            Logger.LogDebug("Returning SearchProducts view.");
            return View("SearchProducts", products);
        }

        [HttpPost]
        [ValidateCaptcha(Prefix = "ProductReview")]
        public async Task<ActionResult> Review(ProductCommentDto productComment)
        {
            Logger.LogDebug($"Entering Review POST action with productComment Email: {productComment?.Email}, ProductId: {productComment?.ProductId}");
            if (productComment == null)
            {
                Logger.LogError("ProductComment is null.");
                Logger.LogDebug("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
                var message = firstError ?? CaptchaService.GetErrorMessage();
                Logger.LogError($"Product review validation failed: {message}");
                TempData["ReviewFormError"] = message;
                TempData["RecaptchaError"] = message;
                Logger.LogDebug($"Redirecting to Detail action with SeoUrl: {productComment.SeoUrl}");
                return RedirectToAction("Detail", new { id = productComment.SeoUrl });
            }

            Logger.LogDebug($"Looking up user by email: {productComment.Email}");

            // Compared with == rather than String.Equals(StringComparison): EF6 cannot translate the
            // StringComparison overloads, and the database collation is already case-insensitive.
            var email = productComment.Email.ToStr().Trim();
            var user = await UsersService.GetUserByEmailAsync(email);
            Logger.LogDebug($"User found: {(user != null ? $"ID: {user.Id}" : "None")}");

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
            Logger.LogInformation($"Saved product comment with ID: {entity.Id}");

            Logger.LogDebug($"Redirecting to Detail action with SeoUrl: {productComment.SeoUrl}");
            return RedirectToAction("Detail", new { id = productComment.SeoUrl });
        }
    }
}