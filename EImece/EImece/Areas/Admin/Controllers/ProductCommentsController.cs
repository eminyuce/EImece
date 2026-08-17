using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductCommentsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, int? id, String search = "")
        {
            if (!id.HasValue)
            {
                SetErrorMessage("Ürün yorumları bir ürün kaydı üzerinden açılmalıdır.");
                return RedirectToAction("Index", "Products");
            }

            var productComments = await ProductCommentService.GetAdminPageListAsync(id.Value, search, CurrentLanguage, cancellationToken);
            ViewBag.Product = await ProductService.GetSingleAsync(id.Value);
            return View(productComments);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, int? id, String search = "")
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Products");
            }

            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { id = id.Value, search });
            }

            var productComments = await ProductCommentService.GetAdminPageListAsync(id.Value, search, CurrentLanguage, cancellationToken);
            return new QueryableResult<ProductComment>(productComments.AsQueryable());
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
                return RedirectToAction("Index", new { id = productId });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, productComment);
                SetErrorMessage();
                return RedirectToAction("Index", new { id = productComment.ProductId });
            }
        }
    }
}
