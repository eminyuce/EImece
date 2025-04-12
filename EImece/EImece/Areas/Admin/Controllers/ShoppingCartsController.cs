using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using Resources;
using System;
using System.Net;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ShoppingCartsController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IShoppingCartService ShoppingCartService { get; set; }

        // GET: Admin/ShoppingCarts
        public ActionResult Index(String search = "")
        {
            var items = ShoppingCartService.GetAdminPageList(search, CurrentLanguage);
            return View(items);
        }

        public ActionResult Detail(int id)
        {
            var shoppingCart = ShoppingCartService.GetSingle(id);
            if (shoppingCart == null)
            {
                return HttpNotFound();
            }
            return View(shoppingCart);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = ShoppingCartService.GetSingle(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            try
            {
                ShoppingCartService.DeleteById(id);
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, item);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }
    }
}