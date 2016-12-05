using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;

namespace EImece.Areas.Admin.Controllers
{
    public class TestController : BaseAdminController
    {
        // GET: Admin/Test
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult getData()
        {
            //Datatable parameter
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //paging parameter
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //sorting parameter
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //filter parameter
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            List<Product> allCustomer = new List<Product>();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            //Database query
            var v = ProductRepository.GetAll();
                //search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    v = v.Where(a =>
                        a.ProductCode.Contains(searchValue) ||
                        a.Name.Contains(searchValue) 
                        );
                }

            //sort
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                //for make sort simpler we will add Syste.Linq.Dynamic reference
                v = v.OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count();
                allCustomer = v.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = allCustomer });
        }
    }
}