using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Helpers;

namespace EImece.Areas.Admin.Controllers
{
    public class TestController : BaseAdminController
    {
        // GET: Admin/Test
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Index2()
        {
            return View();
        }
        public ActionResult getData(int id=0)
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
            List<Product> products = new List<Product>();
            int pageSize = length.ToInt();
            int skip = start.ToInt();
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
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                //for make sort simpler we will add Syste.Linq.Dynamic reference
                v = v.OrderBy(sortColumn + " " + sortColumnDir);
            }
            else
            {
                v = v.OrderBy(r => r.Id);
            }

            recordsTotal = v.Count();
                products = v.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = products });
        }

        public ActionResult getproducts(int pageNo = 1, int pageSize = 50,  string[] sort = null, string search = null)
        {
            // Determine the number of records to skip
            int skip = (pageNo - 1) * pageSize;

            IQueryable<Product> queryable = ProductRepository.GetAll();

            // Apply the search
            if (!String.IsNullOrEmpty(search))
            {
                string[] searchElements = search.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string searchElement in searchElements)
                {
                    string element = searchElement;
                    queryable = queryable.Where(c => c.Name.Contains(element) || c.ProductCode.Contains(element));
                }
            }

            // Add the sorting
            if (sort != null)
                queryable = queryable.ApplySorting(sort);
            else
                queryable = queryable.OrderBy(c => c.Id);

            // Get the total number of records
            int totalItemCount = queryable.Count();

            // Retrieve the customers for the specified page
            var prodocts = queryable
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // Return the paged results
            var item = new PagedResult<Product>(prodocts, pageNo, pageSize, totalItemCount);
            return Json(item, JsonRequestBehavior.AllowGet);
        }
    }
}