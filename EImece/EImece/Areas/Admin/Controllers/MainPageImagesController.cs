using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using System.Linq.Expressions;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using NLog;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain;

namespace EImece.Areas.Admin.Controllers
{
    public class MainPageImagesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // GET: Admin/MainPageImages
        public ActionResult Index(String search="")
        {
            Expression<Func<MainPageImage, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var mainPageImages = MainPageImageService.SearchEntities(whereLambda, search);
            return View(mainPageImages);
        }

        // GET: Admin/MainPageImages/Details/5
        public ActionResult Details(int id=0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MainPageImage mainPageImage = MainPageImageService.GetSingle(id);
            if (mainPageImage == null)
            {
                return HttpNotFound();
            }
            return View(mainPageImage);
        }

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = MainPageImage.GetInstance<MainPageImage>();


            if (id == 0)
            {

            }
            else
            {
                content = MainPageImageService.GetSingle(id);
            }


            return View(content);
        }

        //
        // POST: /StoryCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(MainPageImage mainpageimage, HttpPostedFileBase mainPageImageFile = null)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    if (mainPageImageFile != null)
                    {
                        var mainImage = FilesHelper.SaveFileFromHttpPostedFileBase(mainPageImageFile, 0, 0, EImeceImageType.MainPageImages);
                        FileStorageService.SaveOrEditEntity(mainImage);
                        mainpageimage.MainImageId = mainImage.Id;
                    }
                    mainpageimage.ImageState = true;
                    MainPageImageService.SaveOrEditEntity(mainpageimage);
                    int contentId = mainpageimage.Id;
                    return RedirectToAction("Index");
                }
                else
                {
               
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, mainpageimage);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.Message);
            }

            return View(mainpageimage);
        }


        [AuthorizeRoles(Settings.AdministratorRole)]
        public ActionResult Delete(int id=0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MainPageImage mainPageImage = MainPageImageService.GetSingle(id);
            if (mainPageImage == null)
            {
                return HttpNotFound();
            }
            return View(mainPageImage);
        }

        // POST: Admin/MainPageImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Settings.AdministratorRole)]
        public ActionResult DeleteConfirmed(int id=0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

  
            MainPageImageService.DeleteMainPageImage(id);
            return RedirectToAction("Index");
        }
 
    }
}
