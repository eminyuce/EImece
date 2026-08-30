using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Resources;

using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class ImportDataController : BaseAdminController
    {
        protected IProductCategoryService ProductCategoryService { get; }

        public ImportDataController(
            ISettingService settingService,
            IProductCategoryService productCategoryService)
            : base(settingService)
        {
            ProductCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
        }
        public ActionResult Index()
        {
            String path = "~/App_Data/";
            var pathName = Server.MapPath(path);
            var files = Directory.GetFiles(pathName).ToList();
            ViewBag.PathName = pathName;
            return View(files);
        }

        public ActionResult ExcelUpload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExcelUploadImport(HttpPostedFileBase excelFile = null)
        {
            if (excelFile == null || excelFile.ContentLength <= 0)
            {
                ModelState.AddModelError("", AdminResource.ExcelFileRequired);
                return View("ExcelUpload");
            }

            String path = "~/App_Data/";
            var root = Server.MapPath(path);
            string originalName = Path.GetFileName(excelFile.FileName);
            if (string.IsNullOrWhiteSpace(originalName) || !IsAllowedExcelExtension(originalName))
            {
                ModelState.AddModelError("", AdminResource.ExcelFileAllowedOnly);
                return View("ExcelUpload");
            }

            string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(originalName);
            var pathName = SecurityHelper.GetSafeStorageFilePath(root, fileName);
            excelFile.SaveAs(pathName);
            return RedirectToAction("DisplayTable", new { id = fileName });
        }

        public ActionResult DisplayTable(String id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            String path = "~/App_Data/";
            var root = Server.MapPath(path);
            var pathName = SecurityHelper.GetSafeStorageFilePath(root, id);
            DataTable dt = ExcelHelper.Excel_To_DataTable(pathName, 0);
            ViewBag.PathName = Path.GetFileName(id);
            return View(dt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisplayExcel(String pathName, String selectedTable)
        {
            String path = "~/App_Data/";
            var root = Server.MapPath(path);
            pathName = SecurityHelper.GetSafeStorageFilePath(root, pathName);

            DataTable dt = ExcelHelper.Excel_To_DataTable(pathName, 0);
            if (selectedTable.Equals("ProductCategories", StringComparison.InvariantCultureIgnoreCase))
            {
                List<ProductCategory> items = dt.ConvertToList<ProductCategory>().Where(r => !String.IsNullOrEmpty(r.Name)).ToList();
                foreach (var item in items)
                {
                    item.Lang = CurrentLanguage;
                    await ProductCategoryService.SaveOrEditEntityAsync(item);
                }
            }
            return RedirectToAction("Index");
        }

        private static bool IsAllowedExcelExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            return string.Equals(ext, ".xls", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase);
        }
    }
}
