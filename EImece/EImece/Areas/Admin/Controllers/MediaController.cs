using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.HelperModels;
using System.IO;
using System.Web.Hosting;
using Ninject;
using EImece.Domain;
using NLog;
using EImece.Domain.Helpers.AttributeHelper;

namespace EImece.Areas.Admin.Controllers
{
    public class MediaController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public FilesHelper filesHelper;
       
        public MediaController(FilesHelper fh)
        {
            filesHelper = fh;
            filesHelper.Init(Settings.DeleteURL, Settings.DeleteType, Settings.StorageRoot, Settings.UrlBase, Settings.TempPath, Settings.ServerMapPath);
        }


        // GET: Admin/Media
        public ActionResult Index(int contentId, String mod, String imageType)
        {
            int id = contentId;
            var returnModel = new MediaAdminIndexModel();
            MediaModType? enumMod = EnumHelper.Parse<MediaModType>(mod);
            EImeceImageType? enumImageType = EnumHelper.Parse<EImeceImageType>(imageType);
            returnModel.Id = id;
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
                FileStorageService.SaveUploadImages(Id, imageType, mod, resultList);
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
        [AuthorizeRoles(Settings.AdministratorRole)]
        public JsonResult DeleteFile(string file, int contentId, String mod, String imageType)
        {
            var CurrentContext = HttpContext;
            filesHelper.DeleteFile(file, CurrentContext);

            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}