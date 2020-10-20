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
        public ActionResult Index(int id, String search = "")
        {
            var productComments = ProductCommentService.GetAdminPageList(id, search, CurrentLanguage);
            ViewBag.Product = ProductService.GetSingle(id);
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
                return RedirectToAction("Index", new { id = productId });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, productComment);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }
    }
}