using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MediaController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private FilesHelper filesHelper;

        public MediaController(FilesHelper filesHelper)
        {
            this.filesHelper = filesHelper;
            this.filesHelper.InitFilesMediaFolder();
        }

        // GET: Admin/Media
        public ActionResult Index(int contentId, String mod, String imageType)
        {
            int id = contentId;
            var returnModel = new MediaAdminIndexModel();
            MediaModType? enumMod = EnumHelper.Parse<MediaModType>(mod);
            EImeceImageType? enumImageType = EnumHelper.Parse<EImeceImageType>(imageType);
            returnModel.Id = id;
            returnModel.Lang = GetCurrentLanguage;
            returnModel.ImageType = enumImageType.Value;
            returnModel.MediaMod = enumMod.Value;
            returnModel.FileStorages = FileStorageService.GetUploadImages(contentId, enumMod, enumImageType);
            switch (enumMod)
            {
                case MediaModType.Stories:
                    returnModel.BaseContent = StoryService.GetSingle(id);
                    break;

                case MediaModType.Products:
                    returnModel.BaseContent = ProductService.GetSingle(id);
                    break;

                case MediaModType.Menus:
                    returnModel.BaseContent = MenuService.GetSingle(id);
                    break;

                case MediaModType.MainPageImages:
                    returnModel.BaseContent = MenuService.GetSingle(id);
                    break;

                default:
                    break;
            }

            return View(returnModel);
        }

        public ActionResult Show(int id, String mod, String imageType)
        {
            var CurrentContext = HttpContext;
            JsonFiles ListOfFiles = filesHelper.GetFileList(CurrentContext);
            var model = new FilesViewModel()
            {
                Files = ListOfFiles.files
            };

            return View(model);
        }

        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Upload()
        {
            int Id = Request.Form["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(Request.Form["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(Request.Form["mod"].ToStr());
            int imageHeight = Request.Form["imageHeight"].ToInt();
            int imageWidth = Request.Form["imageWidth"].ToInt();

            string selectedTags = Request.Form["selectedTags"];

            var resultList = new List<ViewDataUploadFilesResult>();

            var CurrentContext = HttpContext;

            filesHelper.UploadAndShowResults(CurrentContext, resultList);
            JsonFiles files = new JsonFiles(resultList);

            bool isEmpty = !resultList.Any();
            if (isEmpty)
            {
                return Json("Error ");
            }
            else
            {
                FileStorageService.SaveUploadImages(Id, imageType, mod, resultList, CurrentLanguage, selectedTags);
                return Json(files);
            }
        }

        public JsonResult GetFileList()
        {
            var CurrentContext = HttpContext;
            var list = filesHelper.GetFileList(CurrentContext);

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [DeleteAuthorize()]
        public JsonResult DeleteFile(string file, int contentId, String mod, String imageType)
        {
            var CurrentContext = HttpContext;
            filesHelper.DeleteFile(file, CurrentContext);

            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var fileStorage = FileStorageService.GetSingle(id);
            if (fileStorage == null)
            {
                return HttpNotFound();
            }
            try
            {
                FileStorageService.DeleteFileStorage(id);
                return ReturnIndexIfNotUrlReferrer("Index", new { id = product.ProductCategoryId });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete fileStorage:" + ex.StackTrace, fileStorage);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }
    }
}