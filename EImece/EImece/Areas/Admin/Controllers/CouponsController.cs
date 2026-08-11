using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using Resources;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class CouponsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Coupon, bool>> whereLambda = r => r.Name.Contains(search) || r.Code.Contains(search);
            var result = await CouponService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(result);
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Coupon>();
            if (id == 0)
            {
            }
            else
            {
                content = await CouponService.GetSingleAsync(id);
                content.StartDateStr = content.StartDate.ToString("dd/MM/yyyy",
                                CultureInfo.InvariantCulture);
                content.EndDateStr = content.EndDate.ToString("dd/MM/yyyy",
                                CultureInfo.InvariantCulture);
            }

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(Coupon coupon, String saveButton = null)
        {
            try
            {
                if (coupon == null)
                {
                    return HttpNotFound();
                }

                coupon.StartDate = coupon.StartDateStr.ToDateTime();
                coupon.EndDate = coupon.EndDateStr.ToDateTime();
                if (coupon.EndDate > coupon.StartDate)
                {
                    coupon.Lang = CurrentLanguage;
                    await CouponService.SaveOrEditEntityAsync(coupon);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                }
                else
                {
                    ModelState.AddModelError("EndDateStr", AdminResource.EndDateBiggerThanStartDateText);
                    return View(coupon);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, coupon);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            RemoveModelState();
            return View(coupon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            var item = await CouponService.GetSingleAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            try
            {
                await CouponService.DeleteEntityAsync(item);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, item);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format);
        }

        private async Task<ActionResult> DownloadFileAsync(string format = "excel")
        {
            String search = "";
            Expression<Func<Coupon, bool>> whereLambda = r => r.Name.Contains(search) || r.Code.Contains(search);
            var Coupons = await CouponService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);

            var result = from r in Coupons
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("Coupons-{0}", GetCurrentLanguage), format);
        }
    }
}
