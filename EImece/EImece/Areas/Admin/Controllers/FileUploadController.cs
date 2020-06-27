using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.HelperModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class FileUploadController : BaseAdminController
    {
        public FilesHelper filesHelper { get; set; }

        public FileUploadController(FilesHelper fh)
        {
            filesHelper = fh;
            filesHelper.InitFilesMediaFolder(Constants.FileUploadDeleteURL);
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