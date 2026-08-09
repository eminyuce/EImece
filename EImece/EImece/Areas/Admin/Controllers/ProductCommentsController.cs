using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using Resources;
using System;
using System.Net;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductCommentsController : BaseAdminController
    {
        // GET: Admin/ProductCategories
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public ActionResult Index(int? id, String search = "")
        {
            if (!id.HasValue)
            {
                // Comments are scoped to a product (opened from Products grid); bare URL is not a listing.
                SetErrorMessage("Ürün yorumları bir ürün kaydı üzerinden açılmalıdır.");
                return RedirectToAction("Index", "Products");
            }

            var productComments = ProductCommentService.GetAdminPageList(id.Value, search, CurrentLanguage);
            ViewBag.Product = ProductService.GetSingle(id.Value);
            return View(productComments);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            var productComment = ProductCommentService.GetSingle(id);
            if (productComment == null)
            {
                return HttpNotFound();
            }
            try
            {
                var productId = productComment.ProductId;
                ProductCommentService.DeleteEntity(productComment);
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