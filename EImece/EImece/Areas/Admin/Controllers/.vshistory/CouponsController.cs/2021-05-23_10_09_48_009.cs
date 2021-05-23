using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using Microsoft.AspNet.Identity;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Globalization;

namespace EImece.Areas.Admin.Controllers
{
    public class CouponsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            Expression<Func<Coupon, bool>> whereLambda = r => r.Name.Contains(search) || r.Code.Contains(search);
            var result = CouponService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(result);
        }

        //
        // GET: /Tag/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Coupon>();
            if (id == 0)
            {
            }
            else
            {
                content = CouponService.GetSingle(id);
                content.StartDateStr = content.StartDate.ToString("dd/MM/yyyy",
                                CultureInfo.InvariantCulture);
                content.EndDateStr = content.EndDate.ToString("dd/MM/yyyy",
                                CultureInfo.InvariantCulture);
            }

            return View(content);
        }

        //
        // POST: /Tag/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Coupon coupon)
        {
            try
            {
                if (coupon == null)
                {
                    return HttpNotFound();
                }

                coupon.StartDate = coupon.StartDateStr.ToDateTime();
                coupon.EndDate = coupon.EndDateStr.ToDateTime();
                coupon.Lang = CurrentLanguage;
                CouponService.SaveOrEditEntity(coupon);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, coupon);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            return View(coupon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = CouponService.GetSingle(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            try
            {
                CouponService.DeleteEntity(item);
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, item);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync()
        {
            return await Task.Run(() =>
            {
                return DownloadFile();
            }).ConfigureAwait(true);
        }

        private ActionResult DownloadFile()
        {
            String search = "";
            Expression<Func<Coupon, bool>> whereLambda = r => r.Name.Contains(search) || r.Code.Contains(search);
            var Coupons = CouponService.SearchEntities(whereLambda, search, CurrentLanguage);

            var result = from r in Coupons
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("Coupons-{0}", GetCurrentLanguage));
        }
    }
}