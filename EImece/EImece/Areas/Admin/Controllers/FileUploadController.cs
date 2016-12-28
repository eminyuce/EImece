using EImece.Domain.Helpers;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Hosting;
using Ninject;

namespace EImece.Areas.Admin.Controllers
{
    public class FileUploadController : BaseAdminController
    {
        public FilesHelper filesHelper { get; set; }
        String tempPath = "~/media/tempFiles/";
        String serverMapPath = "~/media/images/";
        private string StorageRoot
        {
            get { return Path.Combine(HostingEnvironment.MapPath(serverMapPath)); }
        }
        private string UrlBase = "/media/images/";
        String DeleteURL = "/FileUpload/DeleteFile/?file=";
        String DeleteType = "GET";
        public FileUploadController(FilesHelper fh)
        {
            filesHelper = fh;
            filesHelper.Init(DeleteURL, DeleteType, StorageRoot, UrlBase, tempPath, serverMapPath);
    
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Show()
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
        public JsonResult DeleteFile(string file)
        {
            var CurrentContext = HttpContext;
            filesHelper.DeleteFile(file, CurrentContext);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

    }
}