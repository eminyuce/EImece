using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class BrandsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            var brands = BrandService.GetAdminPageList(search, CurrentLanguage);
            return View(brands);
        }

        //
        // GET: /Brand/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            TempData[Constants.TempDataReturnUrlReferrer] = Request.UrlReferrer.ToStr();
            var content = EntityFactory.GetBaseContentInstance<Brand>();

            if (id == 0)
            {
            }
            else
            {
                content = BrandService.GetBaseContent(id);
            }

            return View(content);
        }

        //
        // POST: /Brand/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Brand brand, int[] tags = null, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (brand == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                        brand.ImageHeight,
                        brand.ImageWidth,
                        EImeceImageType.BrandMainImage, brand);

                    brand.Lang = CurrentLanguage;
                    brand = BrandService.SaveOrEditEntity(brand);


                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, brand);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.StackTrace);
            }
            if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            }
            RemoveModelState();

            return View(brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            Brand Brand = BrandService.GetBaseContent(id);
            if (Brand == null)
            {
                return HttpNotFound();
            }
            try
            {
                BrandService.DeleteBrandById(id);
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Brand);
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
            var brands = BrandService.GetAdminPageList(search, CurrentLanguage);

            var result = from r in brands
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             Description = r.Description,
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("brands-{0}", GetCurrentLanguage));
        }
    }
}