using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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
            return View(dt);
        }

        public ActionResult DisplayExcel(String id, String selectedTable)
        {
            String path = "~/App_Data/";
            var pathName = Server.MapPath(path + id);

            DataTable dt = ExcelHelper.Excel_To_DataTable(pathName, 0);
            if (selectedTable.Equals("ProductCategories"))
            {
                List<ProductCategory> companyList = dt.ConvertToList<ProductCategory>();

            }
            return RedirectToAction("Index");
        }
    }
}