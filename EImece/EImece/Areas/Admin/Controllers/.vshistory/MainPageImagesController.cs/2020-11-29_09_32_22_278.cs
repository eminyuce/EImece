using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MainPageImagesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // GET: Admin/MainPageImages
        public ActionResult Index(String search = "")
        {
            Expression<Func<MainPageImage, bool>> whereLambda = r => r.Name.Contains(search);
            var mainPageImages = MainPageImageService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(mainPageImages);
        }

        public ActionResult SaveOrEdit(int id = 0)
        {
         

            var content = EntityFactory.GetBaseContentInstance<MainPageImage>();
            
            if (id == 0)
            {
            }
            else
            {
                content = MainPageImageService.GetBaseContent(id);
            }

            return View(content);
        }

        //
        // POST: /StoryCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(MainPageImage mainpageimage, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (mainpageimage == null)
                {
                    return HttpNotFound();
                }
                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(
                      postedImage,
                      mainpageimage.ImageHeight,
                      mainpageimage.ImageWidth,
                      EImeceImageType.MainPageImages,
                      mainpageimage);

                    mainpageimage.ImageState = true;
                    MainPageImageService.SaveOrEditEntity(mainpageimage);
                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, mainpageimage);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            }
            RemoveModelState();
            return View(mainpageimage);
        }

        // POST: Admin/MainPageImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                MainPageImageService.DeleteMainPageImage(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete MainPageImages:" + ex.StackTrace, id);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }
    }
}