using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ImportDataController : BaseAdminController
    {
        // GET: Admin/ImportData

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

        public ActionResult ExcelUploadImport(HttpPostedFileBase excelFile = null)
        {
            String path = "~/App_Data/";
            var r = new Random();
            string fileName = r.Next(0, 1000) + excelFile.FileName;
            var pathName = Server.MapPath(path + fileName);
            excelFile.SaveAs(pathName);
            return RedirectToAction("DisplayTable", new { id = fileName });
        }

        public ActionResult DisplayTable(String id)
        {
            String path = "~/App_Data/";
            var pathName = Server.MapPath(path + "/" + id);
            DataTable dt = ExcelHelper.Excel_To_DataTable(pathName, 0);
            ViewBag.PathName = id;
            return View(dt);
        }

        [HttpPost]
        public ActionResult DisplayExcel(String pathName, String selectedTable)
        {
            String path = "~/App_Data/";
            pathName = Server.MapPath(path + pathName);

            DataTable dt = ExcelHelper.Excel_To_DataTable(pathName, 0);
            if (selectedTable.Equals("ProductCategories", StringComparison.InvariantCultureIgnoreCase))
            {
                List<ProductCategory> items = dt.ConvertToList<ProductCategory>().Where(r => !String.IsNullOrEmpty(r.Name)).ToList();
                foreach (var item in items)
                {
                    item.Lang = CurrentLanguage;
                    ProductCategoryService.SaveOrEditEntity(item);
                }
            }
            return RedirectToAction("Index");
        }
    }
}