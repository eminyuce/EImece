using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Filters;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductCommentsController : BaseAdminController
    {
        private const string IsoDateFormat = "yyyy-MM-dd";

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected IProductCommentService ProductCommentService { get; }
        protected IProductService ProductService { get; }

        public ProductCommentsController(
            ISettingService settingService,
            IProductCommentService productCommentService,
            IProductService productService)
            : base(settingService)
        {
            ProductCommentService = productCommentService ?? throw new ArgumentNullException(nameof(productCommentService));
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, int? id, string search = "", int[] rating = null, string ratings = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var selectedRatings = ProductCommentAdminListHelper.ParseRatings(rating, ratings);
            var productId = NormalizeProductId(id);
            NormalizeDateRange(ref startDate, ref endDate);
            var productComments = await ProductCommentService.GetAdminPageListAsync(productId, search, CurrentLanguage, selectedRatings, startDate, endDate, cancellationToken);
            if (productId.HasValue)
            {
                ViewBag.Product = await ProductService.GetSingleAsync(productId.Value);
            }
            SetFilterViewBag(selectedRatings, startDate, endDate);
            return View(productComments);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, int? id, string search = "", int[] rating = null, string ratings = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var selectedRatings = ProductCommentAdminListHelper.ParseRatings(rating, ratings);
            var productId = NormalizeProductId(id);
            NormalizeDateRange(ref startDate, ref endDate);
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { id = productId, search, ratings = FormatRatings(selectedRatings), startDate = FormatDate(startDate), endDate = FormatDate(endDate) });
            }

            var productComments = await ProductCommentService.GetAdminPageListAsync(productId, search, CurrentLanguage, selectedRatings, startDate, endDate, cancellationToken);
            return new QueryableResult<ProductComment>(productComments.AsQueryable());
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcel(CancellationToken cancellationToken, int? id, string search = "", int[] rating = null, string ratings = null, DateTime? startDate = null, DateTime? endDate = null, string format = "excel")
        {
            var selectedRatings = ProductCommentAdminListHelper.ParseRatings(rating, ratings);
            var productId = NormalizeProductId(id);
            NormalizeDateRange(ref startDate, ref endDate);
            var productComments = await ProductCommentService.GetAdminPageListAsync(productId, search, CurrentLanguage, selectedRatings, startDate, endDate, cancellationToken);
            var result = from r in productComments
                         select new
                         {
                             ProductName = r.Product != null ? r.Product.Name : "",
                             ProductCode = r.Product != null ? r.Product.ProductCode : "",
                             r.ProductId,
                             r.Subject,
                             r.Name,
                             r.Email,
                             r.Rating,
                             r.Review,
                             r.IsActive,
                             r.CreatedDate,
                             r.UpdatedDate
                         };

            return DownloadFile(result, string.Format("product-comments-{0}", GetCurrentLanguage), format);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            var productComment = await ProductCommentService.GetSingleAsync(id);
            if (productComment == null)
            {
                return HttpNotFound();
            }
            try
            {
                var productId = productComment.ProductId;
                await ProductCommentService.DeleteEntityAsync(productComment);
                SetSuccessMessage();
                return RedirectToCommentsList(productId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, productComment);
                SetErrorMessage();
                return RedirectToCommentsList(productComment.ProductId);
            }
        }

        private ActionResult RedirectToCommentsList(int productId)
        {
            var referrer = Request?.UrlReferrer;
            if (referrer != null)
            {
                var path = referrer.AbsolutePath ?? string.Empty;
                if (path.IndexOf("/productcomments/index/" + productId, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return RedirectToAction("Index", new { id = productId });
                }
            }

            return RedirectToAction("Index");
        }

        private void SetFilterViewBag(IList<int> selectedRatings, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.SelectedRatings = selectedRatings ?? new List<int>();
            ViewBag.StartDate = FormatDate(startDate);
            ViewBag.EndDate = FormatDate(endDate);
        }

        private static void NormalizeDateRange(ref DateTime? startDate, ref DateTime? endDate)
        {
            if (startDate.HasValue)
            {
                startDate = startDate.Value.Date;
            }
            if (endDate.HasValue)
            {
                endDate = endDate.Value.Date;
            }
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                var swap = startDate;
                startDate = endDate;
                endDate = swap;
            }
        }

        private static string FormatDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString(IsoDateFormat, CultureInfo.InvariantCulture) : "";
        }

        private static string FormatRatings(IList<int> ratings)
        {
            if (ratings == null || ratings.Count == 0)
            {
                return null;
            }

            return string.Join(",", ratings);
        }

        private static int? NormalizeProductId(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return null;
            }

            return id;
        }
    }
}
